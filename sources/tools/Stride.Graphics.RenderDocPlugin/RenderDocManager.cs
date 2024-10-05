// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Stride.Graphics;
public partial class RenderDocManager
{
    private bool isCaptureStarted;
    private readonly RenderDocPointers apiPointers;

    public unsafe bool IsInitialized
    {
        get { return apiPointers != null; }
    }

    // Matching https://github.com/baldurk/renderdoc/blob/master/renderdoc/api/app/renderdoc_app.h

    public unsafe RenderDocManager()
    {
        string path = 
            OperatingSystem.IsWindows() ? 
                GetRenderDocPathWindows() :
            OperatingSystem.IsLinux() ?
                GetRenderDocPathLinux() :
                null;

        if (path == null)
            throw new FileNotFoundException("Cannot find RenderDoc installation or your OS is not supported");

        // Preload the library before using the UnmanagedFunctionPointerAttribute
        if (!NativeLibrary.TryLoad(path, out var libHandle))
        {
            return;
        }

        if (!NativeLibrary.TryGetExport(libHandle, "RENDERDOC_GetAPI", out IntPtr rdApiHandle))
            return;    

        // Get main entry point to get other function pointers
        var getAPI = Marshal.GetDelegateForFunctionPointer<RENDERDOC_GetAPI>(rdApiHandle);

        // API version 10600 (1.35) has 27 function pointers
        if (!getAPI(RENDERDOC_API_VERSION_1_6_0, ref RenderDocPointers))
            return;  
    }

    private static unsafe string GetRenderDocPathWindows()
    {            
        const string RenderdocClsid = "{5D6BF029-A6BA-417A-8523-120492B1DCE3}";
        var reg = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("CLSID\\" + RenderdocClsid + "\\InprocServer32");
        if (reg == null)
        {
            return null;
        }
        var path = reg.GetValue(null)?.ToString();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return null;
        }
        return path;
    }

    private static unsafe string GetRenderDocPathLinux()
    {            
        return "/usr/lib64/renderdoc/librenderdoc.so";
    }

    public unsafe void Initialize(string captureFilePath = null)
    {
        var finalLogFilePath = captureFilePath ?? FindAvailablePath("RenderDoc" + Assembly.GetEntryAssembly().Location);
        GetMethod<RENDERDOC_SetCaptureFilePathTemplate>(RenderDocAPIFunction.SetCaptureFilePathTemplate)(finalLogFilePath);

        var focusToggleKey = RenderDocKeyButton.F11;
        GetMethod<RENDERDOC_SetFocusToggleKeys>(RenderDocAPIFunction.SetFocusToggleKeys)(ref focusToggleKey, 1);
        var captureKey = RenderDocKeyButton.F12;
        GetMethod<RENDERDOC_SetCaptureKeys>(RenderDocAPIFunction.SetCaptureKeys)(ref captureKey, 1);
    }

    public void RemoveHooks()
    {
        if (IsInitialized)
        {
            GetMethod<RENDERDOC_RemoveHooks>(RenderDocAPIFunction.RemoveHooks)();
        }
    }

    public void StartFrameCapture(GraphicsDevice graphicsDevice = null, IntPtr hwndPtr = 0)
    {
        GetMethod<RENDERDOC_StartFrameCapture>(RenderDocAPIFunction.StartFrameCapture)(GetDevicePointer(graphicsDevice), hwndPtr);
        isCaptureStarted = true;
    }

    public void EndFrameCapture(GraphicsDevice graphicsDevice = null, IntPtr hwndPtr = 0)
    {
        if (!isCaptureStarted)
            return;

        GetMethod<RENDERDOC_EndFrameCapture>(RenderDocAPIFunction.EndFrameCapture)(GetDevicePointer(graphicsDevice), hwndPtr);
        isCaptureStarted = false;
    }

    public void DiscardFrameCapture(GraphicsDevice graphicsDevice = null, IntPtr hwndPtr = 0)
    {
        if (!isCaptureStarted)
            return;

        GetMethod<RENDERDOC_DiscardFrameCapture>(RenderDocAPIFunction.DiscardFrameCapture)(GetDevicePointer(graphicsDevice), hwndPtr);
        isCaptureStarted = false;
    }

    private static IntPtr GetDevicePointer(GraphicsDevice graphicsDevice)
    {
        var devicePointer = IntPtr.Zero;
#if STRIDE_GRAPHICS_API_DIRECT3D11 || STRIDE_GRAPHICS_API_DIRECT3D12
        if (graphicsDevice != null)
            devicePointer = ((SharpDX.CppObject)SharpDXInterop.GetNativeDevice(graphicsDevice)).NativePointer;
#endif
        return devicePointer;
    }

    private unsafe TDelegate GetMethod<TDelegate>(RenderDocAPIFunction function)
    {
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(apiPointers[(int)function]);
    }

    private static string FindAvailablePath(string logFilePath)
    {
        var filePath = Path.GetFileNameWithoutExtension(logFilePath);
        for (int i = 0; i < 1000000; i++)
        {
            var path = filePath;
            if (i > 0)
            {
                path += i;
            }
            path += ".rdc";

            if (!File.Exists(path))
            {
                return Path.Combine(Path.GetDirectoryName(logFilePath), path);
            }
        }
        return logFilePath;
    }

    [Flags]
    private enum InAppOverlay : uint
    {
        eOverlay_Enabled = 0x1,
        eOverlay_FrameRate = 0x2,
        eOverlay_FrameNumber = 0x4,
        eOverlay_CaptureList = 0x8,
        eOverlay_Default = (eOverlay_Enabled | eOverlay_FrameRate | eOverlay_FrameNumber | eOverlay_CaptureList),
        eOverlay_All = 0xFFFFFFFF,
        eOverlay_None = 0,
    };


    // API breaking change history:
    // Version 1 -> 2 - strings changed from wchar_t* to char* (UTF-8)
    private const int RENDERDOC_API_VERSION_1_4_0 = 10400;

    //////////////////////////////////////////////////////////////////////////
    // In-program functions
    //////////////////////////////////////////////////////////////////////////
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private unsafe delegate bool RENDERDOC_GetAPI(int version, ref IntPtr* apiPointers);
}

