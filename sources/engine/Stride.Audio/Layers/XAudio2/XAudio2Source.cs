// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.XAudio;

namespace Stride.Audio;

public unsafe class XAudio2Source : IAudioSource
{
    public IXAudio2MasteringVoice* masteringVoice;
    public IXAudio2SourceVoice* sourceVoice;
    public X3DAUDIO_EMITTER emitter;
    public X3DAUDIO_DSP_SETTINGS dsp_settings;
    public IXAPOHrtfParameters* hrtf_params_;
    public XAudio2Listener Listener;
    public volatile bool Playing;
    public volatile bool Pause;
    public volatile bool Looped;
    public int SampleRate { get; set; }
    public bool Mono { get; set; }
    public bool Streamed { get; set; }
    public volatile float Pitch = 1.0f;
    public volatile float DopplerPitch = 1.0f;

    public SpinLock bufferLock_;
    public SpinLock apply3DLock_;
    public List<XAudio2Buffer> FreeBuffers = [];

    public Buffer* SingleBuffer;

    public volatile int SamplesAtBegin = 0;

}

public class IXAPOHrtfParameters
{
}
