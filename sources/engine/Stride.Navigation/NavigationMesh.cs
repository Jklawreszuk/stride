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
            return new RcVec3f(v.X, v.Y, v.Z);
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
        
        internal Dictionary<Guid, NavigationMeshLayer> LayersInternal = [];
        private DtNavMesh _navMesh;
        private DtNavMeshQuery _navQuery;
        private readonly List<long> _tileRefs = [];

        /// <summary>
        /// The layers of this navigation mesh, there will be one layer for each enabled group that a navigation mesh is selected to build for
        /// </summary>
        [DataMember]
        public IReadOnlyDictionary<Guid, NavigationMeshLayer> Layers => LayersInternal;

        ~NavigationMesh()
        {
            // Cleanup allocated tiles
            foreach(var tile in _tileRefs)
            {
                _navMesh.RemoveTile(tile);
            }
        }
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

            // Initialize the query object
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

            long tileRef = _navMesh.AddTile(navData, 0, 0);
            if(tileRef != 0)
            {
                _tileRefs.Add(tileRef);
                return true;
            }

            return false;
        }

        public bool RemoveTile(Point coord)
        {
            long tileRef = _navMesh.GetTileRefAt(coord.X, coord.Y, 0);

            if(_navMesh.RemoveTile(tileRef) != 0)
            {
                _tileRefs.Remove(tileRef);
                return true;
            }

            return false;
        }
        
        public void DoPathFindQuery(PathFindQuery query, ref PathFindResult result)
        {
            // Reset result
            result.PathFound = false;

            // Find the starting polygons and point on it to start from
            DtQueryDefaultFilter filter = new DtQueryDefaultFilter();
            DtStatus status = _navQuery.FindNearestPoly(query.Source.AsRcVec3f(), query.FindNearestPolyExtent.AsRcVec3f(), filter, out long startPoly, out RcVec3f startPoint, out _);
            if (status.Failed())
                return;
            status = _navQuery.FindNearestPoly(query.Target.AsRcVec3f(), query.FindNearestPolyExtent.AsRcVec3f(), filter, out long endPoly, out RcVec3f endPoint, out _);
            if (status.Failed())
                return;
            
            List<long> polys = new(query.MaxPathPoints);
            status = _navQuery.FindPath(startPoly, endPoly, startPoint, endPoint, filter, ref polys, new());
            if (status.Failed() || status.IsPartial())
                return;
            
            List<DtStraightPath> straightPath = new(query.MaxPathPoints);
            
            status = _navQuery.FindStraightPath(startPoint, endPoint, 
                polys, ref straightPath, query.MaxPathPoints, 0);
            if (status.Failed())
                return;
            result.PathFound = true;
        }

        public void DoRaycastQuery(RaycastQuery query, out NavigationRaycastResult result)
        {
            // Reset result
            result = new() { Hit = false };
            DtQueryDefaultFilter filter = new DtQueryDefaultFilter();

            DtStatus status = _navQuery.FindNearestPoly(query.Source.AsRcVec3f(), query.FindNearestPolyExtent.AsRcVec3f(), filter, out long startPoly, out _, out _);
            if (status.Failed())
                return;
            
            List<long> polys = new (query.MaxPathPoints);
            var normal = result.Normal.AsRcVec3f();
            status = _navQuery.Raycast(startPoly, query.Source.AsRcVec3f(), 
                query.Target.AsRcVec3f(), filter, 
                out float t, out normal, ref polys);
            
            if (status.Failed())
                return;
            
            result.Hit = true;
            result.Position = Vector3.Lerp(query.Source, query.Target, t);
        }

    }
}
