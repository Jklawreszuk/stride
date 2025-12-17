// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Stride.Core.Mathematics;

namespace Stride.Audio;

internal interface IAudioProvider
{
    IAudioDevice Create(string deviceName, DeviceFlags flags);
    void Destroy(IAudioDevice device);
    void Update(IAudioDevice device);
    void SetMasterVolume(IAudioDevice device, float volume);
    IAudioListener ListenerCreate(IAudioDevice device);
    void ListenerDestroy(IAudioListener listener);
    bool ListenerEnable(IAudioListener listener);
    void ListenerDisable(IAudioListener listener);
    IAudioSource SourceCreate(IAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor, HrtfEnvironment environment);
    void SourceDestroy(IAudioSource source);
    double SourceGetPosition(IAudioSource source);
    void SourceSetPan(IAudioSource source, float pan);
    IAudioBuffer BufferCreate(int maxBufferSize);
    void BufferDestroy(IAudioBuffer buffer);
    void BufferFill(IAudioBuffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono);
    void SourceSetBuffer(IAudioSource source, IAudioBuffer buffer);
    void SourceFlushBuffers(IAudioSource source);
    void SourceQueueBuffer(IAudioSource source, IAudioBuffer buffer, IntPtr pcm, int bufferSize, BufferType streamType);
    IAudioBuffer SourceGetFreeBuffer(IAudioSource source);
    void SourcePlay(IAudioSource source);
    void SourcePause(IAudioSource source);
    void SourceStop(IAudioSource source);
    void SourceSetLooping(IAudioSource source, bool looped);
    void SourceSetRange(IAudioSource source, double startTime, double stopTime);
    void SourceSetGain(IAudioSource source, float gain);
    void SourceSetPitch(IAudioSource source, float pitch);
    void ListenerPush3D(IAudioListener listener, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform);
    void SourcePush3D(IAudioSource source, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform);
    bool SourceIsPlaying(IAudioSource source);
}
