// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Stride.Core.Mathematics;

namespace Stride.Audio
{
    /// <summary>
    /// Wrapper around OpenAL
    /// </summary>
    public class AudioLayer
    {
        private static IAudioProvider Provider;

        public static void Init()
        {
            Provider = new XAudio2Provider();
        }

        public static IAudioDevice Create(string deviceName, DeviceFlags flags)
        {
            return Provider.Create(deviceName, flags);
        }

        public static void Destroy(IAudioDevice device)
        {
            Provider.Destroy(device);
        }

        public static void Update(IAudioDevice device)
        {
            Provider.Update(device);
        }

        public static void SetMasterVolume(IAudioDevice device, float volume)
        {
            Provider.SetMasterVolume(device, volume);
        }

        public static IAudioListener ListenerCreate(IAudioDevice device)
        {
            return Provider.ListenerCreate(device);
        }

        public static void ListenerDestroy(IAudioListener listener)
        {
            Provider.ListenerDestroy(listener);
        }

        public static bool ListenerEnable(IAudioListener listener)
        {
            return Provider.ListenerEnable(listener);
        }

        public static void ListenerDisable(IAudioListener listener)
        {
            Provider.ListenerDisable(listener);
        }

        public static IAudioSource SourceCreate(IAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor,
            HrtfEnvironment environment)
        {
            return Provider.SourceCreate(listener, sampleRate, maxNumberOfBuffers, mono, spatialized, streamed, hrtf, hrtfDirectionFactor, environment);
        }

        public static void SourceDestroy(IAudioSource source)
        {
            Provider.SourceDestroy(source);
        }

        public static double SourceGetPosition(IAudioSource source)
        {
            return Provider.SourceGetPosition(source);
        }

        public static void SourceSetPan(IAudioSource source, float pan)
        {
            Provider.SourceSetPan(source, pan);
        }

        public static IAudioBuffer BufferCreate(int maxBufferSizeBytes)
        {
            return Provider.BufferCreate(maxBufferSizeBytes);
        }

        public static void BufferDestroy(IAudioBuffer buffer)
        {
            Provider.BufferDestroy(buffer);
        }

        public static void BufferFill(IAudioBuffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono)
        {
            Provider.BufferFill(buffer, pcm, bufferSize, sampleRate, mono);
        }

        public static void SourceSetBuffer(IAudioSource source, IAudioBuffer buffer)
        {
            Provider.SourceSetBuffer(source, buffer);
        }

        public static void SourceFlushBuffers(IAudioSource source)
        {
            Provider.SourceFlushBuffers(source);
        }

        public static void SourceQueueBuffer(IAudioSource source, IAudioBuffer buffer, IntPtr pcm, int bufferSize, BufferType streamType)
        {
            Provider.SourceQueueBuffer(source, buffer, pcm, bufferSize, streamType);
        }

        public static IAudioBuffer SourceGetFreeBuffer(IAudioSource source)
        {
            return Provider.SourceGetFreeBuffer(source);
        }

        public static void SourcePlay(IAudioSource source)
        {
            Provider.SourcePlay(source);
        }

        public static void SourcePause(IAudioSource source)
        {
            Provider.SourcePause(source);
        }

        public static void SourceStop(IAudioSource source)
        {
            Provider.SourceStop(source);
        }

        public static void SourceSetLooping(IAudioSource source, bool looped)
        {
            Provider.SourceSetLooping(source, looped);
        }

        public static void SourceSetRange(IAudioSource source, double startTime, double stopTime)
        {
            Provider.SourceSetRange(source, startTime, stopTime);
        }

        public static void SourceSetGain(IAudioSource source, float gain)
        {
            Provider.SourceSetGain(source, gain);
        }

        public static void SourceSetPitch(IAudioSource source, float pitch)
        {
            Provider.SourceSetPitch(source, pitch);
        }

        public static void ListenerPush3D(IAudioListener listener, ref Vector3 pos, ref Vector3 forward, ref Vector3 up, ref Vector3 vel, ref Matrix worldTransform)
        {
            Provider.ListenerPush3D(listener, pos, forward, up, vel, worldTransform);
        }

        public static void SourcePush3D(IAudioSource source, ref Vector3 pos, ref Vector3 forward, ref Vector3 up, ref Vector3 vel, ref Matrix worldTransform)
        {
            Provider.SourcePush3D(source, pos, forward, up, vel, worldTransform);
        }

        public static bool SourceIsPlaying(IAudioSource source)
        {
            return Provider.SourceIsPlaying(source);
        }
    }
}
