using System;
using System.Runtime.InteropServices;

public static class Kernel32
{
    public const int PROCESS_VM_OPERATION = 0x0008;
    public const int PROCESS_WM_READ = 0x0010;
    public const int PROCESS_VM_WRITE = 0x0020;


    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern int GetLastError();

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = false)]
    public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dSize, int flNewProtect, out int lpflOldProtect);

    [DllImport("kernel32.dll", SetLastError = false)]
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
