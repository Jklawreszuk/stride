// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Stride.Graphics;

public class RenderDocApi
{
    internal const int RENDERDOC_API_VERSION = 10600;
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal unsafe delegate bool RENDERDOC_GetAPI(int version, ref IntPtr* apiPointers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_RemoveHooks();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_SetCaptureFilePathTemplate([MarshalAs(UnmanagedType.LPStr)] string logfile);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate string RENDERDOC_GetLogFilePathTemplate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate bool RENDERDOC_GetCapture(int idx, [MarshalAs(UnmanagedType.LPStr)] string logfile, out int pathlength, out long timestamp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_SetActiveWindow(IntPtr devicePointer, IntPtr wndHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_TriggerCapture();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_StartFrameCapture(IntPtr devicePointer, IntPtr wndHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate bool RENDERDOC_EndFrameCapture(IntPtr devicePointer, IntPtr wndHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate InAppOverlay RENDERDOC_GetOverlayBits();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_MaskOverlayBits(InAppOverlay And, InAppOverlay Or);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_SetFocusToggleKeys(ref KeyButton keys, int num);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_SetCaptureKeys(ref KeyButton keys, int num);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate void RENDERDOC_UnloadCrashHandler();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate bool RENDERDOC_DiscardFrameCapture(IntPtr devicePointer, IntPtr wndHandle);
}
