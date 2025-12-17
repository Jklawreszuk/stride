// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.XAudio;
using Stride.Core.Mathematics;
using Buffer = System.Buffer;

namespace Stride.Audio;

public unsafe class XAudio2Provider : IAudioProvider
{
    private const int AudioChannels = 2;
    private const float MaxFreqRatio = 1024.0f;
    private const float SpeedOfSound = 343.5f;
    private const int SpeakerFrontLeft = 1;
    private const int SpeakerFrontRight = 2;
    private const int SpeakerStereo = SpeakerFrontLeft | SpeakerFrontRight;
    
    private readonly XAudio xAudio = XAudio.GetApi();
    
    public IAudioDevice Create(string deviceName, DeviceFlags flags)
    {
        var device = new XAudio2Device { Hrtf = flags == DeviceFlags.Hrtf };

        //XAudio2, no flags, processor 1
        var result = xAudio.CreateWithVersionInfo(ref device.xAudio, device.Hrtf ? XAudio.XAudio21024Quantum : 0u, XAudio.Processor1, 0);
        if (HResult.IndicatesFailure(result))
        {
            return null;
        }

        //this means opening the real audio device, which will be virtual actually so in the case of default device change Xaudio will deal with it for us.
        result = device.xAudio->CreateMasteringVoice(ref device.masteringVoice, AudioChannels, 0, 0, deviceName, null, AudioStreamCategory.GameEffects);
        if (HResult.IndicatesFailure(result))
        {
            return null;
        }

        //start audio rendering
        result = device.xAudio->StartEngine();
        if (HResult.IndicatesFailure(result))
        {
            return null;
        }		
			
        //X3DAudio
        result = X3DAudio.X3DAudioInitialize(SpeakerStereo, SpeedOfSound, out device.x3_audio);
        if (HResult.IndicatesFailure(result))
        {
            return null;
        }

        return device;
    }

    public void Destroy(IAudioDevice device)
    {
        XAudio2Device xAudio2Device = (XAudio2Device)device;
        xAudio2Device.xAudio->StopEngine();
        xAudio2Device.masteringVoice->DestroyVoice();
    }

    public void Update(IAudioDevice device)
    {
        
    }

    public void SetMasterVolume(IAudioDevice device, float volume)
    {
        ((XAudio2Device)device).masteringVoice->SetVolume(volume,0);
    }

    public IAudioListener ListenerCreate(IAudioDevice device)
    {
        var listener = new XAudio2Listener();
        listener.Device = (XAudio2Device)device;
        listener.XAudioListener = new X3DAudioListener();
        return listener;
    }

    public void ListenerDestroy(IAudioListener listener)
    {
        //nothing to destroy
    }

    public bool ListenerEnable(IAudioListener listener)
    {
        return true;
    }

    public void ListenerDisable(IAudioListener listener)
    {
        //unused in XAudio2
    }

