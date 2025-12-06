// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Stride.Graphics;

internal enum RenderDocApiFunction
{
    GetAPIVersion,

    SetCaptureOptionU32,
    SetCaptureOptionF32,

    GetCaptureOptionU32,
    GetCaptureOptionF32,

    SetFocusToggleKeys,
    SetCaptureKeys,

    GetOverlayBits,
    MaskOverlayBits,

    RemoveHooks,
    UnloadCrashHandler,

    SetCaptureFilePathTemplate,
    GetCaptureFilePathTemplate,

    GetNumCaptures,
    GetCapture,

    TriggerCapture,

    IsTargetControlConnected,
    LaunchReplayUI,

    SetActiveWindow,

    StartFrameCapture,
    IsFrameCapturing,
    EndFrameCapture,

    TriggerMultiFrameCapture,
    SetCaptureFileComments,
    DiscardFrameCapture,
}
