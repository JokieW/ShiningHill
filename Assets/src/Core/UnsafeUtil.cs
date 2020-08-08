
using System.Runtime.InteropServices;

namespace SH.Core
{
    public static class UnsafeUtil
    {
        public static unsafe void MemCopy<T>(T[] source, long sourceIndex, T[] destination, long destinationIndex, long count) where T: unmanaged
        {
            int size = Marshal.SizeOf<T>();
            long byteCount = count * size;
            fixed (void* sourcePtr = source)
            fixed (void* destinationPtr = destination)
            {
                byte* byteSourcePtr = (byte*)sourcePtr;
                byte* bytedestinationPtr = (byte*)destinationPtr;
                byteSourcePtr += sourceIndex * size;
                bytedestinationPtr += destinationIndex * size;
                MemCopy(byteSourcePtr, bytedestinationPtr, byteCount);
            }
        }
        
        public static unsafe void MemCopy(void* source, void* destination, long count)
        {
            long lenghtByte = count % 8;
            byte* sourceByte = (byte*)source;
            byte* destinationByte = (byte*)destination;
            for (long i = 0; i < lenghtByte; i++)
            {
                *destinationByte++ = *sourceByte++;
            }

            long lengthLong = count - lenghtByte;
            ulong* sourceLong = (ulong*)sourceByte;
            ulong* destinationLong = (ulong*)destinationByte;
            for (int i = 0; i < lengthLong; i += 8)
            {
                *destinationLong++ = *sourceLong++;
            }
        }
    }
}
