using System;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;

using SH.Native;

namespace SH.Runtime.Shared
{
    public static unsafe class Scribe
    {
        private static uint _pageSize;
        private static uint _pageMask;

        static Scribe()
        {
            /*Kernel32.SYSTEM_INFO si;
            Kernel32.GetSystemInfo(out si);
            _pageSize = si.PageSize;
            _pageMask = 0xFFFFFFFF - (si.PageSize - 1);*/
        }

        public static IntPtr OpenProcess(Process memProcess)
        {
            IntPtr handle = Kernel32.OpenProcess(Kernel32.PROCESS_VM_OPERATION | Kernel32.PROCESS_WM_READ | Kernel32.PROCESS_VM_WRITE, false, memProcess.Id);

            /*foreach (IntPtr intptr in _pages)
            {
                int oldsettings;
                Kernel32.VirtualProtectEx(handle, intptr, _pageSize, 0x40, out oldsettings);
            }*/

            return handle;
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/aa366786(v=vs.85).aspx
        private static HashSet<IntPtr> _pages = new HashSet<IntPtr>();
        public static IntPtr RegAddr(long address)
        {
            IntPtr addr = new IntPtr(address);
            _pages.Add(new IntPtr(address | _pageMask));
            return addr;
        }

        //Bytes
        public static byte ReadByte(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            byte buffer;
            Kernel32.ReadProcessMemory(handle, address, &buffer, 1, out bytesRead);
            return buffer;
        }

        public static void WriteByte(IntPtr handle, IntPtr address, byte data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 1, out bytesWrite))
            {
                Console.WriteLine("WriteByte error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        //bools
        public static bool ReadBool(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            bool data = false;
            Kernel32.ReadProcessMemory(handle, address, &data, 1, out bytesRead);
            return data;
        }

        public static void WriteBool(IntPtr handle, IntPtr address, bool data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 1, out bytesWrite))
            {
                Console.WriteLine("WriteBool error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        //shorts
        public static ushort ReadUInt16(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            ushort data = 0;
            Kernel32.ReadProcessMemory(handle, address, &data, 2, out bytesRead);
            return data;
        }

        public static void WriteUInt16(IntPtr handle, IntPtr address, ushort data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 2, out bytesWrite))
            {
                Console.WriteLine("WriteUInt16 error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        public static short ReadInt16(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            short data = 0;
            Kernel32.ReadProcessMemory(handle, address, &data, 2, out bytesRead);
            return data;
        }

        public static void WriteInt16(IntPtr handle, IntPtr address, short data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 2, out bytesWrite))
            {
                Console.WriteLine("WriteInt16 error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        //ints
        public static uint ReadUInt32(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            uint data = 0u;
            Kernel32.ReadProcessMemory(handle, address, &data, 4, out bytesRead);
            return data;
        }

        public static void WriteUInt32(IntPtr handle, IntPtr address, uint data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 4, out bytesWrite))
            {
                Console.WriteLine("WriteUInt32 error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        public static int ReadInt32(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            int data = 0;
            Kernel32.ReadProcessMemory(handle, address, &data, 4, out bytesRead);
            return data;
        }

        public static void WriteInt32(IntPtr handle, IntPtr address, int data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 4, out bytesWrite))
            {
                Console.WriteLine("WriteInt32 error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        //longs
        public static ulong ReadUInt64(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            ulong data = 0uL;
            Kernel32.ReadProcessMemory(handle, address, &data, 8, out bytesRead);
            return data;
        }

        public static void WriteUInt64(IntPtr handle, IntPtr address, ulong data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 8, out bytesWrite))
            {
                Console.WriteLine("WriteUInt64 error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        public static long ReadInt64(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            long data = 0L;
            Kernel32.ReadProcessMemory(handle, address, &data, 8, out bytesRead);
            return data;
        }

        public static void WriteInt64(IntPtr handle, IntPtr address, long data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 8, out bytesWrite))
            {
                Console.WriteLine("WriteInt64 error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        //floats
        public static float ReadSingle(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            float dat = 0.0f;
            Kernel32.ReadProcessMemory(handle, address, &dat, 4, out bytesRead);
            return dat;
        }

        public static void WriteSingle(IntPtr handle, IntPtr address, float data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 4, out bytesWrite))
            {
                Console.WriteLine("WriteSingle error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        //doubles
        public static double ReadDouble(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            double data = 0.0;
            Kernel32.ReadProcessMemory(handle, address, &data, 8, out bytesRead);
            return data;
        }

        public static void WriteDouble(IntPtr handle, IntPtr address, double data)
        {
            IntPtr bytesWrite;
            if (!Kernel32.WriteProcessMemory(handle, address, &data, 8, out bytesWrite))
            {
                Console.WriteLine("WriteDouble error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        //Vector3
        public static Vector3 ReadVector3(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            float* buffer = stackalloc float[3];
            Kernel32.ReadProcessMemory(handle, address, buffer, 12, out bytesRead);
            return new Vector3(buffer[0], buffer[1], buffer[2]);
        }

        public static void WriteVector3(IntPtr handle, IntPtr address, Vector3 data)
        {
            IntPtr bytesWrite;
            float* buffer = stackalloc float[3];
            buffer[0] = data.x;
            buffer[1] = data.y;
            buffer[2] = data.z;
            if (!Kernel32.WriteProcessMemory(handle, address, buffer, 12, out bytesWrite))
            {
                Console.WriteLine("WriteVector3 error: " + Kernel32.GetLastError().ToString("X"));
            }
        }

        //Quaternion
        public static Quaternion ReadQuaternion(IntPtr handle, IntPtr address)
        {
            IntPtr bytesRead;
            Quaternion data = new Quaternion();
            Kernel32.ReadProcessMemory(handle, address, &data, 16, out bytesRead);
            return data;
        }

        public static void WriteQuaternion(IntPtr handle, IntPtr address, Quaternion data)
        {
            IntPtr bytesWrite;

            if (!Kernel32.WriteProcessMemory(handle, address, &data, 16, out bytesWrite))
            {
                Console.WriteLine("WriteVector3 error: " + Kernel32.GetLastError().ToString("X"));
            }
        }
    }
}
