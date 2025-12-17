// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Audio;

public interface IAudioSource
{
    public int SampleRate { get; set; }
    public bool Mono { get; set; }
    public bool Streamed { get; set; }
}
