using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ShiningHill
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

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            char c;
            while ((c = reader.ReadChar()) != 0)
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static void SkipByte(this BinaryReader reader, byte? expected = null)
        {
            if (expected != null)
            {
                byte b = reader.ReadByte();
                if (b != expected.Value)
                {
                    Debug.LogWarning(String.Format("SKIP UNEXPECTED: byte at offset {0:X} was {1}, expected {2}. (Stream Length {3:X})", reader.BaseStream.Position - 1, b, expected.Value, reader.BaseStream.Length));
                }
            }
            else
            {
                reader.BaseStream.Position++;
            }
        }

        public static void SkipBytes(this BinaryReader reader, int count, byte? expected = null)
        {
            if (expected != null)
            {
                for (int i = 0; i != count; i++)
                {
                    byte b = reader.ReadByte();
                    if (b != expected.Value)
                    {
                        Debug.LogWarning(String.Format("SKIP UNEXPECTED: byte at offset {0:X} was {1}, expected {2}. (Stream Length {3:X})", reader.BaseStream.Position - 1, b, expected.Value, reader.BaseStream.Length));
                    }
                }
            }
            else
            {
                reader.BaseStream.Position += count;
            }
        }

        public static void SkipInt16(this BinaryReader reader, short? expected = null)
        {
            short s = reader.ReadInt16();
            if (expected != null && s != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int16 at offset {0:X} was {1}, expected {2}. (Stream Length {3:X})", reader.BaseStream.Position - 2, s, expected.Value, reader.BaseStream.Length));
            }
            
        }
        public static void SkipInt32(this BinaryReader reader, int? expected = null)
        {
            int i = reader.ReadInt32();
            if (expected != null && i != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int32 at offset {0:X} was {1}, expected {2}. (Stream Length {3:X})", reader.BaseStream.Position - 4, i, expected.Value, reader.BaseStream.Length));
            }
        }
        public static void SkipInt64(this BinaryReader reader, long? expected = null)
        {
            long l = reader.ReadInt64();
            if (expected != null && l != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int64 at offset {0:X} was {1}, expected {2}. (Stream Length {3:X})", reader.BaseStream.Position - 8, l, expected.Value, reader.BaseStream.Length));
            }
        }
	}
}