// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace Stride.Graphics
{
    public class RenderDocManager
    {
        private bool isCaptureStarted;
        private unsafe IntPtr* apiPointers;

        private unsafe bool IsInitialized
        {
            get { return apiPointers != null; }
        }

        // Matching https://github.com/baldurk/renderdoc/blob/master/renderdoc/api/app/renderdoc_app.h

        public unsafe RenderDocManager(string captureFilePath = null)
        {
            string path = GetRenderDocPath();
            if (path == null)
            {
                return;
            }

            // Preload the library before using the UnmanagedFunctionPointerAttribute
            if (!NativeLibrary.TryLoad(path, out var libHandle))
            {
                return;
            }

            if (!NativeLibrary.TryGetExport(libHandle, nameof(RenderDocApi.RENDERDOC_GetAPI), out IntPtr getAPIAddress))
                return;

            // Get main entry point to get other function pointers
            var getAPI = Marshal.GetDelegateForFunctionPointer<RenderDocApi.RENDERDOC_GetAPI>(getAPIAddress);

            // API version 10400 has 25 function pointers
            if (!getAPI(RenderDocApi.RENDERDOC_API_VERSION, ref apiPointers))
                return;

            Initialize(captureFilePath);
        }

        private static string GetRenderDocPath()
        {
            if (OperatingSystem.IsWindows())
                return RenderDocPathWindows();
            if (OperatingSystem.IsLinux())
                return RenderDocPathLinux();
            return null;
        }

        private static string RenderDocPathWindows()
        {
            const string RenderdocClsid = "{5D6BF029-A6BA-417A-8523-120492B1DCE3}";
            var reg = Registry.ClassesRoot.OpenSubKey("CLSID\\" + RenderdocClsid + "\\InprocServer32");
            if (reg == null)
            {
                return null;
            }
            string path = reg.GetValue(null) != null ? reg.GetValue(null).ToString() : null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return path;
            }

            return null;
        }

        private static string RenderDocPathLinux()
        {
            return "/usr/lib64/renderdoc/librenderdoc.so";
        }

        private void Initialize(string captureFilePath = null)
        {
            var finalLogFilePath = captureFilePath ?? FindAvailablePath("RenderDoc" + Assembly.GetEntryAssembly().Location);
            GetMethod<RenderDocApi.RENDERDOC_SetCaptureFilePathTemplate>(RenderDocApiFunction.SetCaptureFilePathTemplate)(finalLogFilePath);

            var focusToggleKey = KeyButton.eRENDERDOC_Key_F11;
            GetMethod<RenderDocApi.RENDERDOC_SetFocusToggleKeys>(RenderDocApiFunction.SetFocusToggleKeys)(ref focusToggleKey, 1);
            var captureKey = KeyButton.eRENDERDOC_Key_F12;
            GetMethod<RenderDocApi.RENDERDOC_SetCaptureKeys>(RenderDocApiFunction.SetCaptureKeys)(ref captureKey, 1);
        }

        public void RemoveHooks()
        {
            if (IsInitialized)
            {
                GetMethod<RenderDocApi.RENDERDOC_RemoveHooks>(RenderDocApiFunction.RemoveHooks)();
            }
        }

        public void StartFrameCapture(GraphicsDevice graphicsDevice, IntPtr hwndPtr)
        {
            GetMethod<RenderDocApi.RENDERDOC_StartFrameCapture>(RenderDocApiFunction.StartFrameCapture)(GetDevicePointer(graphicsDevice), hwndPtr);
            isCaptureStarted = true;
        }

        public void EndFrameCapture(GraphicsDevice graphicsDevice, IntPtr hwndPtr)
        {
            if (!isCaptureStarted)
                return;

            GetMethod<RenderDocApi.RENDERDOC_EndFrameCapture>(RenderDocApiFunction.EndFrameCapture)(GetDevicePointer(graphicsDevice), hwndPtr);
            isCaptureStarted = false;
        }

        public void DiscardFrameCapture(GraphicsDevice graphicsDevice, IntPtr hwndPtr)
        {
            if (!isCaptureStarted)
                return;

            GetMethod<RenderDocApi.RENDERDOC_DiscardFrameCapture>(RenderDocApiFunction.DiscardFrameCapture)(GetDevicePointer(graphicsDevice), hwndPtr);
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

        private unsafe TDelegate GetMethod<TDelegate>(RenderDocApiFunction function)
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
        
    }
}
