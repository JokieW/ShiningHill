using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SH.Core
{
    public static class BinaryWriterExtension
    {
        [ThreadStatic]
        private static byte[] _buffer2048 = new byte[2048];

        public unsafe static long Write(this BinaryWriter writer, byte* value, long length)
        {
            byte[] writeBuffer = _buffer2048;
            int writeBufferIndex = 0;
            long lengthLeft = length;
            long written = 0L;

            for (long i = 0; i < lengthLeft % 8; i++, writeBufferIndex++)
            {
                writeBuffer[i] = value[written++];
            }
            lengthLeft -= lengthLeft % 8;

            if (lengthLeft > 0)
            {
                long* longvalue = (long*)(value + written);
                fixed (byte* writeBufferPtr = writeBuffer)
                {
                    for (long i = 0; i < lengthLeft / 8; i++, writeBufferIndex += 8)
                    {
                        if (writeBufferIndex + 8 > writeBuffer.Length)
                        {
                            writer.Write(writeBuffer, 0, writeBufferIndex);
                            writeBufferIndex = 0;
                        }
                        long* longWriteBufferPtr = (long*)(writeBufferPtr + writeBufferIndex);
                        *longWriteBufferPtr = *longvalue;
                        longvalue++;
                        written += 8;
                    }
                    if (writeBufferIndex != 0)
                    {
                        writer.Write(writeBuffer, 0, writeBufferIndex);
                    }
                }
            }

            return written;
        }

        public unsafe static int WriteStruct<T>(this BinaryWriter writer, T value) where T : unmanaged
        {
            int structlength = Marshal.SizeOf<T>();
            byte* structbytes = (byte*)&value;
            return (int)writer.Write(structbytes, structlength);
        }

        public unsafe static int WriteStruct<T>(this BinaryWriter writer, in T value) where T : unmanaged
        {
            int structlength = Marshal.SizeOf<T>();
            fixed (T* pValue = &value)
            {
                byte* structbytes = (byte*)pValue;
                return (int)writer.Write(structbytes, structlength);
            }
        }

        public static int WriteNullTerminatedString(this BinaryWriter writer, string value)
        {
            for (int i = 0; i != value.Length; i++)
            {
                writer.Write((byte)value[i]);
            }
            writer.Write((byte)0x00);
            return value.Length + 1;
        }
    }
}
