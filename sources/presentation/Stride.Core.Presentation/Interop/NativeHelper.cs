// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
namespace Stride.Core.Presentation.Interop
{
    public static partial class NativeHelper
    {
        public static object SW_RESTORE;
        public static int WS_MINIMIZE;
        public static object GWL_STYLE;

        public static int GetWindowLong(IntPtr processMainWindowHandle, object gwlStyle)
        {
            return 0;
        }

        public static void ShowWindow(IntPtr processMainWindowHandle, object swRestore)
        {
            throw new NotImplementedException();
        }

        public static void SetForegroundWindow(IntPtr processMainWindowHandle)
        {
            throw new NotImplementedException();
        }
    }
}
