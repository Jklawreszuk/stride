// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    public static IXAudio2VoiceCallback* CreateCallback()
    {
        var vtbl = (IXAudio2VoiceCallbackVtbl*)Marshal.AllocHGlobal(sizeof(IXAudio2VoiceCallbackVtbl));

        vtbl->OnVoiceProcessingPassStart = null;
        vtbl->OnVoiceProcessingPassEnd   = null;
        vtbl->OnStreamEnd                = &OnStreamEnd;
        vtbl->OnBufferStart              = &OnBufferStart;
        vtbl->OnBufferEnd                = &OnBufferEnd;
        vtbl->OnLoopEnd                  = &OnLoopEnd;
        vtbl->OnVoiceError               = null;

        var cb = (IXAudio2VoiceCallback*)Marshal.AllocHGlobal(sizeof(void*));
        cb->LpVtbl = (void**)vtbl;

        return cb;
    }
   
    [UnmanagedCallersOnly]
    static void OnStreamEnd()
    {
        if (playing_)
        {
            if (streamed_)
            {
                //buffer was flagged as end of stream
                //looping is handled by the streamer, in the top level layer
                xnAudioSourceStop(this);
            }
            else if (!looped_)
            {
                playing_ = false;
                pause_ = false;
            }
        }
    }
    
    [UnmanagedCallersOnly]
    static void OnBufferStart(void* context)
    {
        var buffer = (XAudio2Buffer*)context;
        var source = buffer->Owner;
        
        if (streamed_)
        {
            auto buffer = static_cast<xnAudioBuffer*>(context);

            if (buffer->type_ == BeginOfStream)
            {
                //we need this info to compute position of stream
                XAUDIO2_VOICE_STATE state;
                GetState(&state);

                samplesAtBegin = state.SamplesPlayed;
            }
        }
    }
    
    [UnmanagedCallersOnly]
    static void OnBufferEnd(void* context)
    {
        var buffer = (XAudio2Buffer*)context;
        var source = buffer->Owner;

        if (!source->Streamed)
            return;

        source->Lock();
        source->ReturnBuffer(buffer);
        source->Unlock();
    }
    
    [UnmanagedCallersOnly]
    static void OnLoopEnd(void* context)
    {
        var buffer = (XAudio2Buffer*)context;
        var source = buffer->Owner;
        
        if (!source->Streamed)
        {
            VoiceState state;
            GetState(&state);

            samplesAtBegin = state.SamplesPlayed;
        }
    }
    
    struct IXAudio2VoiceCallbackVtbl
    {
        public delegate* unmanaged<void*, void> OnVoiceProcessingPassStart;
        public delegate* unmanaged<void*, void> OnVoiceProcessingPassEnd;
        public delegate* unmanaged<void> OnStreamEnd;
        public delegate* unmanaged<void*, void> OnBufferStart;
        public delegate* unmanaged<void*, void> OnBufferEnd;
        public delegate* unmanaged<void*, void> OnLoopEnd;
        public delegate* unmanaged<void*, int, void> OnVoiceError;
    }
}

public class IXAPOHrtfParameters
{
}
