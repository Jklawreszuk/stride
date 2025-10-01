// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Recast;
using Stride.Core.Mathematics;
using static Stride.Navigation.Navigation;

namespace Stride.Navigation;

internal class NavigationBuilder
{
    private BuildSettings _buildSettings;
    private GeneratedData _result = new();
    private byte[] _triareas;
    private RcHeightfield _solid;
    private RcContext _context;
    private RcCompactHeightfield _chf;
    private RcContourSet _cset;
    private RcPolyMesh _pmesh;
    private RcPolyMeshDetail _dmesh;
    private DtMeshData _navmeshData;
    private int _navmeshDataLength;

    public NavigationBuilder()
    {
        _context = new RcContext();
    }

    public GeneratedData BuildNavmesh(ref Vector3[] vertices, int numVertices, ref int[] indices, int numIndices)
    {
        GeneratedData ret = _result;
        ret.Success = false;

        RcVec3f bmin = new (_buildSettings.BoundingBox.Minimum.X,_buildSettings.BoundingBox.Minimum.Y,_buildSettings.BoundingBox.Minimum.Z);
        RcVec3f bmax = new (_buildSettings.BoundingBox.Maximum.X,_buildSettings.BoundingBox.Maximum.Y,_buildSettings.BoundingBox.Maximum.Z);

        RcVec3f bbSize = bmax - bmin;

        if (bbSize.X <= 0.0f || bbSize.Y <= 0.0f || bbSize.Z <= 0.0f)
            return ret; // Negative or empty bounding box
        
        if (_buildSettings.DetailSampleDist < 1.0f)
            return ret;
        if (_buildSettings.DetailSampleMaxError <= 0.0f)
            return ret;
        if (_buildSettings.EdgeMaxError < 0.1f)
            return ret;
        if (_buildSettings.EdgeMaxLen < 0.0f)
            return ret;
        if (_buildSettings.RegionMinArea < 0.0f)
            return ret;
        if (_buildSettings.RegionMergeArea < 0.0f)
            return ret;
        if (_buildSettings.TileSize <= 0)
            return ret;
        
        if (_buildSettings.CellSize < 0.01f)
            _buildSettings.CellSize = 0.01f;
        if (_buildSettings.CellHeight < 0.01f)
            _buildSettings.CellHeight = 0.01f;

        int maxEdgeLen = (int)(_buildSettings.EdgeMaxLen / _buildSettings.CellSize);
        float maxSimplificationError = _buildSettings.EdgeMaxError;
        int maxVertsPerPoly = 6;
        float detailSampleDist = _buildSettings.CellSize * _buildSettings.DetailSampleDist;
        float detailSampleMaxError = _buildSettings.CellHeight * _buildSettings.DetailSampleMaxError;

        int walkableHeight = (int)MathF.Ceiling(_buildSettings.AgentHeight / _buildSettings.CellHeight);
        int walkableClimb = (int)MathF.Floor(_buildSettings.AgentMaxClimb / _buildSettings.CellHeight);
        int walkableRadius = (int)MathF.Ceiling(_buildSettings.AgentRadius / _buildSettings.CellSize);

        // Size of the tile border
        int borderSize = walkableRadius + 3;
        int tileSize = _buildSettings.TileSize;

        // Expand bounding box by border size so that all required geometry is included
        bmin.X -= borderSize * _buildSettings.CellSize;
        bmin.Z -= borderSize * _buildSettings.CellSize;
        bmax.X += borderSize * _buildSettings.CellSize;
        bmax.Z += borderSize * _buildSettings.CellSize;

        int width = tileSize + borderSize * 2;
        int height = tileSize + borderSize * 2;

        // Make sure state is clean
        Cleanup();
        
        if (numIndices == 0 || numVertices == 0 || walkableClimb < 0)
            return ret;

        _solid = new RcHeightfield(width, height, bmin, bmax, _buildSettings.CellSize, _buildSettings.CellHeight, borderSize);
        
        int numTriangles = numIndices / 3;
        _triareas = new byte[numTriangles];
        
        Span<Vector3> vectorSpan = vertices.AsSpan();
        Span<float> floatSpan = MemoryMarshal.Cast<Vector3, float>(vectorSpan);
        
        // Find walkable triangles and rasterize into heightfield
        RcCommons.MarkWalkableTriangles(_context, _buildSettings.AgentMaxSlope, floatSpan.ToArray(), indices, numTriangles, new());
        RcRasterizations.RasterizeTriangles(_context,  floatSpan.ToArray(), indices, _triareas.Select(t => (int)t).ToArray(), numTriangles, _solid, walkableClimb);
        
        // Filter walkable surfaces.
        RcFilters.FilterLowHangingWalkableObstacles(_context, walkableClimb, _solid);
        RcFilters.FilterLedgeSpans(_context, walkableHeight, walkableClimb, _solid);
        RcFilters.FilterWalkableLowHeightSpans(_context, walkableHeight, _solid);
        
        // Compact the heightfield so that it is faster to handle from now on.
        // This will result more cache coherent data as well as the neighbours
        // between walkable cells will be calculated.
        _chf = RcCompacts.BuildCompactHeightfield(_context, walkableHeight, walkableClimb, _solid);
        
        // No longer need solid heightfield after compacting it
        //rcFreeHeightField(m_solid);
        _solid = null;

        // Erode the walkable area by agent radius.
        RcAreas.ErodeWalkableArea(_context, walkableRadius, _chf);
            
        // Prepare for region partitioning, by calculating distance field along the walkable surface.
        RcRegions.BuildDistanceField(_context, _chf);
        
        // Partition the walkable surface into simple regions without holes.
        RcRegions.BuildRegions(_context, _chf, _buildSettings.RegionMinArea, _buildSettings.RegionMergeArea);

        // Create contours.
        _cset = RcContours.BuildContours(_context, _chf, maxSimplificationError, maxEdgeLen, 0);

        // Build polygon navmesh from the contours.
        _pmesh = RcMeshs.BuildPolyMesh(_context, _cset, maxVertsPerPoly);
        
        // Free intermediate results
        _cset = null;

        _dmesh = RcMeshDetails.BuildPolyMeshDetail(_context, _pmesh, _chf, detailSampleDist, detailSampleMaxError);
        if (_dmesh == null)
            return ret;

        // Free intermediate results
        _chf = null;

        // Update poly flags from areas.
        for (int i = 0; i < _pmesh.npolys; ++i)
        {
            if (_pmesh.areas[i] ==  RcConstants.RC_WALKABLE_AREA)
                _pmesh.areas[i] = 0;

            if (_pmesh.areas[i] == 0)
            {
                _pmesh.flags[i] = 1;
            }
        }

        // Generate native navmesh format and store the data pointers in the return structure
        if (!CreateDetourMesh())
            return ret;
        ret.NavmeshData = _navmeshData;
        ret.NavmeshDataLength = _navmeshDataLength;
        ret.Success = true;

        return ret;
    }

