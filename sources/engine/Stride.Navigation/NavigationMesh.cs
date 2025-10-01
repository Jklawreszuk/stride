// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Core.Serialization;
using Stride.Core.Serialization.Contents;
using Stride.Core.Serialization.Serializers;

namespace Stride.Navigation
{
    internal static class Vector3Extensions
    {
        internal static RcVec3f AsRcVec3f(this Vector3 v)
        {
            return new RcVec3f { X = v.X, Y = v.Y, Z = v.Z };
        }
    }
    /// <summary>
    /// A Navigation Mesh, can be used for pathfinding.
    /// </summary>
    [DataContract("NavigationMesh")]
    [ReferenceSerializer, DataSerializerGlobal(typeof(ReferenceSerializer<NavigationMesh>), Profile = "Content")]
    [DataSerializer(typeof(NavigationMeshSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<NavigationMesh>))]
    public class NavigationMesh
    {
        // Stores the cached build information to allow incremental building on this navigation mesh
        internal NavigationMeshCache Cache;

        internal float TileSize;

        internal float CellSize;
        
        internal Dictionary<Guid, NavigationMeshLayer> LayersInternal = new Dictionary<Guid, NavigationMeshLayer>();
        private DtNavMesh _navMesh;
        private DtNavMeshQuery _navQuery;
        private List<long> _tileRefs = [];

        /// <summary>
        /// The layers of this navigation mesh, there will be one layer for each enabled group that a navigation mesh is selected to build for
        /// </summary>
        [DataMember]
        public IReadOnlyDictionary<Guid, NavigationMeshLayer> Layers => LayersInternal;

        internal class NavigationMeshSerializer : DataSerializer<NavigationMesh>
        {
            private DictionarySerializer<Guid, NavigationMeshLayer> layersSerializer;
            private DataSerializer<NavigationMeshCache> cacheSerializer;

            public override void Initialize(SerializerSelector serializerSelector)
            {
                cacheSerializer = MemberSerializer<NavigationMeshCache>.Create(serializerSelector, false);
                
                layersSerializer = new DictionarySerializer<Guid, NavigationMeshLayer>();
                layersSerializer.Initialize(serializerSelector);
            }

            public override void PreSerialize(ref NavigationMesh obj, ArchiveMode mode, SerializationStream stream)
            {
                base.PreSerialize(ref obj, mode, stream);
                if (mode == ArchiveMode.Deserialize)
                {
                    if (obj == null)
                        obj = new NavigationMesh();
                }
            }

            public override void Serialize(ref NavigationMesh obj, ArchiveMode mode, SerializationStream stream)
            {
                // Serialize tile size because it is needed
                stream.Serialize(ref obj.TileSize);
                stream.Serialize(ref obj.CellSize);

                cacheSerializer.Serialize(ref obj.Cache, mode, stream);
                layersSerializer.Serialize(ref obj.LayersInternal, mode, stream);
            }
        }

        public bool Init(float cellTileSize)
        {
            DtNavMeshParams meshParams = new();
            meshParams.orig.X = 0.0f;
            meshParams.orig.Y = 0.0f;
            meshParams.orig.Z = 0.0f;
            meshParams.tileWidth = cellTileSize;
            meshParams.tileHeight = cellTileSize;

            // TODO: Link these parameters to the builder
            const int tileBits = 14;
            const int polyBits = 22 - tileBits;
            
            meshParams.maxTiles = 1 << tileBits;
            meshParams.maxPolys = 1 << polyBits;

            // Allocate objects
            _navMesh = new DtNavMesh(meshParams, 2048);
            _navQuery = new DtNavMeshQuery(_navMesh);
            
            return true;
        }

        public bool LoadTile(DtMeshData navData)
        {
            if (_navMesh == null || _navQuery == null)
                return false;
            if (navData == null)
                return false;
            
            const long tileRef = 0;
            if(_navMesh.AddTile(navData, 0, tileRef) == DtStatus.DT_SUCCESS.Value)
            {
                _tileRefs.Add(tileRef);
                return true;
            }

            return false;
        }

        public bool RemoveTile(Point coord)
        {
            long tileRef = _navMesh.GetTileRefAt(coord.X, coord.Y, 0);

            long status = _navMesh.RemoveTile(tileRef);
            if(status ==  DtStatus.DT_SUCCESS.Value)
            {
                _tileRefs.Remove(tileRef);
                return true;
            }

            return false;
        }

        public void DoRaycastQuery(RaycastQuery query, ref NavigationRaycastResult result)
        {
            // Reset result
            result.Hit = false;
            IDtQueryFilter filter = null;

            DtStatus status = _navQuery.FindNearestPoly(query.Source.AsRcVec3f(), query.FindNearestPolyExtent.AsRcVec3f(), filter, out long startPoly, out _, out _);
            if (status.Value == DtStatus.DT_FAILURE.Value)
                return;
            
            List<long> polys = new (query.MaxPathPoints);
            int raycastPolyCount = 0;
            var normal = result.Normal.AsRcVec3f();
            status = _navQuery.Raycast(startPoly, query.Source.AsRcVec3f(), 
                query.Target.AsRcVec3f(), filter, 
                out float t, out normal, ref polys);
            if (status.Value == DtStatus.DT_FAILURE.Value)
                return;
            
            result.Hit = true;

            result.Position = Vector3.Lerp(query.Source, query.Target, t);
        }

        public void DoPathFindQuery(PathFindQuery query, ref PathFindResult result)
        {
            // Reset result
            result.PathFound = false;

            // Find the starting polygons and point on it to start from
            IDtQueryFilter filter = null;
            DtStatus status = _navQuery.FindNearestPoly(query.Source.AsRcVec3f(), query.FindNearestPolyExtent.AsRcVec3f(), filter, out long startPoly, out RcVec3f startPoint, out _);
            if (status.Value == DtStatus.DT_FAILURE.Value)
                return;
            status = _navQuery.FindNearestPoly(query.Target.AsRcVec3f(), query.FindNearestPolyExtent.AsRcVec3f(), filter, out long endPoly, out RcVec3f endPoint, out _);
            if (status.Value == DtStatus.DT_FAILURE.Value)
                return;
            
            List<long> polys = new(query.MaxPathPoints);
            int pathPointCount = 0;
            status = _navQuery.FindPath(startPoly, endPoly, startPoint, endPoint, 
                filter, ref polys, new());
            if (status.Value == DtStatus.DT_FAILURE.Value)
                return;
            
            List<Vector3> straightPath = new(query.MaxPathPoints);
            List<DtStraightPath> straightPathFlags = new(query.MaxPathPoints);
            List<long> straightpathPolys = new(query.MaxPathPoints);
            
            status = _navQuery.FindStraightPath(startPoint, endPoint, 
                polys, ref straightPathFlags, query.MaxPathPoints, 0);
            if (status.Value == DtStatus.DT_FAILURE.Value)
                return;
            result.PathFound = true;
        }
    }
}
