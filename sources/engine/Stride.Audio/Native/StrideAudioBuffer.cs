// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Silk.NET.XAudio;

namespace Stride.Audio;

public class StrideAudioBuffer
{
    public short[] Pcm { get; set; }
    public int Size { get; set; }
    public int SampleRate { get; set; }
    public uint Value;
    public BufferType Type { get; set; }
    
    //XAudio2
    public Buffer Buffer;
    public ulong PlayBegin { get; set; }
    public ulong LoopBegin { get; set; }
    public int LoopLength { get; set; }
    public int LoopCount { get; set; }
    public int Flags { get; set; }
    public int PlayLength { get; set; }
}
