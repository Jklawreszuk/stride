// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Stride.Audio;

[Flags]
internal enum Calculate
{
    Matrix = 1,
    Delay = 2,
    LPF_Direct = 4,
    LPF_Reverb = 8,
    Reverb = 16,
    Doppler = 32,
}
