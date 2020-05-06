using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.IO;

namespace ShiningHill
{
    [CreateAssetMenu]
	public abstract class MaterialRolodex : ScriptableObject 
	{
        protected List<TexMatsPair> texMatPairs = new List<TexMatsPair>();

        public MaterialRolodex()
        {
        }

        protected abstract Material CreateDiffuse(Texture tex);
        protected abstract Material CreateTransparent(Texture tex);
        protected abstract Material CreateCutout(Texture tex);
        protected abstract Material CreateSelfIllum(Texture tex);

        public void AddTextures(Texture[] texs)
        {
            for(int i = 0; i < texs.Length; i++)
            {
                Texture tex = texs[i];
                texMatPairs.Add(new TexMatsPair(tex));
                AssetDatabase.AddObjectToAsset(tex, AssetDatabase.GetAssetPath(this));
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
                    MapFile.ColorBGRA[] buffer = new MapFile.ColorBGRA[tex.pixels.Length];
                    UnityEngine.Profiling.Profiler.EndSample();
                    UnityEngine.Profiling.Profiler.BeginSample("read");
                    reader.ReadStruct<MapFile.ColorBGRA>(buffer);
                    UnityEngine.Profiling.Profiler.EndSample();
                    UnityEngine.Profiling.Profiler.EndSample();

                    UnityEngine.Profiling.Profiler.BeginSample("<MapFile.ColorBGRA> convert");
                    for (int j = 0; j != tex.pixels.Length; j++)
                    {
                        MapFile.ColorBGRA bgra = buffer[j];
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
                        tex.pixels[j] = reader.ReadStruct<MapFile.ColorRGBA5551>();
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                    UnityEngine.Profiling.Profiler.EndSample();
                }

                group.textures[i] = tex;
            }
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.EndSample();
        }

        [Serializable]
        protected class TexMatsPair
        {
            [SerializeField]
            public Texture texture;
            [SerializeField]
            public Material[] materials;

            public TexMatsPair(Texture tex)
            {
                texture = tex;
                materials = new Material[(int)MapFile.MaterialType.__Count];
            }

            public Material GetOrCreate(MapFile.MaterialType matType, MaterialRolodex rolodex)
            {
                Material mat = materials[(int)matType];
                if (mat == null)
                {
                    if (matType == MapFile.MaterialType.Diffuse) mat = rolodex.CreateDiffuse(texture);
                    else if (matType == MapFile.MaterialType.Cutout) mat = rolodex.CreateCutout(texture);
                    else if (matType == MapFile.MaterialType.Transparent) mat = rolodex.CreateTransparent(texture);
                    else if (matType == MapFile.MaterialType.SelfIllum) mat = rolodex.CreateSelfIllum(texture);
                    else throw new InvalidOperationException();

                    AssetDatabase.AddObjectToAsset(mat, rolodex);
                    materials[(int)matType] = mat;
                }
                return mat;
            }
        }


        [Serializable]
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
        }
    }
}