    public IAudioSource SourceCreate(IAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor,
        HrtfEnvironment environment)
    {
        var source = new XAudio2Source
        {
            Listener = (XAudio2Listener)listener,
            SampleRate = sampleRate,
            Mono = mono,
            Streamed = streamed,
            masteringVoice = ((XAudio2Listener)listener).Device.masteringVoice
        };

        if((spatialized && !hrtf) || (hrtf && !source.Listener.Device.Hrtf))
		{
			//if spatialized we also need those structures to calculate 3D audio
			source.emitter = new();
			source.emitter.ChannelCount = 1;
			source.emitter.CurveDistanceScaler = 1;
			source.emitter.DopplerScaler = 1;

			source.dsp_settings = new();
			source.dsp_settings.SrcChannelCount = 1;
			source.dsp_settings.DstChannelCount = AudioChannels;
            var matrix = new float[AudioChannels];
            source.dsp_settings.pMatrixCoefficients = matrix;
            var delay = new float[AudioChannels];
            source.dsp_settings.pDelayTimes = delay;
        }

		source.FreeBuffers = new(maxNumberOfBuffers);

		//Normal PCM formal 16 bit shorts
		WaveFormatEx pcmWaveFormat = new();
        pcmWaveFormat.WFormatTag = 1;
        pcmWaveFormat.NChannels = mono ? (ushort)1 : (ushort)2;
		pcmWaveFormat.NSamplesPerSec = (uint)sampleRate;
        pcmWaveFormat.NAvgBytesPerSec = (uint)(sampleRate * pcmWaveFormat.NChannels * sizeof(short));
        pcmWaveFormat.WBitsPerSample = 16;
		pcmWaveFormat.NBlockAlign = (ushort)(pcmWaveFormat.NChannels * pcmWaveFormat.WBitsPerSample / 8);
        
        int result = ((XAudio2Listener)listener).Device.xAudio->CreateSourceVoice(ref source.sourceVoice, &pcmWaveFormat, 0, MaxFreqRatio, null, null, null);
        if (HResult.IndicatesFailure(result))
        {
            return null;
        }

        if (spatialized && source.Listener.Device.Hrtf && hrtf)
        {
   //          IXAudio2SubmixVoice* submixVoice = null;
   //
   //          HrtfDirectivity directivity = new(HrtfDirectivityType.OmniDirectional, hrtfDirectionFactor);
   //          HrtfApoInit apoInit = new(directivity);
   //
   //          IUnknown apoRoot = new();
   //          result = HrtpApo.CreateHrtfApo(&apoInit, &apoRoot);
   //          if (HResult.IndicatesFailure(result))
   //          {
   //              return null;
   //          }
   //
   //          fixed (void* hrtfPtr = &source.hrtf_params) { 
			// 	fixed(Guid* guidPtr = &HrtfParamsIID)
   //              	apoRoot.QueryInterface(guidPtr, &hrtfPtr);
			// }
   //
			// EffectDescriptor fxDesc = new();
			// fxDesc.InitialState = true;
			// fxDesc.OutputChannels = 2; // Stereo output
			// fxDesc.PEffect = (IUnknown*)source.hrtf_params; // HRTF xAPO set as the effect.
   //
			// EffectChain fxChain = new();
			// fxChain.EffectCount = 1;
			// fxChain.PEffectDescriptors = &fxDesc;
   //
			// VoiceSends sends = new();
			// SendDescriptor sendDesc = new();
			// sendDesc.POutputVoice = (IXAudio2Voice*)source.masteringVoice;
			// sends.SendCount = 1;
			// sends.PSends = &sendDesc;
   //
			// // HRTF APO expects mono 48kHz input, so we configure the submix voice for that format.
			// result = listener.Device.xAudio->CreateSubmixVoice(&submixVoice, 1, 48000, 0, 0, &sends, &fxChain);
			// if (HResult.IndicatesFailure(result))
			// {
			// 	return null;
			// }
   //
			// source.hrtf_params->SetEnvironment(environment);
   //
			// VoiceSends voice_sends = new();
   //          SendDescriptor voice_sendDesc = new();
   //          voice_sendDesc.POutputVoice = (IXAudio2Voice*)submixVoice;
			// voice_sends.SendCount = 1;
			// voice_sends.PSends = &voice_sendDesc;
			// result = source.sourceVoice->SetOutputVoices(&voice_sends);
			// if (HResult.IndicatesFailure(result))
			// {
			// 	return null;
			// }
		}

		return source;
    }

    public void SourceDestroy(IAudioSource source)
    {
        XAudio2Source sourceXAudio2Source = (XAudio2Source)source;
        sourceXAudio2Source.sourceVoice->Stop(0,0);
        sourceXAudio2Source.sourceVoice->DestroyVoice();
    }

