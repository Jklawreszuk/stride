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

    public IAudioDevice Create(string deviceName, DeviceFlags flags)
    {
        var device = new OpenALDevice { Value = alc.OpenDevice(deviceName) };
        alc.GetError(device.Value);
        return device.Value == null ? null : device;
    }

    public void Destroy(IAudioDevice device)
    {
        alc.CloseDevice(((OpenALDevice)device).Value);
    }

    public void Update(IAudioDevice device)
    {
        OpenALDevice openALDevice = (OpenALDevice)device;
        openALDevice.DeviceLock.Lock();

        for (var i = 0; i < openALDevice.Listeners.Count; i++)
        {
            var listener = openALDevice.Listeners[i];

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
        openALDevice.DeviceLock.Unlock();
    }

    public void SetMasterVolume(IAudioDevice device, float volume)
    {
        OpenALDevice openALDevice = (OpenALDevice)device;
        openALDevice.DeviceLock.Lock();
        for (var i = 0; i < openALDevice.Listeners.Count; i++)
        {
            al.SetListenerProperty(ListenerFloat.Gain, volume);
        }
        openALDevice.DeviceLock.Unlock();
    }

    public IAudioListener ListenerCreate(IAudioDevice device)
    {
        OpenALDevice openALDevice = (OpenALDevice)device;
        var listener = new OpenALLister();
        listener.Device = openALDevice;

        listener.Context = alc.CreateContext(openALDevice.Value, null);
        System.Diagnostics.Debug.Assert(alc.GetError(openALDevice.Value) == ContextError.NoError);
        alc.MakeContextCurrent(listener.Context);
        System.Diagnostics.Debug.Assert(alc.GetError(openALDevice.Value) == ContextError.NoError);
        alc.ProcessContext(listener.Context);        
        System.Diagnostics.Debug.Assert(alc.GetError(openALDevice.Value) == ContextError.NoError);

        openALDevice.DeviceLock.Lock();

        openALDevice.Listeners.Add(listener);

        openALDevice.DeviceLock.Unlock();

        return listener;
    }

    public void ListenerDestroy(IAudioListener listener)
    {
        OpenALLister openAlLister = (OpenALLister)listener;
        openAlLister.Device.DeviceLock.Lock();

        openAlLister.Device.Listeners.Remove(openAlLister);

        openAlLister.Device.DeviceLock.Unlock();

        alc.DestroyContext(openAlLister.Context);
    }

    public bool ListenerEnable(IAudioListener listener)
    {
        OpenALLister openAlLister = (OpenALLister)listener;
        bool res = alc.MakeContextCurrent(openAlLister.Context);
        alc.ProcessContext(openAlLister.Context);
        return res;
    }

    public void ListenerDisable(IAudioListener listener)
    {
        alc.SuspendContext(((OpenALLister)listener).Context);
        alc.MakeContextCurrent(null);
    }

    public IAudioSource SourceCreate(IAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor, HrtfEnvironment environment)
    {
        OpenALLister openAlLister = (OpenALLister)listener;
        var source = new OpenALSource
        {
            Listener = openAlLister,
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

        openAlLister.Sources.Add(source);

        return source;
    }

    public void SourceDestroy(IAudioSource source)
    {
        OpenALSource openAlSource = (OpenALSource)source;
        fixed(uint* sourcePtr = &openAlSource.Value)
        {
            al.DeleteSources(1, sourcePtr);
        }
        openAlSource.Listener.Sources.Remove(openAlSource);
    }

    public double SourceGetPosition(IAudioSource source)
    {
        OpenALSource openAlSource = (OpenALSource)source;
        al.GetSourceProperty(openAlSource.Value, SourceFloat.SecOffset, out var offset);

        if (!source.Streamed)
        {				
            return offset;
        }

        return offset + openAlSource.DequeuedTime;
    }

    public void SourceSetPan(IAudioSource source, float pan)
    {
        float clampedPan = pan > 1.0f ? 1.0f : pan < -1.0f ? -1.0f : pan;

        al.SetSourceProperty(((OpenALSource)source).Value, SourceVector3.Position, clampedPan, MathF.Sqrt(1.0f - clampedPan * clampedPan), 0f);
    }

    public IAudioBuffer BufferCreate(int maxBufferSize)
    {
        var buffer = new OpenALBuffer();
        buffer.Pcm = new short[maxBufferSize];
        fixed (uint* bufferPtr = &buffer.Value)
        {
            al.GenBuffers(1, bufferPtr);
        }
        return buffer;
    }

    public void BufferDestroy(IAudioBuffer buffer)
    {
        OpenALBuffer openAlBuffer = (OpenALBuffer)buffer;
        fixed (uint* bufferPtr = &openAlBuffer.Value)
        {
            al.DeleteBuffers(1, bufferPtr);
        }
        openAlBuffer.Pcm = null;
    }

    public void BufferFill(IAudioBuffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono)
    {
        OpenALBuffer openAlBuffer = (OpenALBuffer)buffer;
        //we have to keep a copy sadly because we might need to offset the data at some point	

        openAlBuffer.Pcm = new short[bufferSize];		
        Marshal.Copy(pcm, openAlBuffer.Pcm, 0, bufferSize);

        openAlBuffer.Size = bufferSize;
        openAlBuffer.SampleRate = sampleRate;

        al.BufferData(openAlBuffer.Value, mono ? BufferFormat.Mono16 : BufferFormat.Stereo16, pcm.ToPointer(), bufferSize, sampleRate);
    }

    public void SourceSetBuffer(IAudioSource source, IAudioBuffer buffer)
    {
        OpenALSource openAlSource = (OpenALSource)source;
        openAlSource.SingleBuffer = (OpenALBuffer)buffer;
        al.SetSourceProperty(openAlSource.Value, SourceInteger.Buffer, openAlSource.SingleBuffer.Value);
    }

    public void SourceFlushBuffers(IAudioSource source)
    {
        OpenALSource openAlSource = (OpenALSource)source;
        if (source.Streamed)
        {
            //flush all buffers
            al.GetSourceProperty(openAlSource.Value, GetSourceInteger.BuffersProcessed, out var processed);
            while (processed-- <= 0)
            {
                uint buffer = 0;
                al.SourceUnqueueBuffers(openAlSource.Value, 1, &buffer);
            }

            //return the source to undetermined mode
            al.SetSourceProperty(openAlSource.Value, SourceInteger.Buffer, 0);

            //set all buffers as free
            openAlSource.FreeBuffers.Clear();
            foreach (var buffer in openAlSource.Listener.Buffers)
            {
                openAlSource.FreeBuffers.Add(buffer.Value);
            }
        }
    }

    public void SourceQueueBuffer(IAudioSource source, IAudioBuffer buffer, IntPtr pcm, int bufferSize, BufferType streamType)
    {
        OpenALBuffer openAlBuffer = (OpenALBuffer)buffer;
        OpenALSource openAlSource = (OpenALSource)source;
        openAlBuffer.Type = streamType;
        openAlBuffer.Size = bufferSize;
        al.BufferData(openAlBuffer.Value, source.Mono ? BufferFormat.Mono16 : BufferFormat.Stereo16, pcm.ToPointer(), bufferSize, openAlSource.SampleRate);
        fixed (uint* bufferPtr = &openAlBuffer.Value)
        {
            al.SourceQueueBuffers(openAlSource.Value, 1, bufferPtr);
        }

        openAlSource.Listener.Buffers[openAlBuffer.Value] = openAlBuffer;
    }

    public IAudioBuffer SourceGetFreeBuffer(IAudioSource source)
    {
        OpenALSource openAlSource = (OpenALSource)source;
        if(openAlSource.FreeBuffers.Count > 0)
        {
            var buffer = openAlSource.FreeBuffers[^1];
            openAlSource.FreeBuffers.Remove(buffer);
            return buffer;
        }

        return null;
    }

    public void SourcePlay(IAudioSource source)
    {
        al.SourcePlay(((OpenALSource)source).Value);
    }

    public void SourcePause(IAudioSource source)
    {
        al.SourcePlay(((OpenALSource)source).Value);
    }

    public void SourceStop(IAudioSource source)
    {
        al.SourcePlay(((OpenALSource)source).Value);
        SourceFlushBuffers(source);

        //reset timing info
        if(source.Streamed)
            ((OpenALSource)source).DequeuedTime = 0.0f;
    }

    public void SourceSetLooping(IAudioSource source, bool looped)
    {
        al.SetSourceProperty(((OpenALSource)source).Value, SourceBoolean.Looping, looped);
    }

    public void SourceSetRange(IAudioSource source, double startTime, double stopTime)
    {
        OpenALSource openAlSource = (OpenALSource)source;
        if (source.Streamed)
        {
            return;
        }

        al.GetSourceProperty(openAlSource.Value, GetSourceInteger.SourceState, out var playing);
        if (playing == (int)SourceState.Playing) 
            al.SourceStop(openAlSource.Value);

        al.SetSourceProperty(openAlSource.Value, SourceInteger.Buffer, 0);

        //OpenAL is kinda bad and offers only starting offset...
        //As result we need to rewrite the buffer
        if(startTime == 0 && stopTime == 0)
        {
            //cancel the offsetting			
            fixed (short* pPcm = openAlSource.SingleBuffer.Pcm)
            {				
                al.BufferData(openAlSource.SingleBuffer.Value, openAlSource.Mono ? BufferFormat.Mono16 : BufferFormat.Stereo16, pPcm, openAlSource.SingleBuffer.Size, openAlSource.SingleBuffer.SampleRate);	
            }					
        }
        else
        {
            //offset the data
            int sampleStart = (int)(openAlSource.SingleBuffer.SampleRate * (openAlSource.Mono ? 1.0 : 2.0) * startTime);
            int sampleStop = (int)(openAlSource.SingleBuffer.SampleRate * (openAlSource.Mono ? 1.0 : 2.0) * stopTime);

            if (sampleStart > openAlSource.SingleBuffer.Size / sizeof(short))
            {
                return; //the starting position must be less then the total length of the buffer
            }

            if (sampleStop > openAlSource.SingleBuffer.Size / sizeof(short)) //if the end point is more then the length of the buffer fix the value
            {
                sampleStop = openAlSource.SingleBuffer.Size / sizeof(short);
            }

            var len = sampleStop - sampleStart;


            fixed (short* pPcm = openAlSource.SingleBuffer.Pcm)
            {            
                short* offsettedBuffer = pPcm + sampleStart;
                al.BufferData(openAlSource.SingleBuffer.Value, openAlSource.Mono ? BufferFormat.Mono16 : BufferFormat.Stereo16, offsettedBuffer, len * sizeof(short), openAlSource.SingleBuffer.SampleRate);
            }
        }

        al.SetSourceProperty(openAlSource.Value, SourceInteger.Buffer, openAlSource.SingleBuffer.Value);
        if (playing == (int)SourceState.Playing) 
            al.SourcePlay(openAlSource.Value);
    }

    public void SourceSetGain(IAudioSource source, float gain)
    {
        al.SetSourceProperty(((OpenALSource)source).Value, SourceFloat.Gain, gain);
    }

    public void SourceSetPitch(IAudioSource source, float pitch)
    {
        al.SetSourceProperty(((OpenALSource)source).Value, SourceFloat.Pitch, pitch);
    }

    public void ListenerPush3D(IAudioListener listener, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
        float[] ori = [forward[0], forward[1], -forward[2], up[0], up[1], -up[2]];

        fixed(float * pOri = ori)
        {
            al.SetListenerProperty(ListenerFloatArray.Orientation, pOri);
        }
        al.SetListenerProperty(ListenerVector3.Position, pos.X, pos.Y, pos.Z);

        al.SetListenerProperty(ListenerVector3.Velocity, vel.X, vel.Y, -vel.Z);
    }

    public void SourcePush3D(IAudioSource source, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
        float[] ori = [forward[0], forward[1], -forward[2], up[0], up[1], -up[2]];

        fixed(float * pOri = ori)
        {
            al.SetSourceProperty(((OpenALSource)source).Value, SourceVector3.Direction, pOri);//Todo maybe I should add vector here
        }
        al.SetSourceProperty(((OpenALSource)source).Value, SourceVector3.Position, pos.X, pos.Y, pos.Z);

        al.SetSourceProperty(((OpenALSource)source).Value, SourceVector3.Velocity, vel.X, vel.Y, -vel.Z);	
    }

    public bool SourceIsPlaying(IAudioSource source)
    {
        al.GetSourceProperty(((OpenALSource)source).Value, GetSourceInteger.SourceState, out var value);
        return value is (int)SourceState.Playing or (int)SourceState.Paused;
    }
}
