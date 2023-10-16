// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices;
using System.Security;
using Silk.NET.OpenAL;
using Stride.Core.Mathematics;

namespace Stride.Audio
{
    /// <summary>
    /// Wrapper around OpenAL
    /// </summary>
    public class AudioLayer
    {
        private static ALContext _alc;
        private static AL _al;

        public struct Listener
        {
            public AudioDevice ALDevice { get; internal set; }
            public unsafe Context* ALContext { get; internal set; }
        }

        public struct Buffer
        {
            public IntPtr Ptr;
        }

        public static void Init()
        {
            _alc = ALContext.GetApi();
            _al = AL.GetApi();
        }

        public static unsafe AudioDevice Create(string deviceName)
        {
            var res = new AudioDevice();
            res.ALDevice = _alc.OpenDevice(deviceName);
            if (res.ALDevice == null)
            {
                return null;
            }
            return res;
        }

        public static unsafe void Destroy(AudioDevice device)
        {
            _alc.CloseDevice(device.ALDevice);
            _alc.GetError(device.ALDevice);
        }

        public static unsafe void Update(AudioDevice device)
        {
            device.DeviceLock.Lock();

            foreach (var listener in device.Listeners)
            {
                ContextState lock (listener->context) ;

                foreach (var source in listener->sources)
                {
                    if (source->streamed)
                    {
                        var processed = 0;
                        GetSourceI(source->source, AL_BUFFERS_PROCESSED, &processed);
                        while (processed--)
                        {
                            float preDTime;
                            _al.GetSourceProperty(source.ALSource, , &preDTime);
                            GetSourceF(source->source, AL_SEC_OFFSET, &preDTime);

                            ALuint buffer;
                            SourceUnqueueBuffers(source->source, 1, &buffer);
                            xnAudioBuffer* bufferPtr = source->listener->buffers[buffer];

                            ALfloat postDTime;
                            GetSourceF(source->source, AL_SEC_OFFSET, &postDTime);

                            if (bufferPtr->type == EndOfStream || bufferPtr->type == EndOfLoop)
                            {
                                source->dequeuedTime = 0.0;
                            }
                            else
                            {
                                source->dequeuedTime += preDTime - postDTime;
                            }

                            source->freeBuffers.push_back(bufferPtr);
                        }
                    }
                }
            }

            device->deviceLock.Unlock();
        }

        public static void SetMasterVolume(AudioDevice device, float volume)
        {
            device->deviceLock.Lock();
            foreach (var listener in device->listeners)
            {
                ContextState lock (listener->context) ;
                _al.SetListenerProperty(ListenerFloat.Gain, volume);
            }
            device->deviceLock.Unlock();
        }

        public unsafe static Listener ListenerCreate(AudioDevice device)
        {
            var res = new Listener();
            res.ALDevice = device;

            res.ALContext = _alc.CreateContext(device.ALDevice, null);
            _alc.GetError(device.ALDevice);
            _alc.MakeContextCurrent(res.ALContext);
            _alc.GetError(device.ALDevice);
            _alc.ProcessContext(res.ALContext);
            _alc.GetError(device.ALDevice);
            
            device->deviceLock.Lock();

            device->listeners.insert(res);

            device->deviceLock.Unlock();

            return res;
        }

        public unsafe static void ListenerDestroy(Listener listener)
        {
            //listener->device->deviceLock.Lock();

            //listener->device->listeners.erase(listener);

            //listener->device->deviceLock.Unlock();

            _alc.DestroyContext(listener.ALContext);

            delete listener;
        }

        public unsafe static bool ListenerEnable(Listener listener)
        {
            bool res = _alc.MakeContextCurrent(listener.ALContext);
            _alc.ProcessContext(listener.ALContext);
            return res;
        }

        public unsafe static void ListenerDisable(Listener listener)
        {
            _alc.SuspendContext(listener.ALContext);
            _alc.MakeContextCurrent(null);
        }

        public static AudioSource SourceCreate(Listener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor, HrtfEnvironment environment)
        {
            (void)spatialized;
            (void)maxNBuffers;

            var res = new AudioSource;
            res->listener = listener;
            res->sampleRate = sampleRate;
            res->mono = mono;
            res->streamed = streamed;

            ContextState lock (listener->context) ;

            GenSources(1, &res->source);
            AL_ERROR;
            SourceF(res->source, AL_REFERENCE_DISTANCE, 1.0f);
            AL_ERROR;

            if (spatialized)
            {
                //make sure we are able to 3D
                SourceI(res->source, AL_SOURCE_RELATIVE, AL_FALSE);
            }
            else
            {
                //make sure we are able to pan
                SourceI(res->source, AL_SOURCE_RELATIVE, AL_TRUE);
            }

            listener->sources.insert(res);

            return res;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceDestroy", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceDestroy(AudioSource source);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceGetPosition", CallingConvention = CallingConvention.Cdecl)]
        public static double SourceGetPosition(AudioSource source);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetPan", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceSetPan(AudioSource source, float pan);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioBufferCreate", CallingConvention = CallingConvention.Cdecl)]
        public static Buffer BufferCreate(int maxBufferSizeBytes);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioBufferDestroy", CallingConvention = CallingConvention.Cdecl)]
        public static void BufferDestroy(Buffer buffer);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioBufferFill", CallingConvention = CallingConvention.Cdecl)]
        public static void BufferFill(Buffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceSetBuffer(AudioSource source, Buffer buffer);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceFlushBuffers", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceFlushBuffers(AudioSource source);

        public enum BufferType
        {
            None,
            BeginOfStream,
            EndOfStream,
            EndOfLoop,
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceQueueBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceQueueBuffer(AudioSource source, Buffer buffer, IntPtr pcm, int bufferSize, BufferType streamType);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceGetFreeBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static Buffer SourceGetFreeBuffer(AudioSource source);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourcePlay", CallingConvention = CallingConvention.Cdecl)]
        public static void SourcePlay(AudioSource source);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourcePause", CallingConvention = CallingConvention.Cdecl)]
        public static void SourcePause(AudioSource source);

        public static void SourceStop(AudioSource source)
        {
            // ContextState lock (source->listener->context) ;
            _al.Sou
            _alc.SourceStop(source->source);
            xnAudioSourceFlushBuffers(source);

            //reset timing info
            if (source->streamed)
                source->dequeuedTime = 0.0;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetLooping", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceSetLooping(AudioSource source, bool looped);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetRange", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceSetRange(AudioSource source, double startTime, double stopTime);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetGain", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceSetGain(AudioSource source, float gain);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourceSetPitch", CallingConvention = CallingConvention.Cdecl)]
        public static void SourceSetPitch(AudioSource source, float pitch);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioListenerPush3D", CallingConvention = CallingConvention.Cdecl)]
        public static void ListenerPush3D(Listener listener, ref Vector3 pos, ref Vector3 forward, ref Vector3 up, ref Vector3 vel, ref Matrix worldTransform);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnAudioSourcePush3D", CallingConvention = CallingConvention.Cdecl)]
        public static void SourcePush3D(AudioSource source, ref Vector3 pos, ref Vector3 forward, ref Vector3 up, ref Vector3 vel, ref Matrix worldTransform)
        {

        }

        public static bool SourceIsPlaying(AudioSource source)
        {
            ContextState lock (source->listener->context) ;

            _al.GetSourceProperty(source.ALSource, GetSourceInteger.SourceState, out var value);
            return value == (int)SourceState.Playing || value == (int)SourceState.Paused;
        }
    }

    public class AudioSource
    {
        public uint ALSource { get; internal set; }
    }
}
