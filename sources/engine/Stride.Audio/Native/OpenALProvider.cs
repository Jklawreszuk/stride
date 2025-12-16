// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenAL;
using Stride.Core.Mathematics;

namespace Stride.Audio;

internal unsafe class OpenALProvider : IAudioProvider
{
    private readonly ALContext alc = ALContext.GetApi();
    private readonly AL al = AL.GetApi();

    public StrideAudioDevice Create(string deviceName, DeviceFlags flags)
    {
        var device = new StrideAudioDevice { Value = alc.OpenDevice(deviceName) };
        alc.GetError(device.Value);
        return device.Value == null ? null : device;
    }

    public void Destroy(StrideAudioDevice device)
    {
        alc.CloseDevice(device.Value);
    }

    public void Update(StrideAudioDevice device)
    {
        device.DeviceLock.Lock();

        for (var i = 0; i < device.Listeners.Count; i++)
        {
            var listener = device.Listeners[i];

            for (var j = 0; j < listener.Sources.Count; j++)
            {
                var source = listener.Sources[j];
                if (source.Streamed)
                {
                    int processed = 0;
                    al.GetSourceProperty(source.Value, GetSourceInteger.BuffersProcessed, &processed);
                    while (processed-- > 0)
                    {
                        float preDTime;
                        al.GetSourceProperty(source.Value, SourceFloat.SecOffset, &preDTime);

                        uint buffer;
                        al.SourceUnqueueBuffers(source.Value, 1, &buffer);
                        var bufferPtr = source.Listener.Buffers[buffer];

                        float postDTime;
                        al.GetSourceProperty(source.Value, SourceFloat.SecOffset, &postDTime);

                        if (bufferPtr.Type is BufferType.EndOfStream or BufferType.EndOfLoop)
                        {
                            source.DequeuedTime = 0.0f;
                        }
                        else
                        {
                            source.DequeuedTime += preDTime - postDTime;
                        }

                        source.FreeBuffers.Add(bufferPtr);
                    }
                }
            }
        }
        device.DeviceLock.Unlock();
    }

    public void SetMasterVolume(StrideAudioDevice device, float volume)
    {
        device.DeviceLock.Lock();
        for (var i = 0; i < device.Listeners.Count; i++)
        {
            al.SetListenerProperty(ListenerFloat.Gain, volume);
        }
        device.DeviceLock.Unlock();
    }

    public StrideAudioListener ListenerCreate(StrideAudioDevice device)
    {
        var listener = new StrideAudioListener();
        listener.Device = device;

        listener.Context = alc.CreateContext(device.Value, null);
        System.Diagnostics.Debug.Assert(alc.GetError(device.Value) == ContextError.NoError);
        alc.MakeContextCurrent(listener.Context);
        System.Diagnostics.Debug.Assert(alc.GetError(device.Value) == ContextError.NoError);
        alc.ProcessContext(listener.Context);        
        System.Diagnostics.Debug.Assert(alc.GetError(device.Value) == ContextError.NoError);

        device.DeviceLock.Lock();

        device.Listeners.Add(listener);

        device.DeviceLock.Unlock();

        return listener;
    }

    public void ListenerDestroy(StrideAudioListener listener)
    {
        listener.Device.DeviceLock.Lock();

        listener.Device.Listeners.Remove(listener);

        listener.Device.DeviceLock.Unlock();

        alc.DestroyContext(listener.Context);
    }

    public bool ListenerEnable(StrideAudioListener listener)
    {
        bool res = alc.MakeContextCurrent(listener.Context);
        alc.ProcessContext(listener.Context);
        return res;
    }

    public void ListenerDisable(StrideAudioListener listener)
    {
        alc.SuspendContext(listener.Context);
        alc.MakeContextCurrent(null);
    }

    public StrideAudioSource SourceCreate(StrideAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor, HrtfEnvironment environment)
    {
        var source = new StrideAudioSource
        {
            Listener = listener,
            SampleRate = sampleRate,
            Mono = mono,
            Streamed = streamed,
        };

        fixed(uint* sourcePtr = &source.Value)
        {
            al.GenSources(1, sourcePtr);
        }

        System.Diagnostics.Debug.Assert(al.GetError() == AudioError.NoError);
        al.SetSourceProperty(source.Value, SourceFloat.ReferenceDistance, 1.0f);
        System.Diagnostics.Debug.Assert(al.GetError() == AudioError.NoError);

        //make sure we are able to 3D or pan
        al.SetSourceProperty(source.Value, SourceBoolean.SourceRelative, !spatialized);

        listener.Sources.Add(source);

        return source;
    }

