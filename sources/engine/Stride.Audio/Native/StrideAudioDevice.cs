// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Collections.Generic;
using Silk.NET.OpenAL;

namespace Stride.Audio;

public unsafe class StrideAudioDevice
{
    public Device* Value { get; set; } //OPENAL, TODO: REFACTOR THIS 
    public SpinLock DeviceLock { get; set; } = new();
    public List<StrideAudioListener> Listeners { get; set; } = [];
}
