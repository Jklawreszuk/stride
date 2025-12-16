// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Core.Mathematics;

namespace Stride.Audio;

internal interface IAudioProvider
{
    StrideAudioDevice Create(string deviceName, DeviceFlags flags);
    void Destroy(StrideAudioDevice device);
    void Update(StrideAudioDevice device);
    void SetMasterVolume(StrideAudioDevice device, float volume);
    StrideAudioListener ListenerCreate(StrideAudioDevice device);
    void ListenerDestroy(StrideAudioListener listener);
    bool ListenerEnable(StrideAudioListener listener);
    void ListenerDisable(StrideAudioListener listener);
    StrideAudioSource SourceCreate(StrideAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor, HrtfEnvironment environment);
    void SourceDestroy(StrideAudioSource source);
    double SourceGetPosition(StrideAudioSource source);
    void SourceSetPan(StrideAudioSource source, float pan);
    StrideAudioBuffer BufferCreate(int maxBufferSize);
    void BufferDestroy(StrideAudioBuffer buffer);
    void BufferFill(StrideAudioBuffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono);
    void SourceSetBuffer(StrideAudioSource source, StrideAudioBuffer buffer);
    void SourceFlushBuffers(StrideAudioSource source);
    void SourceQueueBuffer(StrideAudioSource source, StrideAudioBuffer buffer, IntPtr pcm, int bufferSize, BufferType streamType);
    StrideAudioBuffer SourceGetFreeBuffer(StrideAudioSource source);
    void SourcePlay(StrideAudioSource source);
    void SourcePause(StrideAudioSource source);
    void SourceStop(StrideAudioSource source);
    void SourceSetLooping(StrideAudioSource source, bool looped);
    void SourceSetRange(StrideAudioSource source, double startTime, double stopTime);
    void SourceSetGain(StrideAudioSource source, float gain);
    void SourceSetPitch(StrideAudioSource source, float pitch);
    void ListenerPush3D(StrideAudioListener listener, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform);
    void SourcePush3D(StrideAudioSource source, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform);
    bool SourceIsPlaying(StrideAudioSource source);
}
