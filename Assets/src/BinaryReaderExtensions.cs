using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SilentParty
{
	public static class BinaryReaderExtensions 
	{
        public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader)
        {
            Matrix4x4 mat = new Matrix4x4();
            for (int i = 0; i != 16; i++)
            {
                mat[i] = reader.ReadSingle();
            }
            return mat;
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Color32 ReadColor32(this BinaryReader reader)
        {
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            byte a = reader.ReadByte();
            return new Color32(b, g, r, a);
        }

        public static void SkipByte(this BinaryReader reader)
        {
            reader.BaseStream.Position++;
        }

        public static void SkipBytes(this BinaryReader reader, int count)
        {
            reader.BaseStream.Position += count;
        }

        public static void SkipInt16(this BinaryReader reader)
        {
            reader.ReadInt16();
        }
        public static void SkipInt32(this BinaryReader reader)
        {
            reader.ReadInt32();
        }
        public static void SkipInt64(this BinaryReader reader)
        {
            reader.ReadInt64();
        }
	}
}