    public double SourceGetPosition(IAudioSource source)
    {
        XAudio2Source sourceXAudio2Source = (XAudio2Source)source;
        VoiceState state;
        sourceXAudio2Source.sourceVoice->GetState(&state, 0);

        if (!source.Streamed)
            return (sourceXAudio2Source.SingleBuffer->PlayBegin + state.SamplesPlayed - (ulong)sourceXAudio2Source.SamplesAtBegin) / (float)source.SampleRate;
			
        //things work different for streamed sources, but anyway we simply subtract the snapshotted samples at begin of the stream ( could be the begin of the loop )
        return (state.SamplesPlayed - (ulong)sourceXAudio2Source.SamplesAtBegin) / (float)source.SampleRate;
    }

    public void SourceSetPan(IAudioSource source, float pan)
    {
        XAudio2Source sourceXAudio2Source = (XAudio2Source)source;
        float leftGain  = pan <= 0 ? 1.0f : 1.0f - pan;
        float rightGain = pan >= 0 ? 1.0f : 1.0f + pan;
        
        uint sourceChannels = source.Mono ? 1u : 2u;
        float[] panning = source.Mono ? [ leftGain, rightGain ] : [ leftGain, 0, 0, rightGain ];
       
        fixed(float* panningPtr = &panning[0])
        {
            sourceXAudio2Source.sourceVoice->SetOutputMatrix((IXAudio2Voice*)sourceXAudio2Source.masteringVoice, sourceChannels, AudioChannels, panningPtr, 0);
        }
    }

    public IAudioBuffer BufferCreate(int maxBufferSize)
    {
        return new XAudio2Buffer(maxBufferSize);
    }

    public void BufferDestroy(IAudioBuffer buffer)
    {
        ((XAudio2Buffer)buffer).Dispose();
    }


    public void BufferFill(IAudioBuffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono)
    {
        XAudio2Buffer xAudio2Buffer = (XAudio2Buffer)buffer;
        xAudio2Buffer.Buffer->AudioBytes = (uint)bufferSize;
			
        xAudio2Buffer.Buffer->PlayBegin = 0;
        xAudio2Buffer.Size = bufferSize / sizeof(short) / (mono ? 1 : 2);
        xAudio2Buffer.Buffer->PlayLength = (uint)xAudio2Buffer.Size;
        
        // Buffer.MemoryCopy(
        //     pcm.ToPointer(),
        //     xAudio2Buffer.Buffer.PAudioData,
        //     xAudio2Buffer.MaxBufferSize,
        //     bufferSize);
    }

    public void SourceSetBuffer(IAudioSource source, IAudioBuffer buffer)
    {
        //this function is called only when the audio source is actually fully cached in memory, so we deal only with the first buffer
        source.Streamed = false;
        ((XAudio2Source)source).FreeBuffers[0] = (XAudio2Buffer)buffer;
        ((XAudio2Source)source).SingleBuffer = ((XAudio2Buffer)buffer).Buffer;
        ((XAudio2Source)source).sourceVoice->SubmitSourceBuffer(((XAudio2Source)source).SingleBuffer, null);
    }

    public void SourceFlushBuffers(IAudioSource source)
    {
        ((XAudio2Source)source).sourceVoice->FlushSourceBuffers();
    }

    public void SourceQueueBuffer(IAudioSource source, IAudioBuffer buffer, IntPtr pcm, int bufferSize, BufferType streamType)
    {
        //used only when streaming, to fill a buffer, often..
        source.Streamed = true;
        
        XAudio2Source xAudio2Source = (XAudio2Source)source;
        XAudio2Buffer xAudio2Buffer = (XAudio2Buffer)buffer;

        //flag the stream
        xAudio2Buffer.Buffer->Flags = streamType == BufferType.EndOfStream ? (uint)XAudio.EndOfStream : 0;
        xAudio2Buffer.Type = streamType;
			
        xAudio2Buffer.Size = bufferSize;
        xAudio2Buffer.Buffer->AudioBytes = (uint)xAudio2Buffer.Size;
        xAudio2Buffer.Buffer->PAudioData = (byte*)pcm.ToPointer();
        xAudio2Source.sourceVoice->SubmitSourceBuffer(xAudio2Buffer.Buffer, null);
    }

