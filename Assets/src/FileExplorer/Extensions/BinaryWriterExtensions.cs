using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Runtime.InteropServices;

namespace ShiningHill
{
	public static class BinaryWriterExtensions
    {

        public unsafe static void WriteStruct<T>(this BinaryWriter writer, T value) where T: unmanaged
        {
            int structlength = Marshal.SizeOf<T>();
            byte* structbytes = (byte*)&value;
            for(int i = 0; i < structlength; i++)
            {
                writer.Write(*(structbytes + i));
            }
        }

        public static void WriteNullTerminatedString(this BinaryWriter writer, string value)
        {
            for(int i = 0; i != value.Length; i++)
            {
                writer.Write((byte)value[i]);
            }
            writer.Write((byte)0x00);
        }
    }
}