// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using Silk.NET.OpenAL;
using Silk.NET.XAudio;

namespace Stride.Audio;

public unsafe class StrideAudioDevice
{
    public Device* Value { get; set; } //OPENAL, TODO: REFACTOR THIS 
    public SpinLock DeviceLock { get; set; } = new();
    public List<StrideAudioListener> Listeners { get; set; } = [];
    //XAudio2
    public bool Hrtf { get; set; }
    public IXAudio2* xAudio;
    public IXAudio2MasteringVoice* masteringVoice;
    public IntPtr x3_audio;
}