    public IAudioBuffer SourceGetFreeBuffer(IAudioSource source)
    {
        XAudio2Buffer buffer = null;
        XAudio2Source xAudio2Source = (XAudio2Source)source;
        for (int i = 0; i < xAudio2Source.FreeBuffers.Count; i++)
        {
            if (xAudio2Source.FreeBuffers[i] != null)
            {
                buffer = xAudio2Source.FreeBuffers[i];
                xAudio2Source.FreeBuffers[i] = null;
                break;
            }
        }
			
        return buffer;
    }

    public void SourcePlay(IAudioSource source)
    {
        XAudio2Source xAudio2Source = (XAudio2Source)source;
        xAudio2Source.sourceVoice->Start(0,0);
        xAudio2Source.Playing = true;

        if(!xAudio2Source.Streamed && !xAudio2Source.Pause)
        {
            VoiceState state = new();
            xAudio2Source.sourceVoice->GetState(&state, 0);
            xAudio2Source.SamplesAtBegin = (int)state.SamplesPlayed;
        }

        xAudio2Source.Pause = false;
    }

    public void SourcePause(IAudioSource source)
    {
        XAudio2Source xAudio2Source = (XAudio2Source)source;
        xAudio2Source.sourceVoice->Stop(0,0);
        xAudio2Source.Playing = false;
        xAudio2Source.Pause = true;
    }

    public void SourceStop(IAudioSource source)
    {
        XAudio2Source xAudio2Source = (XAudio2Source)source;
        xAudio2Source.sourceVoice->Stop(0, 0);
        xAudio2Source.sourceVoice->FlushSourceBuffers();
        xAudio2Source.Playing = false;
        xAudio2Source.Pause = false;

        //since we flush we also rebuffer in this case
        if (!source.Streamed)
        {
            xAudio2Source.sourceVoice->SubmitSourceBuffer(xAudio2Source.SingleBuffer, null);
        }
    }

    public void SourceSetLooping(IAudioSource source, bool looped)
    {
        XAudio2Source xAudio2Source = (XAudio2Source)source;
        xAudio2Source.Looped = looped;

        if (!xAudio2Source.Streamed)
        {
            if (!xAudio2Source.Looped)
            {
                xAudio2Source.SingleBuffer->LoopBegin = 0;
                xAudio2Source.SingleBuffer->LoopLength = 0;
                xAudio2Source.SingleBuffer->LoopCount = 0;
                xAudio2Source.SingleBuffer->Flags = XAudio.EndOfStream;
            }
            else
            {
                xAudio2Source.SingleBuffer->LoopBegin = xAudio2Source.SingleBuffer->PlayBegin;
                xAudio2Source.SingleBuffer->LoopLength = xAudio2Source.SingleBuffer->PlayLength;
                xAudio2Source.SingleBuffer->LoopCount = XAudio.LoopInfinite;
                xAudio2Source.SingleBuffer->Flags = 0;
            }

            xAudio2Source.sourceVoice->FlushSourceBuffers();
            xAudio2Source.sourceVoice->SubmitSourceBuffer(xAudio2Source.SingleBuffer, null);
        }
    }

    public void SourceSetRange(IAudioSource source, double startTime, double stopTime)
    {
        XAudio2Source xAudio2Source = (XAudio2Source)source;
        if(!source.Streamed)
        {
            var singleBuffer = xAudio2Source.FreeBuffers[0];
            if(startTime == 0 && stopTime == 0)
            {
                xAudio2Source.SingleBuffer->PlayBegin = 0;
                xAudio2Source.SingleBuffer->PlayLength = (uint)singleBuffer.Size;
            }
            else
            {					
                var sampleStart = (int)(source.SampleRate * startTime);
                var sampleStop = (int)(source.SampleRate * stopTime);

                if (sampleStart > singleBuffer.Size)
                {
                    return; //the starting position must be less then the total length of the buffer
                }

                if (sampleStop > singleBuffer.Size) //if the end point is more then the length of the buffer fix the value
                {
                    sampleStop = singleBuffer.Size;
                }

                uint len = (uint)(sampleStop - sampleStart);
                if (len > 0)
                {
                    xAudio2Source.SingleBuffer->PlayBegin = (uint)sampleStart;
                    xAudio2Source.SingleBuffer->PlayLength = len;
                }
            }

            //sort looping properties and re-submit buffer
            xAudio2Source.sourceVoice->Stop(0,0);
            SourceSetLooping(source, xAudio2Source.Looped);
        }
    }

