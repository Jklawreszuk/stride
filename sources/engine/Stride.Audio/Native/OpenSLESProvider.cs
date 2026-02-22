// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using Stride.Core.Mathematics;

namespace Stride.Audio;

internal class OpenSLESProvider : IAudioProvider
{
    public StrideAudioBuffer BufferCreate(int maxBufferSize)
    {
        throw new System.NotImplementedException();
    }

    public void BufferDestroy(StrideAudioBuffer buffer)
    {
        throw new System.NotImplementedException();
    }

    public void BufferFill(StrideAudioBuffer buffer, nint pcm, int bufferSize, int sampleRate, bool mono)
    {
        throw new System.NotImplementedException();
    }

    public StrideAudioDevice Create(string deviceName, DeviceFlags flags)
    {
        throw new System.NotImplementedException();
    }

    public void Destroy(StrideAudioDevice device)
    {
        throw new System.NotImplementedException();
    }

    public StrideAudioListener ListenerCreate(StrideAudioDevice device)
    {
        throw new System.NotImplementedException();
    }

    public void ListenerDestroy(StrideAudioListener listener)
    {
        throw new System.NotImplementedException();
    }

    public void ListenerDisable(StrideAudioListener listener)
    {
        throw new System.NotImplementedException();
    }

    public bool ListenerEnable(StrideAudioListener listener)
    {
        throw new System.NotImplementedException();
    }

    public void ListenerPush3D(StrideAudioListener listener, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
        throw new System.NotImplementedException();
    }

    public void SetMasterVolume(StrideAudioDevice device, float volume)
    {
        throw new System.NotImplementedException();
    }

    public StrideAudioSource SourceCreate(StrideAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor, HrtfEnvironment environment)
    {
        throw new System.NotImplementedException();
    }

    public void SourceDestroy(StrideAudioSource source)
    {
        throw new System.NotImplementedException();
    }

    public void SourceFlushBuffers(StrideAudioSource source)
    {
        throw new System.NotImplementedException();
    }

    public StrideAudioBuffer SourceGetFreeBuffer(StrideAudioSource source)
    {
        throw new System.NotImplementedException();
    }

    public double SourceGetPosition(StrideAudioSource source)
    {
        throw new System.NotImplementedException();
    }

    public bool SourceIsPlaying(StrideAudioSource source)
    {
        throw new System.NotImplementedException();
    }

    public void SourcePause(StrideAudioSource source)
    {
        throw new System.NotImplementedException();
    }

    public void SourcePlay(StrideAudioSource source)
    {
        throw new System.NotImplementedException();
    }

    public void SourcePush3D(StrideAudioSource source, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
        throw new System.NotImplementedException();
    }

    public void SourceQueueBuffer(StrideAudioSource source, StrideAudioBuffer buffer, nint pcm, int bufferSize, BufferType streamType)
    {
        throw new System.NotImplementedException();
    }

    public void SourceSetBuffer(StrideAudioSource source, StrideAudioBuffer buffer)
    {
        throw new System.NotImplementedException();
    }

    public void SourceSetGain(StrideAudioSource source, float gain)
    {
        throw new System.NotImplementedException();
    }

    public void SourceSetLooping(StrideAudioSource source, bool looped)
    {
        throw new System.NotImplementedException();
    }

    public void SourceSetPan(StrideAudioSource source, float pan)
    {
        throw new System.NotImplementedException();
    }

    public void SourceSetPitch(StrideAudioSource source, float pitch)
    {
        throw new System.NotImplementedException();
    }

    public void SourceSetRange(StrideAudioSource source, double startTime, double stopTime)
    {
        throw new System.NotImplementedException();
    }

    public void SourceStop(StrideAudioSource source)
    {
        throw new System.NotImplementedException();
    }

    public void Update(StrideAudioDevice device)
    {
        throw new System.NotImplementedException();
    }
}