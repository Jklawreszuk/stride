// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Runtime.InteropServices;

namespace Stride.Graphics;    

[StructLayout(LayoutKind.Sequential)]
internal struct RenderDocAPIFunctions
{
  /// FUNCTIONS ///
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate void RENDERDOC_GetAPIVersion(ref int major, ref int minor, ref int patch);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate int RENDERDOC_SetCaptureOptionU32(int captureOption, uint value);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate int RENDERDOC_SetCaptureOptionF32(int captureOption, float value);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate uint RENDERDOC_GetCaptureOptionU32(int captureOption);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate float RENDERDOC_GetCaptureOptionF32(int captureOption);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate void RENDERDOC_SetFocusToggleKeys(ref RenderDocKeyButton keys, int num);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate void RENDERDOC_SetCaptureKeys(ref RenderDocKeyButton keys, int num);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate RenderDocInAppOverlay RENDERDOC_GetOverlayBits();
  
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate void RENDERDOC_MaskOverlayBits(RenderDocInAppOverlay And, RenderDocInAppOverlay Or);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
  public delegate void RENDERDOC_RemoveHooks();

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate void RENDERDOC_UnloadCrashHandler();

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate void RENDERDOC_SetCaptureFilePathTemplate(string logfile);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate string RENDERDOC_GetCaptureFilePathTemplate();

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_GetNumCaptures();

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_GetCapture(uint idx, string fileName, ref uint pathLen, ref uint timeStamp);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate void RENDERDOC_TriggerCapture();

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_IsTargetControlConnected();

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_LaunchReplayUI(uint connectTargetControl, string cmdline);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_SetActiveWindow(nint devicePointer, nint wndHandle);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate void RENDERDOC_StartFrameCapture(nint devicePointer, nint wndHandle);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_IsFrameCapturing();

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_EndFrameCapture(nint devicePointer, nint wndHandle);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate void RENDERDOC_TriggerMultiFrameCapture(uint numberOfFrames);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate void RENDERDOC_SetCaptureFileComments(string filePath, string comments);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_DiscardFrameCapture(nint devicePointer, nint wndHandle);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate uint RENDERDOC_ShowReplayUI();

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate void RENDERDOC_SetCaptureTitle(string title);
  
  // DELEGATES

  public RENDERDOC_GetAPIVersion GetAPIVersion;
  public RENDERDOC_SetCaptureOptionU32 SetCaptureOptionU32;
  public RENDERDOC_SetCaptureOptionF32 SetCaptureOptionF32;
  public RENDERDOC_GetCaptureOptionU32 GetCaptureOptionU32;
  public RENDERDOC_GetCaptureOptionF32 GetCaptureOptionF32;
  public RENDERDOC_SetFocusToggleKeys SetFocusToggleKeys;
  public RENDERDOC_SetCaptureKeys SetCaptureKeys;
  public RENDERDOC_GetOverlayBits GetOverlayBits;
  public RENDERDOC_MaskOverlayBits MaskOverlayBits;
  public RENDERDOC_RemoveHooks RemoveHooks;
  public RENDERDOC_UnloadCrashHandler UnloadCrashHandler;
  public RENDERDOC_SetCaptureFilePathTemplate SetCaptureFilePathTemplate;
  public RENDERDOC_GetCaptureFilePathTemplate GetCaptureFilePathTemplate;
  public RENDERDOC_GetNumCaptures GetNumCaptures;
  public RENDERDOC_GetCapture GetCapture;
  public RENDERDOC_TriggerCapture TriggerCapture;
  public RENDERDOC_IsTargetControlConnected IsTargetControlConnected;
  public RENDERDOC_LaunchReplayUI LaunchReplayUI;
  public RENDERDOC_SetActiveWindow SetActiveWindow;
  public RENDERDOC_StartFrameCapture StartFrameCapture;
  public RENDERDOC_IsFrameCapturing IsFrameCapturing;
  public RENDERDOC_EndFrameCapture EndFrameCapture;
  public RENDERDOC_TriggerMultiFrameCapture TriggerMultiFrameCapture;
  public RENDERDOC_SetCaptureFileComments SetCaptureFileComments;
  public RENDERDOC_DiscardFrameCapture DiscardFrameCapture;
  public RENDERDOC_ShowReplayUI ShowReplayUI;
  public RENDERDOC_SetCaptureTitle SetCaptureTitle;
}
