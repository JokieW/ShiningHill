using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ShiningHill
{
	public static class BinaryReaderExtensions
    {
        #region Matrices
        /// <summary>
        /// Reads 16 singles and returns a Matrix4x4
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader)
        {
            Matrix4x4 mat = new Matrix4x4();
            for (int i = 0; i != 16; i++)
            {
                mat[i] = reader.ReadSingle();
            }
            return mat;
        }
        #endregion

        #region Vectors
        /// <summary>
        /// Reads 2 singles and returns a Vector2
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads 3 singles and returns a Vector3
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        /// <summary>
        /// Reads 3 shorts and returns a Vector3
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Vector3 ReadShortVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        }

        /// <summary>
        /// Reads 4 singles and returns a Vector4
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        #endregion

        #region Quaternions
        /// <summary>
        /// Reads 4 singles and returns a Quaternion
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        #endregion

        #region Colors
        /// <summary>
        /// Reads 4 singles expecting R G B A
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Color ReadColor(this BinaryReader reader)
        {
            Vector4 color = reader.ReadVector4();
            return new Color(color.x, color.y, color.z, color.w);
        }

        /// <summary>
        /// Reads 4 bytes expecting R G B A
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Color32 ReadRGBA(this BinaryReader reader)
        {
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            byte a = reader.ReadByte();
            return new Color32(r, b, g, a);
        }

        /// <summary>
        /// Reads 4 bytes expecting B G R A
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Color32 ReadBGRA(this BinaryReader reader)
        {
            byte b = reader.ReadByte();
            byte g = reader.ReadByte();
            byte r = reader.ReadByte();
            byte a = reader.ReadByte();
            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Reads 2 bytes expected to be RB GA
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Color32 ReadRGBA5551(this BinaryReader reader)
        {
            //Thanks de_lof
            byte one = reader.ReadByte();
            byte two = reader.ReadByte();
            int r = (two & 0x7c) << 1;
            int g = ((two & 0x03) << 6) | ((one & 0xe0) >> 2);
            int b = (one & 0x1f) << 3;
            int a = (two & 0x80) != 0 ? 255 : 0;
            r |= r >> 5;
            g |= g >> 5;
            b |= b >> 5;
            return new Color32((byte)r, (byte)g, (byte)b, (byte)a );
        }
        #endregion

        #region Strings
        /// <summary>
        /// Reads bytes until a \0 is reached, returns the string 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Reads a whole buffer of bytes, builds one null terminated string contained within
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadStringBuffer(this BinaryReader reader, int bufferSize)
        {
            char[] buffer = reader.ReadChars(bufferSize);
            int length = 0;

            for (int i = 0; i != bufferSize; i++)
            {
                if (buffer[i] != 0) length++; else break;
            }

            return new string(buffer, 0, length);
        }
        #endregion

        #region Peeks
        /// <summary>
        /// Reads a byte without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static byte PeekByte(this BinaryReader reader)
        {
            byte val = reader.PeekByte();
            reader.BaseStream.Position --;
            return val;
        }

        /// <summary>
        /// Reads a short without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static short PeekInt16(this BinaryReader reader)
        {
            short val = reader.ReadInt16();
            reader.BaseStream.Position -= 2;
            return val;
        }

        /// <summary>
        /// Reads an int without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int PeekInt32(this BinaryReader reader)
        {
            int val = reader.ReadInt32();
            reader.BaseStream.Position -= 4;
            return val;
        }

        /// <summary>
        /// Reads a long without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static long PeekInt64(this BinaryReader reader)
        {
            long val = reader.ReadInt64();
            reader.BaseStream.Position -= 8;
            return val;
        }

        /// <summary>
        /// Reads a float without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static float PeekSingle(this BinaryReader reader)
        {
            float val = reader.ReadSingle();
            reader.BaseStream.Position -= 4;
            return val;
        }

        /// <summary>
        /// Reads a double without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static double PeekDouble(this BinaryReader reader)
        {
            double val = reader.ReadDouble();
            reader.BaseStream.Position -= 8;
            return val;
        }
        #endregion

        #region Skips
        /// <summary>
        /// Skips a byte, will log a warning if expected is given and the byte doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipByte(this BinaryReader reader, byte? expected = null)
        {
            if (expected != null)
            {
                byte b = reader.ReadByte();
                if (b != expected.Value)
                {
                    Debug.LogWarning(String.Format("SKIP UNEXPECTED: byte at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 1, b, expected.Value, reader.BaseStream.Length));
                }
            }
            else
            {
                reader.BaseStream.Position++;
            }
        }

        /// <summary>
        /// Skips bytes, will log a warning if expected is given and one of the bytes doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipBytes(this BinaryReader reader, int count, byte? expected = null)
        {
            if (expected != null)
            {
                for (int i = 0; i != count; i++)
                {
                    byte b = reader.ReadByte();
                    if (b != expected.Value)
                    {
                        Debug.LogWarning(String.Format("SKIP UNEXPECTED: byte at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 1, b, expected.Value, reader.BaseStream.Length));
                    }
                }
            }
            else
            {
                reader.BaseStream.Position += count;
            }
        }

        /// <summary>
        /// Skips a short, will log a warning if excpected is given and the short doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt16(this BinaryReader reader, short? expected = null)
        {
            short s = reader.ReadInt16();
            if (expected != null && s != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int16 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 2, s, expected.Value, reader.BaseStream.Length));
            }
            
        }

        /// <summary>
        /// Skips an unsigned short, will log a warning if excpected is given and the short doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt16(this BinaryReader reader, ushort? expected = null)
        {
            ushort s = reader.ReadUInt16();
            if (expected != null && s != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int16 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 2, s, expected.Value, reader.BaseStream.Length));
            }

        }

        /// <summary>
        /// Skips an int, will log a warning if excpected is given and the int doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt32(this BinaryReader reader, int? expected = null)
        {
            int i = reader.ReadInt32();
            if (expected != null && i != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int32 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 4, i, expected.Value, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips an unsigned int, will log a warning if excpected is given and the int doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt32(this BinaryReader reader, uint? expected = null)
        {
            uint i = reader.ReadUInt32();
            if (expected != null && i != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int32 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 4, i, expected.Value, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips a long, will log a warning if excpected is given and the long doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt64(this BinaryReader reader, long? expected = null)
        {
            long l = reader.ReadInt64();
            if (expected != null && l != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int64 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 8, l, expected.Value, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips an unsigned long, will log a warning if excpected is given and the long doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt64(this BinaryReader reader, ulong? expected = null)
        {
            ulong l = reader.ReadUInt64();
            if (expected != null && l != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: int64 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 8, l, expected.Value, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips a float, will log a warning if excpected is given and the float doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipSingle(this BinaryReader reader, float? expected = null)
        {
            float f = reader.ReadSingle();
            if (expected != null && f != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: single at offset {0:X} was {1} ({1}), expected {2} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 4, f, expected.Value, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips a double, will log a warning if excpected is given and the float doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipDouble(this BinaryReader reader, double? expected = null)
        {
            double d = reader.ReadDouble();
            if (expected != null && d != expected.Value)
            {
                Debug.LogWarning(String.Format("SKIP UNEXPECTED: single at offset {0:X} was {1} ({1}), expected {2} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 4, d, expected.Value, reader.BaseStream.Length));
            }
        }
        #endregion
    }
}