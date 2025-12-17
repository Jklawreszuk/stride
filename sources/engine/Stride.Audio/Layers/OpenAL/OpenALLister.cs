// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.OpenAL;

namespace Stride.Audio;

internal unsafe struct OpenALLister() : IAudioListener
{
    public OpenALDevice Device;
    public Context* Context { get; set; }
    public List<OpenALSource> Sources { get; set; } = [];
    public Dictionary<uint, OpenALBuffer> Buffers { get; set; } = [];
}
