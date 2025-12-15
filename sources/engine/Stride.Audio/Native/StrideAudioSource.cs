// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;

namespace Stride.Audio;

public class StrideAudioSource
{
    public uint Value;
    public int SampleRate { get; set; }
    public bool Mono { get; set; }
    public bool Streamed { get; set; }
    public float DequeuedTime { get; set; }
    public StrideAudioListener Listener { get; set; }
    public StrideAudioBuffer SingleBuffer { get; set; }
    public List<StrideAudioBuffer> FreeBuffers { get; set; } = [];
}
