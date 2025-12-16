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
            Provider = new OpenALProvider();
        }

        public static StrideAudioDevice Create(string deviceName, DeviceFlags flags)
        {
            return Provider.Create(deviceName, flags);
        }

        public static void Destroy(StrideAudioDevice device)
        {
            Provider.Destroy(device);
        }

        public static void Update(StrideAudioDevice device)
        {
            Provider.Update(device);
        }

        public static void SetMasterVolume(StrideAudioDevice device, float volume)
        {
            Provider.SetMasterVolume(device, volume);
        }

        public static StrideAudioListener ListenerCreate(StrideAudioDevice device)
        {
            return Provider.ListenerCreate(device);
        }

        public static void ListenerDestroy(StrideAudioListener listener)
        {
            Provider.ListenerDestroy(listener);
        }

        public static bool ListenerEnable(StrideAudioListener listener)
        {
            return Provider.ListenerEnable(listener);
        }

        public static void ListenerDisable(StrideAudioListener listener)
        {
            Provider.ListenerDisable(listener);
        }

        public static StrideAudioSource SourceCreate(StrideAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor,
            HrtfEnvironment environment)
        {
            return Provider.SourceCreate(listener, sampleRate, maxNumberOfBuffers, mono, spatialized, streamed, hrtf, hrtfDirectionFactor, environment);
        }

        public static void SourceDestroy(StrideAudioSource source)
        {
            Provider.SourceDestroy(source);
        }

        public static double SourceGetPosition(StrideAudioSource source)
        {
            return Provider.SourceGetPosition(source);
        }

        public static void SourceSetPan(StrideAudioSource source, float pan)
        {
            Provider.SourceSetPan(source, pan);
        }

        public static StrideAudioBuffer BufferCreate(int maxBufferSizeBytes)
        {
            return Provider.BufferCreate(maxBufferSizeBytes);
        }

        public static void BufferDestroy(StrideAudioBuffer buffer)
        {
            Provider.BufferDestroy(buffer);
        }

        public static void BufferFill(StrideAudioBuffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono)
        {
            Provider.BufferFill(buffer, pcm, bufferSize, sampleRate, mono);
        }

        public static void SourceSetBuffer(StrideAudioSource source, StrideAudioBuffer buffer)
        {
            Provider.SourceSetBuffer(source, buffer);
        }

        public static void SourceFlushBuffers(StrideAudioSource source)
        {
            Provider.SourceFlushBuffers(source);
        }

        public static void SourceQueueBuffer(StrideAudioSource source, StrideAudioBuffer buffer, IntPtr pcm, int bufferSize, BufferType streamType)
        {
            Provider.SourceQueueBuffer(source, buffer, pcm, bufferSize, streamType);
        }

        public static StrideAudioBuffer SourceGetFreeBuffer(StrideAudioSource source)
        {
            return Provider.SourceGetFreeBuffer(source);
        }

        public static void SourcePlay(StrideAudioSource source)
        {
            Provider.SourcePlay(source);
        }

        public static void SourcePause(StrideAudioSource source)
        {
            Provider.SourcePause(source);
        }

        public static void SourceStop(StrideAudioSource source)
        {
            Provider.SourceStop(source);
        }

        public static void SourceSetLooping(StrideAudioSource source, bool looped)
        {
            Provider.SourceSetLooping(source, looped);
        }

        public static void SourceSetRange(StrideAudioSource source, double startTime, double stopTime)
        {
            Provider.SourceSetRange(source, startTime, stopTime);
        }

        public static void SourceSetGain(StrideAudioSource source, float gain)
        {
            Provider.SourceSetGain(source, gain);
        }

        public static void SourceSetPitch(StrideAudioSource source, float pitch)
        {
            Provider.SourceSetPitch(source, pitch);
        }

        public static void ListenerPush3D(StrideAudioListener listener, ref Vector3 pos, ref Vector3 forward, ref Vector3 up, ref Vector3 vel, ref Matrix worldTransform)
        {
            Provider.ListenerPush3D(listener, pos, forward, up, vel, worldTransform);
        }

        public static void SourcePush3D(StrideAudioSource source, ref Vector3 pos, ref Vector3 forward, ref Vector3 up, ref Vector3 vel, ref Matrix worldTransform)
        {
            Provider.SourcePush3D(source, pos, forward, up, vel, worldTransform);
        }

        public static bool SourceIsPlaying(StrideAudioSource source)
        {
            return Provider.SourceIsPlaying(source);
        }
    }
}
