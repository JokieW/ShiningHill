using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace SH.Native
{
    [SuppressUnmanagedCodeSecurity]
    public static class User32
    {
        private const string DLLNAME = "User32.dll";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool EnumWindowsProc(IntPtr hWnd, ref HandleData lParam);

        public const int HWND_BOTTOM = 1;
        public const int HWND_NOTOPMOST = -2;
        public const int HWND_TOP = 0;
        public const int HWND_TOPMOST = -1;

        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;

        public const int SWP_ASYNCWINDOWPOS = 0x4000;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_DRAWFRAME = 0x0020;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const int SWP_HIDEWINDOW = 0x0080;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_NOCOPYBITS = 0x0100;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOOWNERZORDER = 0x0200;
        public const int SWP_NOREDRAW = 0x0008;
        public const int SWP_NOREPOSITION = 0x0200;
        public const int SWP_NOSENDCHANGING = 0x0400;
        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_SHOWWINDOW = 0x0040;

        public const int GW_CHILD = 5;
        public const int GW_ENABLEDPOPUP = 6;
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;
        public const int GW_OWNER = 4;

        public const int GWL_STYLE = -16;

        public const uint WS_BORDER = 0x00800000;
        public const uint WS_CAPTION = 0x00C00000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_CHILDWINDOW = 0x40000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_GROUP = 0x00020000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_ICONIC = 0x20000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU);
        public const uint WS_SIZEBOX = 0x00040000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_TABSTOP = 0x00010000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_TILED = 0x00000000;
        public const uint WS_TILEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_VSCROLL = 0x00200000;

        [DllImport(DLLNAME)]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport(DLLNAME)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport(DLLNAME)]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport(DLLNAME)]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport(DLLNAME, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport(DLLNAME)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport(DLLNAME)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport(DLLNAME)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport(DLLNAME)]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport(DLLNAME)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref Rect lpRect);

        [DllImport(DLLNAME)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(DLLNAME)]
        public static extern bool GetClipCursor(ref Rect lpRect);

        [DllImport(DLLNAME)]
        public static extern bool ClipCursor(Rect lpRect);

        [DllImport(DLLNAME)]
        public static extern bool ClipCursor(IntPtr lpRect);

        [DllImport(DLLNAME)]
        public static extern bool GetClientRect(IntPtr hWnd, ref Rect lpRect);

        [DllImport(DLLNAME)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport(DLLNAME)]
        public static extern bool EnumWindows(IntPtr lpEnumFunc, ref HandleData lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public override string ToString()
            {
                return "U32Rect(" + left + ", " + top + ", " + right + ", " + bottom + ")";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HandleData
        {
            public int process_id;
            public IntPtr best_handle;
        }

        public static class util
        {
            public static IntPtr GetMainWindow(Process process)
            {
                if (process == null) return IntPtr.Zero;
                HandleData data = new HandleData() { process_id = process.Id, best_handle = IntPtr.Zero };
                EnumWindows(Marshal.GetFunctionPointerForDelegate((EnumWindowsProc)GetMainWindowCallback), ref data);
                return data.best_handle;
            }

            private static bool GetMainWindowCallback(IntPtr handle, ref HandleData lParam)
            {
                int process_id = 0;
                GetWindowThreadProcessId(handle, out process_id);
                if (
                    lParam.process_id != process_id ||
                    !(GetWindow(handle, GW_OWNER) == IntPtr.Zero && IsWindowVisible(handle))
                    )
                {
                    return true;
                }
                lParam.best_handle = handle;
                return false;
            }

            public static IntPtr GetYoungestWindow(Process process)
            {
                if (process == null) return IntPtr.Zero;
                HandleData data = new HandleData() { process_id = process.Id, best_handle = IntPtr.Zero };
                EnumWindows(Marshal.GetFunctionPointerForDelegate((EnumWindowsProc)GetYoungestWindowCallback), ref data);
                return data.best_handle;
            }

            private static bool GetYoungestWindowCallback(IntPtr handle, ref HandleData lParam)
            {
                int process_id = 0;
                GetWindowThreadProcessId(handle, out process_id);
                if (
                    lParam.process_id != process_id ||
                    (long)lParam.best_handle > (long)handle
                    )
                {
                    return true;
                }
                lParam.best_handle = handle;
                return false;
            }
        }

    }
}
