// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Stride.Core.Mathematics;

namespace Stride.Audio;

public unsafe struct X3DAudioListener
{
    public Vector3 OrientFront; // orientation of front direction, used only for matrix and delay calculations or listeners with cones for matrix, LPF (both direct and reverb paths), and reverb calculations, must be normalized when used
    public Vector3 OrientTop;   // orientation of top direction, used only for matrix and delay calculations, must be orthonormal with OrientFront when used

    public Vector3 Position; // position in user-defined world units, does not affect Velocity
    public Vector3 Velocity; // velocity vector in user-defined world units/second, used only for doppler calculations, does not affect Position

    X3DaudioCone* pCone; // sound cone, used only for matrix, LPF (both direct and reverb paths), and reverb calculations, NULL specifies omnidirectionality
}
