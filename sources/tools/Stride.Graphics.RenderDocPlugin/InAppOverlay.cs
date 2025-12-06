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
