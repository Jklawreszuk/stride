// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;

namespace Stride.Audio;

internal class OpenALSource() : IAudioSource
{
    public uint Value;
    public int SampleRate { get; set; }
    public bool Mono { get; set; }
    public bool Streamed { get; set; }

    public double DequeuedTime { get; set; } = 0.0;

    public OpenALLister Listener;

    public OpenALBuffer SingleBuffer;

    public List<OpenALBuffer> FreeBuffers = [];
}
