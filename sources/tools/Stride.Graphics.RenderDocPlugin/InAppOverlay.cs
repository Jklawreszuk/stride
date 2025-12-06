// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Stride.Graphics;

[Flags]
internal enum InAppOverlay : uint
{
    Enabled = 0x1,
    FrameRate = 0x2,
    FrameNumber = 0x4,
    CaptureList = 0x8,
    Default = (Enabled | FrameRate | FrameNumber | CaptureList),
    All = 0xFFFFFFFF,
    None = 0,
}
