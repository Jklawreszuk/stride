// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Audio;

public class StrideAudioBuffer
{
    public short[] Pcm { get; set; }
    public int Size { get; set; }
    public int SampleRate { get; set; }
    public uint Value;
    public BufferType Type { get; set; }
}
