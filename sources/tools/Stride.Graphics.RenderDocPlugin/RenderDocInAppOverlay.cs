// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Stride.Graphics;

[Flags]
internal enum RenderDocInAppOverlay : uint
{
    eOverlay_Enabled = 0x1,
    eOverlay_FrameRate = 0x2,
    eOverlay_FrameNumber = 0x4,
    eOverlay_CaptureList = 0x8,
    eOverlay_Default = (eOverlay_Enabled | eOverlay_FrameRate | eOverlay_FrameNumber | eOverlay_CaptureList),
    eOverlay_All = 0xFFFFFFFF,
    eOverlay_None = 0,
};

   