    public void SourceDestroy(StrideAudioSource source)
    {
        fixed(uint* sourcePtr = &source.Value)
        {
            al.DeleteSources(1, sourcePtr);
        }
        source.Listener.Sources.Remove(source);
    }

    public double SourceGetPosition(StrideAudioSource source)
    {
        al.GetSourceProperty(source.Value, SourceFloat.SecOffset, out var offset);

        if (!source.Streamed)
        {				
            return offset;
        }

        return offset + source.DequeuedTime;
    }

    public void SourceSetPan(StrideAudioSource source, float pan)
    {
        float clampedPan = pan > 1.0f ? 1.0f : pan < -1.0f ? -1.0f : pan;

        al.SetSourceProperty(source.Value, SourceVector3.Position, clampedPan, MathF.Sqrt(1.0f - clampedPan * clampedPan), 0f);
    }

    public StrideAudioBuffer BufferCreate(int maxBufferSize)
    {
        var buffer = new StrideAudioBuffer();
        buffer.Pcm = new short[maxBufferSize];
        fixed (uint* bufferPtr = &buffer.Value)
        {
            al.GenBuffers(1, bufferPtr);
        }
        return buffer;
    }

    public void BufferDestroy(StrideAudioBuffer buffer)
    {
        fixed (uint* bufferPtr = &buffer.Value)
        {
            al.DeleteBuffers(1, bufferPtr);
        }
        buffer.Pcm = null;
    }

    public void BufferFill(StrideAudioBuffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono)
    {
        //we have to keep a copy sadly because we might need to offset the data at some point	

        buffer.Pcm = new short[bufferSize];		
        Marshal.Copy(pcm, buffer.Pcm, 0, bufferSize);

        buffer.Size = bufferSize;
        buffer.SampleRate = sampleRate;

        al.BufferData(buffer.Value, mono ? BufferFormat.Mono16 : BufferFormat.Stereo16, pcm.ToPointer(), bufferSize, sampleRate);
    }

    public void SourceSetBuffer(StrideAudioSource source, StrideAudioBuffer buffer)
    {
        source.SingleBuffer = buffer;
        al.SetSourceProperty(source.Value, SourceInteger.Buffer, buffer.Value);
    }

    public void SourceFlushBuffers(StrideAudioSource source)
    {
        if (source.Streamed)
        {
            //flush all buffers
            al.GetSourceProperty(source.Value, GetSourceInteger.BuffersProcessed, out var processed);
            while (processed-- <= 0)
            {
                uint buffer = 0;
                al.SourceUnqueueBuffers(source.Value, 1, &buffer);
            }

            //return the source to undetermined mode
            al.SetSourceProperty(source.Value, SourceInteger.Buffer, 0);

            //set all buffers as free
            source.FreeBuffers.Clear();
            foreach (var buffer in source.Listener.Buffers)
            {
                source.FreeBuffers.Add(buffer.Value);
            }
        }
    }

    public void SourceQueueBuffer(StrideAudioSource source, StrideAudioBuffer buffer, IntPtr pcm, int bufferSize, BufferType streamType)
    {
        buffer.Type = streamType;
        buffer.Size = bufferSize;
        al.BufferData(buffer.Value, source.Mono ? BufferFormat.Mono16 : BufferFormat.Stereo16, pcm.ToPointer(), bufferSize, source.SampleRate);
        fixed (uint* bufferPtr = &buffer.Value)
        {
            al.SourceQueueBuffers(source.Value, 1, bufferPtr);
        }

        source.Listener.Buffers[buffer.Value] = buffer;
    }

    public StrideAudioBuffer SourceGetFreeBuffer(StrideAudioSource source)
    {
        if(source.FreeBuffers.Count > 0)
        {
            var buffer = source.FreeBuffers[^1];
            source.FreeBuffers.Remove(buffer);
            return buffer;
        }

        return null;
    }

    public void SourcePlay(StrideAudioSource source)
    {
        al.SourcePlay(source.Value);
    }

    public void SourcePause(StrideAudioSource source)
    {
        al.SourcePause(source.Value);
    }

