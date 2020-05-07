using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

using SH.Core;
using SH.DataFormat.Shared;

namespace SH.DataFormat.SH3
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct TextureGroup
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
            public Color32[] pixels;

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

        public static void ReadTextureGroup(BinaryReader reader, out TextureGroup group)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadTextureGroup");
            UnityEngine.Profiling.Profiler.BeginSample("new TextureGroup");
            group = new TextureGroup();
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.BeginSample("reader.ReadStruct<TextureGroup.Header>");
            group.header = reader.ReadStruct<TextureGroup.Header>();
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.BeginSample("new TextureGroup.Texture");
            group.textures = new TextureGroup.Texture[group.header.textureCount];
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("fori");
            for (int i = 0; i < group.header.textureCount; i++)
            {
                UnityEngine.Profiling.Profiler.BeginSample("new TextureGroup.Texture");
                TextureGroup.Texture tex = new TextureGroup.Texture();
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.BeginSample("ReadStruct<TextureGroup.Texture.Header>");
                tex.header = reader.ReadStruct<TextureGroup.Texture.Header>();
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.BeginSample("reader.SkipBytes");
                reader.SkipBytes(tex.header.bufferSizeAfterHeader);
                UnityEngine.Profiling.Profiler.EndSample();

                int bits = tex.header.bitsPerPixel;
                if (bits == 32 || bits == 24)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("forj3224");

                    tex.pixels = new Color32[tex.header.pixelsLength / 0x04];

                    UnityEngine.Profiling.Profiler.BeginSample("ReadStruct<MapFile.ColorBGRA>");
                    UnityEngine.Profiling.Profiler.BeginSample("new");
                    ColorBGRA[] buffer = new ColorBGRA[tex.pixels.Length];
                    UnityEngine.Profiling.Profiler.EndSample();
                    UnityEngine.Profiling.Profiler.BeginSample("read");
                    reader.ReadStruct<ColorBGRA>(buffer);
                    UnityEngine.Profiling.Profiler.EndSample();
                    UnityEngine.Profiling.Profiler.EndSample();

                    UnityEngine.Profiling.Profiler.BeginSample("<MapFile.ColorBGRA> convert");
                    for (int j = 0; j != tex.pixels.Length; j++)
                    {
                        ColorBGRA bgra = buffer[j];
                        tex.pixels[j] = new Color32(bgra.red, bgra.green, bgra.blue, bgra.alpha);
                    }
                    UnityEngine.Profiling.Profiler.EndSample();

                    UnityEngine.Profiling.Profiler.EndSample();
                }
                else if (bits == 16)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("forj16");
                    tex.pixels = new Color32[tex.header.pixelsLength / 0x02];
                    for (int j = 0; j != tex.pixels.Length; j++)
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("ReadStruct<MapFile.ColorRGBA5551>");
                        tex.pixels[j] = reader.ReadStruct<ColorRGBA5551>();
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                    UnityEngine.Profiling.Profiler.EndSample();
                }

                group.textures[i] = tex;
            }
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}
