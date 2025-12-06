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