[StructLayout(LayoutKind.Sequential)]
internal struct RenderDocPointers
{
     [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RENDERDOC_GetAPIVersion();
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RENDERDOC_RemoveHooks();

    RENDERDOC_GetAPIVersion GetAPIVersion;

    internal RENDERDOC_SetCaptureOptionU32 SetCaptureOptionU32;
    RENDERDOC_SetCaptureOptionF32 SetCaptureOptionF32;

    RENDERDOC_GetCaptureOptionU32 GetCaptureOptionU32;
    RENDERDOC_GetCaptureOptionF32 GetCaptureOptionF32;

    RENDERDOC_SetFocusToggleKeys SetFocusToggleKeys;
    RENDERDOC_SetCaptureKeys SetCaptureKeys;

    RENDERDOC_GetOverlayBits GetOverlayBits;
    RENDERDOC_MaskOverlayBits MaskOverlayBits;

    RENDERDOC_RemoveHooks RemoveHooks;
    RENDERDOC_UnloadCrashHandler UnloadCrashHandler;
    RENDERDOC_SetCaptureFilePathTemplate SetCaptureFilePathTemplate;
    RENDERDOC_GetCaptureFilePathTemplate GetCaptureFilePathTemplate;
    RENDERDOC_GetNumCaptures GetNumCaptures;
    RENDERDOC_GetCapture GetCapture;

    RENDERDOC_TriggerCapture TriggerCapture;
    RENDERDOC_IsTargetControlConnected IsTargetControlConnected;
    RENDERDOC_LaunchReplayUI LaunchReplayUI;

    RENDERDOC_SetActiveWindow SetActiveWindow;

    RENDERDOC_StartFrameCapture StartFrameCapture;
    RENDERDOC_IsFrameCapturing IsFrameCapturing;
    RENDERDOC_EndFrameCapture EndFrameCapture;

    // new function in 1.1.0
    RENDERDOC_TriggerMultiFrameCapture TriggerMultiFrameCapture;

    // new function in 1.2.0
    RENDERDOC_SetCaptureFileComments SetCaptureFileComments;

    // new function in 1.4.0
    RENDERDOC_DiscardFrameCapture DiscardFrameCapture;

    // new function in 1.5.0
    RENDERDOC_ShowReplayUI ShowReplayUI;

    // new function in 1.6.0
    RENDERDOC_SetCaptureTitle SetCaptureTitle;
}