// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Runtime.InteropServices;

namespace Stride.Audio;

internal class X3DAudio
{
    [DllImport("XAudio2_9")]
    public static extern int X3DAudioInitialize(uint SpeakerChannelMask, float SpeedOfSound, out nint Instance);
    
    //[DllImport("XAudio2_9")]
    //public static extern unsafe void X3DAudioCalculate(out nint x3_audio, nint listenerPtr, X3DAudioEmitter emitter, Calculate calculate, out X3DAudioDSPSettings dsp_settings);
}