    public void SourceStop(StrideAudioSource source)
    {
        al.SourceStop(source.Value);
        SourceFlushBuffers(source);

        //reset timing info
        if(source.Streamed)
            source.DequeuedTime = 0.0f;
    }

    public void SourceSetLooping(StrideAudioSource source, bool looped)
    {
        al.SetSourceProperty(source.Value, SourceBoolean.Looping, looped);
    }

    public void SourceSetRange(StrideAudioSource source, double startTime, double stopTime)
    {
        if (source.Streamed)
        {
            return;
        }

        al.GetSourceProperty(source.Value, GetSourceInteger.SourceState, out var playing);
        if (playing == (int)SourceState.Playing) 
            al.SourceStop(source.Value);

        al.SetSourceProperty(source.Value, SourceInteger.Buffer, 0);

        //OpenAL is kinda bad and offers only starting offset...
        //As result we need to rewrite the buffer
        if(startTime == 0 && stopTime == 0)
        {
            //cancel the offsetting			
            fixed (short* pPcm = source.SingleBuffer.Pcm)
            {				
                al.BufferData(source.SingleBuffer.Value, source.Mono ? BufferFormat.Mono16 : BufferFormat.Stereo16, pPcm, source.SingleBuffer.Size, source.SingleBuffer.SampleRate);	
            }					
        }
        else
        {
            //offset the data
            int sampleStart = (int)(source.SingleBuffer.SampleRate * (source.Mono ? 1.0 : 2.0) * startTime);
            int sampleStop = (int)(source.SingleBuffer.SampleRate * (source.Mono ? 1.0 : 2.0) * stopTime);

            if (sampleStart > source.SingleBuffer.Size / sizeof(short))
            {
                return; //the starting position must be less then the total length of the buffer
            }

            if (sampleStop > source.SingleBuffer.Size / sizeof(short)) //if the end point is more then the length of the buffer fix the value
            {
                sampleStop = source.SingleBuffer.Size / sizeof(short);
            }

            var len = sampleStop - sampleStart;


            fixed (short* pPcm = source.SingleBuffer.Pcm)
            {            
                short* offsettedBuffer = pPcm + sampleStart;
                al.BufferData(source.SingleBuffer.Value, source.Mono ? BufferFormat.Mono16 : BufferFormat.Stereo16, offsettedBuffer, len * sizeof(short), source.SingleBuffer.SampleRate);
            }
        }

        al.SetSourceProperty(source.Value, SourceInteger.Buffer, source.SingleBuffer.Value);
        if (playing == (int)SourceState.Playing) 
            al.SourcePlay(source.Value);
    }

    public void SourceSetGain(StrideAudioSource source, float gain)
    {
        al.SetSourceProperty(source.Value, SourceFloat.Gain, gain);
    }

    public void SourceSetPitch(StrideAudioSource source, float pitch)
    {
        al.SetSourceProperty(source.Value, SourceFloat.Pitch, pitch);
    }

    public void ListenerPush3D(StrideAudioListener listener, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
        float[] ori = [forward[0], forward[1], -forward[2], up[0], up[1], -up[2]];

        fixed(float * pOri = ori)
        {
            al.SetListenerProperty(ListenerFloatArray.Orientation, pOri);
        }
        al.SetListenerProperty(ListenerVector3.Position, pos.X, pos.Y, pos.Z);

        al.SetListenerProperty(ListenerVector3.Velocity, vel.X, vel.Y, -vel.Z);
    }

    public void SourcePush3D(StrideAudioSource source, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
        float[] ori = [forward[0], forward[1], -forward[2], up[0], up[1], -up[2]];

        fixed(float * pOri = ori)
        {
            al.SetSourceProperty(source.Value, SourceVector3.Direction, pOri);//Todo maybe I should add vector here
        }
        al.SetSourceProperty(source.Value, SourceVector3.Position, pos.X, pos.Y, pos.Z);

        al.SetSourceProperty(source.Value, SourceVector3.Velocity, vel.X, vel.Y, -vel.Z);	
    }

    public bool SourceIsPlaying(StrideAudioSource source)
    {
        al.GetSourceProperty(source.Value, GetSourceInteger.SourceState, out var value);
        return value is (int)SourceState.Playing or (int)SourceState.Paused;
    }
}
