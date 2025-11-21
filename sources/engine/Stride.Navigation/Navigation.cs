// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Security;
using DotRecast.Detour;
using Stride.Core.Mathematics;

namespace Stride.Navigation
{
    public struct RaycastQuery
    {
        public Vector3 Source;
        public Vector3 Target;
        public Vector3 FindNearestPolyExtent;
        public int MaxPathPoints;
    }
    
    public struct PathFindQuery
    {
        public Vector3 Source;
        public Vector3 Target;
        public Vector3 FindNearestPolyExtent;
        public int MaxPathPoints;
    }

    public struct PathFindResult
    {
        public bool PathFound;

        /// <summary>
        /// Should point to a preallocated array of <see cref="Vector3"/>'s matching the amount in <see cref="PathFindQuery.MaxPathPoints"/>
        /// </summary>
        public IntPtr PathPoints;

        public int NumPathPoints;
    }
    
    internal class Navigation
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct TileHeader
        {
            public int Magic;
            public int Version;
            public int X;
            public int Y;
            public int Layer;
            public uint UserId;
            public int PolyCount;
            public int VertCount;
            public int MaxLinkCount;
            public int DetailMeshCount;
            public int DetailVertCount;
            public int DetailTriCount;
            public int BvNodeCount;
            public int OffMeshConCount;
            public int OffMeshBase;
            public float WalkableHeight;
            public float WalkableRadius;
            public float WalkableClimb;
            public fixed float Bmin[3];
            public fixed float Bmax[3];
            public float BvQuantFactor;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct Poly
        {
            public uint FirstLink;
            public fixed ushort Vertices[6];
            public fixed ushort Neighbours[6];
            public ushort Flags;
            public byte VertexCount;
            public byte AreaAndType;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct BuildSettings
        {
            public BoundingBox BoundingBox;
            public float CellHeight;
            public float CellSize;
            public int TileSize;
            public Point TilePosition;
            public int RegionMinArea;
            public int RegionMergeArea;
            public float EdgeMaxLen;
            public float EdgeMaxError;
            public float DetailSampleDist;
            public float DetailSampleMaxError;
            public float AgentHeight;
            public float AgentRadius;
            public float AgentMaxClimb;
            public float AgentMaxSlope;
        }

        public struct GeneratedData
        {
            public bool Success;
            public IntPtr NavmeshVertices;
            public int NumNavmeshVertices;
            public DtMeshData NavmeshData;
            public int NavmeshDataLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct RaycastResult
        {
            public bool Hit;
            public Vector3 Position;
            public Vector3 Normal;
        }

        // Navmesh generation API

        /// <summary>
        /// Creates a new navigation mesh object.
        /// You must add tiles to it with AddTile before you can perform navigation queries using Query
        /// </summary>
        /// <returns></returns>
        public static NavigationMesh CreateNavmesh(float cellTileSize)
        {
            NavigationMesh navmesh = new NavigationMesh();
            if (!navmesh.Init(cellTileSize))
            {
                navmesh = null;
            }
            return navmesh;
        }

        public static int DtAlign4(int size)
        {
            return (size + 3) & ~3;
        }
    }
}
