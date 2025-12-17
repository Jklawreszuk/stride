// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using Silk.NET.XAudio;

namespace Stride.Audio;

public class XAudio2Buffer : IAudioBuffer
{
    public Buffer Buffer;
    public int Size;
    public BufferType Type { get; set; }
}
