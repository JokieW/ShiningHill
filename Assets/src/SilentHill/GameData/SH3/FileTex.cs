using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

using SH.Core;
using SH.GameData.Shared;

namespace SH.GameData.SH3
{
    [Serializable]
    public class TextureGroup
    {
        public Header header;
        public Texture[] textures;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            public int field_00;
            public int field_04;
            public int headerSize;
            public int totalLength;

            public int field_10;
            public int textureCount;
            public int field_18;
            public int field_1C;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Texture
        {
            public Header header;
            public byte[] pixels;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                public int field_00; // -1 0xFFFFFFFF
                public int field_04;
                public short textureWidth;
                public short textureHeight;
                public byte bitsPerPixel;
                public byte bufferSizeAfterHeader;
                public short field_0E;

                public int pixelsLength;
                public int totalLength;
                public int field_18;
                public int field_1C; //full of nines, 0x99990909 maybe date

                public int field_20; //1
                public int field_24;
                public int field_28;
                public int field_2C;
            }
        }

        public static TextureGroup ReadTextureGroup(BinaryReader reader)
        {
            TextureGroup group = new TextureGroup();
            UnityEngine.Profiling.Profiler.EndSample();
            group.header = reader.ReadStruct<TextureGroup.Header>();
            group.textures = new TextureGroup.Texture[group.header.textureCount];

            for (int i = 0; i < group.header.textureCount; i++)
            {
                TextureGroup.Texture tex = new TextureGroup.Texture();
                tex.header = reader.ReadStruct<TextureGroup.Texture.Header>();
                reader.SkipBytes(tex.header.bufferSizeAfterHeader);

                int bits = tex.header.bitsPerPixel;
                if (bits == 32 || bits == 24)
                {
                    tex.pixels = new byte[tex.header.pixelsLength];
                    reader.Read(tex.pixels, 0, tex.header.pixelsLength);

                    int increment = bits == 24 ? 3 : 4;
                    for (int j = 0, lenj = tex.pixels.Length; j < lenj; j += increment)
                    {
                        byte b = tex.pixels[j + 2];
                        tex.pixels[j + 2] = tex.pixels[j];
                        tex.pixels[j] = b;
                    }
                }
                else if (bits == 16)
                {
                    tex.pixels = new byte[tex.header.pixelsLength * 2];
                    for (int j = 0, k = 0, lenj = tex.header.pixelsLength / 2; j != lenj; j++, k += 4)
                    {
                        ColorRGBA5551 c = reader.ReadStruct<ColorRGBA5551>();
                        c.Unpack(out tex.pixels[k], out tex.pixels[k + 1], out tex.pixels[k + 2], out tex.pixels[k + 3]);
                    }
                }

                group.textures[i] = tex;
            }
            return group;
        }
    }
}
