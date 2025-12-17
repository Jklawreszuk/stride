// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Silk.NET.XAudio;

namespace Stride.Audio;

public unsafe class XAudio2Device : IAudioDevice
{
    public IXAudio2* xAudio;
    public nint x3_audio;
    public IXAudio2MasteringVoice* masteringVoice;
    public bool Hrtf;
}
