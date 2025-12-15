// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.OpenAL;

namespace Stride.Audio;

public unsafe class StrideAudioListener
{
    public StrideAudioDevice Device { get; set; }
    public Context* Context { get; set; } //OPENAL, TODO REFACTOR THIS 
    public List<StrideAudioSource> Sources { get; set; } = [];
    public Dictionary<uint, StrideAudioBuffer> Buffers { get; set; } = [];
}
