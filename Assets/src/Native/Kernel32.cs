#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SH.Native
{
    [SuppressUnmanagedCodeSecurity]
    public static class Kernel32
    {
        private const string DLLNAME = "kernel32.dll";

        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_WM_READ = 0x0010;
        public const int PROCESS_VM_WRITE = 0x0020;

        [DllImport(DLLNAME)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport(DLLNAME)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport(DLLNAME)]
        public static extern int GetLastError();

        [DllImport(DLLNAME)]
        public static unsafe extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, void* lpBuffer, long dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport(DLLNAME, SetLastError = true)]
        public static unsafe extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, void* lpBuffer, long dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport(DLLNAME, SetLastError = false)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, long dSize, int flNewProtect, out int lpflOldProtect);

        [DllImport(DLLNAME, SetLastError = false)]
        public static extern void GetSystemInfo(out SYSTEM_INFO Info);

        [StructLayout(LayoutKind.Explicit)]
        public struct SYSTEM_INFO_UNION
        {
            [FieldOffset(0)]
            public UInt32 OemId;
            [FieldOffset(0)]
            public UInt16 ProcessorArchitecture;
            [FieldOffset(2)]
            public UInt16 Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_INFO
        {
            public SYSTEM_INFO_UNION CpuInfo;
            public UInt32 PageSize;
            public UInt32 MinimumApplicationAddress;
            public UInt32 MaximumApplicationAddress;
            public UInt32 ActiveProcessorMask;
            public UInt32 NumberOfProcessors;
            public UInt32 ProcessorType;
            public UInt32 AllocationGranularity;
            public UInt16 ProcessorLevel;
            public UInt16 ProcessorRevision;
        }
    }
}
#endif