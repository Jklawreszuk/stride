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
    private readonly RenderDocAPIFunctions apiPointers;

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
        if (!getAPI(RENDERDOC_API_VERSION_1_6_0, ref apiPointers))
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
        apiPointers.SetCaptureFilePathTemplate(finalLogFilePath);

        var focusToggleKey = RenderDocKeyButton.F11;
        apiPointers.SetFocusToggleKeys(ref focusToggleKey, 1);
        var captureKey = RenderDocKeyButton.F12;
        apiPointers.SetCaptureKeys(ref captureKey, 1);
    }

    public void RemoveHooks()
    {
        if (IsInitialized)
        {
            apiPointers.RemoveHooks();
        }
    }

    public void StartFrameCapture(IntPtr graphicsDevice = 0, IntPtr hwndPtr = 0)
    {
        apiPointers.StartFrameCapture(GetDevicePointer(graphicsDevice), hwndPtr);
        isCaptureStarted = true;
    }

    public void EndFrameCapture(IntPtr IntPtr = 0, IntPtr hwndPtr = 0)
    {
        if (!isCaptureStarted)
            return;

        apiPointers.EndFrameCapture(GetDevicePointer(graphicsDevice), hwndPtr);
        isCaptureStarted = false;
    }

    public void DiscardFrameCapture(IntPtr IntPtr = 0, IntPtr hwndPtr = 0)
    {
        if (!isCaptureStarted)
            return;

        apiPointers.DiscardFrameCapture(GetDevicePointer(graphicsDevice), hwndPtr);
        isCaptureStarted = false;
    }

    private static IntPtr GetDevicePointer(IntPtr IntPtr)
    {
        var devicePointer = IntPtr.Zero;
// #if STRIDE_GRAPHICS_API_DIRECT3D11 || STRIDE_GRAPHICS_API_DIRECT3D12
//         if (IntPtr != null)
//             devicePointer = ((SharpDX.CppObject)SharpDXInterop.GetNativeDevice(IntPtr)).NativePointer;
// #endif
        return devicePointer;
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


    // API breaking change history:
    // Version 1 -> 2 - strings changed from wchar_t* to char* (UTF-8)
    private const int RENDERDOC_API_VERSION_1_6_0 = 10600;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate bool RENDERDOC_GetAPI(int version, ref RenderDocAPIFunctions apiPointers);
}
