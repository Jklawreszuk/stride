// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.XAudio;

namespace Stride.Audio;

public unsafe class StrideAudioSource
{
    public uint Value;
    public int SampleRate { get; set; }
    public bool Mono { get; set; }
    public bool Streamed { get; set; }
    public float DequeuedTime { get; set; }
    public StrideAudioListener Listener { get; set; }
    public StrideAudioBuffer SingleBuffer { get; set; }
    public List<StrideAudioBuffer> FreeBuffers { get; set; } = [];
    //XAudio2
    public Buffer Buffer;
    public IXAudio2MasteringVoice* masteringVoice;
    public IXAudio2SourceVoice* sourceVoice;
    public X3DAUDIO_EMITTER emitter;
    public X3DAUDIO_DSP_SETTINGS dsp_settings;
    public bool Playing { get; set; }
    public bool Pause { get; set; }
    public bool Looped { get; set; }
    public float Pitch { get; set; }
    public float DopplerPitch { get; set; }
    public int SamplesAtBegin { get; set; }
}