    public void SourceSetGain(IAudioSource source, float gain)
    {
        ((XAudio2Source)source).sourceVoice->SetVolume(gain,0);
    }

    public void SourceSetPitch(IAudioSource source, float pitch)
    {
        ((XAudio2Source)source).Pitch = pitch;
        ((XAudio2Source)source).sourceVoice->SetFrequencyRatio(((XAudio2Source)source).DopplerPitch * ((XAudio2Source)source).Pitch, 0);
    }

    public void ListenerPush3D(IAudioListener listener, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
        XAudio2Listener xAudio2Listener = (XAudio2Listener)listener;
        xAudio2Listener.XAudioListener.Position = pos;
        xAudio2Listener.XAudioListener.Velocity = vel;
        xAudio2Listener.XAudioListener.OrientFront = forward;
        xAudio2Listener.XAudioListener.OrientTop = up;
        xAudio2Listener.WorldTransform = worldTransform;
    }

    public void SourcePush3D(IAudioSource source, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
  //       if(source.hrtf_params != null)
		// {
  //           Matrix invListener = source.Listener.WorldTransform;
  //           Matrix.Invert(ref invListener, out invListener);
  //           Matrix.Multiply(ref worldTransform, ref invListener, out var localTransform);
  //
  //           HrtfPosition hrtfEmitterPos = new() { x = localTransform.M41, y = localTransform.M42, z = localTransform.M43 };
  //           source.hrtf_params->SetSourcePosition(ref hrtfEmitterPos);
  //
		// 	// //set orientation, relative to head, already computed c# side, todo c++ side
		// 	HrtfOrientation hrtfEmitterRot = new(){ 
		// 		element = [
		// 		localTransform.M11, localTransform.M12, localTransform.M13,
		// 		localTransform.M21, localTransform.M22, localTransform.M23,
		// 		localTransform.M31, localTransform.M32, localTransform.M33 ]
		// 	};
		// 	source.hrtf_params->SetSourceOrientation(ref hrtfEmitterRot);
		// }
		// else if (source.emitter != null) 
		// {
		// 	source.emitter.Position = pos;
		// 	source.emitter.Velocity = vel;
		// 	source.emitter.OrientFront = forward;
		// 	source.emitter.OrientTop = up;
  //
		// 	//everything is calculated by Xaudio for us
  //           fixed(X3DAudioListener* listenerPtr = &source.Listener.listener)
  //           {
  //               X3DAudio.X3DAudioCalculate(out source.Listener.device.x3_audio, listenerPtr, source.emitter,
  //                   Calculate.Matrix | Calculate.Doppler | Calculate.LPF_Direct | Calculate.Reverb, out source.dsp_settings);
  //           }
  //
		// 	source.sourceVoice->SetOutputMatrix((IXAudio2Voice*)source.masteringVoice, 1, AudioChannels, source.dsp_settings.pMatrixCoefficients, 0);
		// 	source.dopplerPitch = source.dsp_settings.DopplerFactor;
		// 	source.sourceVoice->SetFrequencyRatio(source.dsp_settings.DopplerFactor * source.Pitch, 0);
  //           FilterParameters filter_parameters = new(FilterType.LowPassFilter,
  //               2.0f * MathF.Sin(MathF.PI / 6.0f * source.dsp_settings.LPFDirectCoefficient),
  //               1.0f);
  //           source.sourceVoice->SetFilterParameters(ref filter_parameters, 0);
		// }
    }

    public bool SourceIsPlaying(IAudioSource source)
    {
        return ((XAudio2Source)source).Playing || ((XAudio2Source)source).Pause;
    }
}
