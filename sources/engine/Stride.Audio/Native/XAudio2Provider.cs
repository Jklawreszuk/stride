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
    
    public StrideAudioDevice Create(string deviceName, DeviceFlags flags)
    {
        var device = new StrideAudioDevice { Hrtf = flags == DeviceFlags.Hrtf };

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

    public void Destroy(StrideAudioDevice device)
    {
        device.xAudio->StopEngine();
        device.masteringVoice->DestroyVoice();
    }

    public void Update(StrideAudioDevice device)
    {
        
    }

    public void SetMasterVolume(StrideAudioDevice device, float volume)
    {
        device.masteringVoice->SetVolume(volume,0);
    }

    public StrideAudioListener ListenerCreate(StrideAudioDevice device)
    {
        var listener = new StrideAudioListener();
        listener.Device = device;
        listener.XAudioListener = new X3DAUDIO_LISTENER();
        return listener;
    }

    public void ListenerDestroy(StrideAudioListener listener)
    {
        //nothing to destroy
    }

    public bool ListenerEnable(StrideAudioListener listener)
    {
        return true;
    }

    public void ListenerDisable(StrideAudioListener listener)
    {
        //unused in XAudio2
    }

    public StrideAudioSource SourceCreate(StrideAudioListener listener, int sampleRate, int maxNumberOfBuffers, bool mono, bool spatialized, bool streamed, bool hrtf, float hrtfDirectionFactor,
        HrtfEnvironment environment)
    {
        var source = new StrideAudioSource
        {
            Listener = listener,
            SampleRate = sampleRate,
            Mono = mono,
            Streamed = streamed,
            masteringVoice = listener.Device.masteringVoice
        };

        if((spatialized && !hrtf) || (hrtf && !source.Listener.Device.Hrtf))
		{
			//if spatialized we also need those structures to calculate 3D audio
			source.emitter = new X3DAUDIO_EMITTER();
			source.emitter.ChannelCount = 1;
			source.emitter.CurveDistanceScaler = 1;
			source.emitter.DopplerScaler = 1;

			source.dsp_settings = new X3DAUDIO_DSP_SETTINGS();
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
        
        int result = listener.Device.xAudio->CreateSourceVoice(ref source.sourceVoice, &pcmWaveFormat, 0, MaxFreqRatio, null, null, null);
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

    public void SourceDestroy(StrideAudioSource source)
    {
        source.sourceVoice->Stop(0,0);
        source.sourceVoice->DestroyVoice();
    }

    public double SourceGetPosition(StrideAudioSource source)
    {
        VoiceState state;
        source.sourceVoice->GetState(&state, 0);

        if (!source.Streamed)
            return (source.SingleBuffer.PlayBegin + state.SamplesPlayed - (ulong)source.SamplesAtBegin) / (float)source.SampleRate;
			
        //things work different for streamed sources, but anyway we simply subtract the snapshotted samples at begin of the stream ( could be the begin of the loop )
        return (state.SamplesPlayed - (ulong)source.SamplesAtBegin) / (float)source.SampleRate;
    }

    public void SourceSetPan(StrideAudioSource source, float pan)
    {
        float leftGain  = pan <= 0 ? 1.0f : 1.0f - pan;
        float rightGain = pan >= 0 ? 1.0f : 1.0f + pan;
        
        uint sourceChannels = source.Mono ? 1u : 2u;
        float[] panning = source.Mono ? [ leftGain, rightGain ] : [ leftGain, 0, 0, rightGain ];
       
        fixed(float* panningPtr = &panning[0])
        {
            source.sourceVoice->SetOutputMatrix((IXAudio2Voice*)source.masteringVoice, sourceChannels, AudioChannels, panningPtr, 0);
        }
    }

    public StrideAudioBuffer BufferCreate(int maxBufferSize)
    {
        var buffer = new StrideAudioBuffer();

        buffer.PAudioDataIntPtr = Marshal.AllocHGlobal(maxBufferSize);
        buffer.Buffer.PAudioData = (byte*)buffer.PAudioDataIntPtr;
        buffer.MaxBufferSize = maxBufferSize;
        return buffer;
    }

    public void BufferDestroy(StrideAudioBuffer buffer)
    {
        if (buffer.PAudioDataIntPtr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(buffer.PAudioDataIntPtr);
            buffer.PAudioDataIntPtr = IntPtr.Zero;
        }
    }


    public void BufferFill(StrideAudioBuffer buffer, IntPtr pcm, int bufferSize, int sampleRate, bool mono)
    {
        buffer.Buffer.AudioBytes = (uint)bufferSize;
			
        buffer.Buffer.PlayBegin = 0;
        buffer.Size = bufferSize / sizeof(short) / (mono ? 1 : 2);
        buffer.Buffer.PlayLength = (uint)buffer.Size;
        
        Buffer.MemoryCopy(
            pcm.ToPointer(),
            buffer.Buffer.PAudioData,
            buffer.MaxBufferSize,
            bufferSize);
    }

    public void SourceSetBuffer(StrideAudioSource source, StrideAudioBuffer buffer)
    {
        //this function is called only when the audio source is actually fully cached in memory, so we deal only with the first buffer
        source.Streamed = false;
        source.FreeBuffers[0] = buffer;
        source.Buffer = buffer.Buffer;
        source.sourceVoice->SubmitSourceBuffer(in source.Buffer, null);
    }

    public void SourceFlushBuffers(StrideAudioSource source)
    {
        source.sourceVoice->FlushSourceBuffers();
    }

    public void SourceQueueBuffer(StrideAudioSource source, StrideAudioBuffer buffer, IntPtr pcm, int bufferSize, BufferType streamType)
    {
        //used only when streaming, to fill a buffer, often..
        source.Streamed = true;

        //flag the stream
        buffer.Buffer.Flags = streamType == BufferType.EndOfStream ? (uint)XAudio.EndOfStream : 0;
        buffer.Type = streamType;
			
        buffer.Size = bufferSize;
        buffer.Buffer.AudioBytes = (uint)buffer.Size;
        buffer.Buffer.PAudioData = (byte*)pcm.ToPointer();
        source.sourceVoice->SubmitSourceBuffer(in buffer.Buffer, null);
    }

    public StrideAudioBuffer SourceGetFreeBuffer(StrideAudioSource source)
    {
        StrideAudioBuffer buffer = null;
        for (int i = 0; i < source.FreeBuffers.Count; i++)
        {
            if (source.FreeBuffers[i] != null)
            {
                buffer = source.FreeBuffers[i];
                source.FreeBuffers[i] = null;
                break;
            }
        }
			
        return buffer;
    }

    public void SourcePlay(StrideAudioSource source)
    {
        source.sourceVoice->Start(0,0);
        source.Playing = true;

        if(!source.Streamed && !source.Pause)
        {
            VoiceState state = new();
            source.sourceVoice->GetState(&state, 0);
            source.SamplesAtBegin = (int)state.SamplesPlayed;
        }

        source.Pause = false;
    }

    public void SourcePause(StrideAudioSource source)
    {
        source.sourceVoice->Stop(0,0);
        source.Playing = false;
        source.Pause = true;
    }

    public void SourceStop(StrideAudioSource source)
    {
        source.sourceVoice->Stop(0, 0);
        source.sourceVoice->FlushSourceBuffers();
        source.Playing = false;
        source.Pause = false;

        //since we flush we also rebuffer in this case
        if (!source.Streamed)
        {
            source.sourceVoice->SubmitSourceBuffer(in source.SingleBuffer.Buffer, null);
        }
    }

    public void SourceSetLooping(StrideAudioSource source, bool looped)
    {
        source.Looped = looped;

        if (!source.Streamed)
        {
            if (!source.Looped)
            {
                source.Buffer.LoopBegin = 0;
                source.Buffer.LoopLength = 0;
                source.Buffer.LoopCount = 0;
                source.Buffer.Flags = XAudio.EndOfStream;
            }
            else
            {
                source.Buffer.LoopBegin = source.Buffer.PlayBegin;
                source.Buffer.LoopLength = source.Buffer.PlayLength;
                source.Buffer.LoopCount = XAudio.LoopInfinite;
                source.Buffer.Flags = 0;
            }

            source.sourceVoice->FlushSourceBuffers();
            source.sourceVoice->SubmitSourceBuffer(in source.Buffer, null);
        }
    }

    public void SourceSetRange(StrideAudioSource source, double startTime, double stopTime)
    {
        if(!source.Streamed)
        {
            var singleBuffer = source.FreeBuffers[0];
            if(startTime == 0 && stopTime == 0)
            {
                source.Buffer.PlayBegin = 0;
                source.Buffer.PlayLength = (uint)singleBuffer.Size;
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
                    source.Buffer.PlayBegin = (uint)sampleStart;
                    source.Buffer.PlayLength = len;
                }
            }

            //sort looping properties and re-submit buffer
            source.sourceVoice->Stop(0,0);
            SourceSetLooping(source, source.Looped);
        }
    }

    public void SourceSetGain(StrideAudioSource source, float gain)
    {
        source.sourceVoice->SetVolume(gain,0);
    }

    public void SourceSetPitch(StrideAudioSource source, float pitch)
    {
        source.Pitch = pitch;
        source.sourceVoice->SetFrequencyRatio(source.DopplerPitch * source.Pitch, 0);
    }

    public void ListenerPush3D(StrideAudioListener listener, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
    {
        listener.XAudioListener.Position = pos;
        listener.XAudioListener.Velocity = vel;
        listener.XAudioListener.OrientFront = forward;
        listener.XAudioListener.OrientTop = up;
        listener.WorldTransform = worldTransform;
    }

    public void SourcePush3D(StrideAudioSource source, Vector3 pos, Vector3 forward, Vector3 up, Vector3 vel, Matrix worldTransform)
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

    public bool SourceIsPlaying(StrideAudioSource source)
    {
        return source.Playing || source.Pause;
    }
}
