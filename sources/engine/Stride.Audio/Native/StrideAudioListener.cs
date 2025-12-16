// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.OpenAL;
using Stride.Core.Mathematics;

namespace Stride.Audio;

public unsafe class StrideAudioListener
{
    public StrideAudioDevice Device { get; set; }
    public Context* Context { get; set; } //OPENAL, TODO REFACTOR THIS 
    public List<StrideAudioSource> Sources { get; set; } = [];
    public Dictionary<uint, StrideAudioBuffer> Buffers { get; set; } = [];

    //XAudio2
    public X3DAUDIO_LISTENER XAudioListener;
    public Matrix WorldTransform;
}

public unsafe struct X3DAUDIO_LISTENER
{
    public Vector3 OrientFront; // orientation of front direction, used only for matrix and delay calculations or listeners with cones for matrix, LPF (both direct and reverb paths), and reverb calculations, must be normalized when used
    public Vector3 OrientTop;   // orientation of top direction, used only for matrix and delay calculations, must be orthonormal with OrientFront when used

    public Vector3 Position; // position in user-defined world units, does not affect Velocity
    public Vector3 Velocity; // velocity vector in user-defined world units/second, used only for doppler calculations, does not affect Position

    X3DAUDIO_CONE* pCone; // sound cone, used only for matrix, LPF (both direct and reverb paths), and reverb calculations, NULL specifies omnidirectionality
}
public struct X3DAUDIO_CONE
{
    public float InnerAngle; // inner cone angle in radians, must be within [0.0f, X3DAUDIO_2PI]
    public float OuterAngle; // outer cone angle in radians, must be within [InnerAngle, X3DAUDIO_2PI]

    public float InnerVolume; // volume level scaler on/within inner cone, used only for matrix calculations, must be within [0.0f, 2.0f] when used
    public float OuterVolume; // volume level scaler on/beyond outer cone, used only for matrix calculations, must be within [0.0f, 2.0f] when used
    public float InnerLPF;    // LPF (both direct and reverb paths) coefficient subtrahend on/within inner cone, used only for LPF (both direct and reverb paths) calculations, must be within [0.0f, 1.0f] when used
    public float OuterLPF;    // LPF (both direct and reverb paths) coefficient subtrahend on/beyond outer cone, used only for LPF (both direct and reverb paths) calculations, must be within [0.0f, 1.0f] when used
    public float InnerReverb; // reverb send level scaler on/within inner cone, used only for reverb calculations, must be within [0.0f, 2.0f] when used
    public float OuterReverb; // reverb send level scaler on/beyond outer cone, used only for reverb calculations, must be within [0.0f, 2.0f] when used
}
