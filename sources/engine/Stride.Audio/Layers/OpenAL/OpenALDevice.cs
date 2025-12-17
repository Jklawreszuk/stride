// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.OpenAL;

namespace Stride.Audio;

internal unsafe struct OpenALDevice() : IAudioDevice
{
    public Device* Value { get; set; }
    public SpinLock DeviceLock { get; set; } = new();
    public List<OpenALLister> Listeners { get; set; } = [];
}
