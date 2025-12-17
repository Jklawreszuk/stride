// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Audio;

internal class OpenALBuffer : IAudioBuffer
{
    public uint Value;
    public short[] Pcm { get; set; }
    public int Size { get; set; }
    public int SampleRate { get; set; }
    public BufferType Type { get; set; }
}