    private bool CreateDetourMesh()
    {
        DtNavMeshCreateParams createParams = new DtNavMeshCreateParams();
        createParams.verts = _pmesh.verts;
        createParams.vertCount = _pmesh.nverts;
        createParams.polys = _pmesh.polys;
        createParams.polyAreas = _pmesh.areas;
        createParams.polyFlags = _pmesh.flags;
        createParams.polyCount = _pmesh.npolys;
        createParams.nvp = _pmesh.nvp;
        createParams.detailMeshes = _dmesh.meshes;
        createParams.detailVerts = _dmesh.verts;
        createParams.detailVertsCount = _dmesh.nverts;
        createParams.detailTris = _dmesh.tris;
        createParams.detailTriCount = _dmesh.ntris;
        // TODO: Support off-mesh connections
        createParams.offMeshConVerts = null;
        createParams.offMeshConRad = null;
        createParams.offMeshConDir = null;
        createParams.offMeshConAreas = null;
        createParams.offMeshConFlags = null;
        createParams.offMeshConUserID = null;
        createParams.offMeshConCount = 0;
        createParams.walkableHeight = _buildSettings.AgentHeight;
        createParams.walkableClimb = _buildSettings.AgentMaxClimb;
        createParams.walkableRadius = _buildSettings.AgentRadius;
        createParams.bmin = _pmesh.bmin;
        createParams.bmax = _pmesh.bmax;
        createParams.cs = _buildSettings.CellSize;
        createParams.ch = _buildSettings.CellHeight;
        createParams.buildBvTree = true;
        createParams.tileX = _buildSettings.TilePosition.X;
        createParams.tileLayer = _buildSettings.TilePosition.Y;

        _navmeshData = DtNavMeshBuilder.CreateNavMeshData(createParams);
        
        return _navmeshDataLength != 0 && _navmeshData != null;
    }

    private void Cleanup()
    {
        if(_navmeshData != null)
        {
            _navmeshData = null;
            _navmeshDataLength = 0;
        }
        _solid = null;
        _triareas = null;
        _chf = null;
        _pmesh = null;
        _dmesh = null;
    }

    public void SetSettings(BuildSettings buildSettings)
    {
        _buildSettings = buildSettings;
    }
}
