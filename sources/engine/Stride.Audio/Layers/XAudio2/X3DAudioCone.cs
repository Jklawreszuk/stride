// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.OpenAL;

namespace Stride.Audio;

public struct X3DaudioCone
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
