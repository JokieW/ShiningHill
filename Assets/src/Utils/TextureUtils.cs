using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{
	public static class TextureUtils 
	{
        public static Texture2D[] ReadDDS(string baseName, BinaryReader reader)
        {
            reader.SkipInt32(2);
            int texturesSize = reader.ReadInt32();
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            reader.SkipInt32(0x19990901); //magic
            reader.SkipInt32(0);
            reader.SkipInt32(0);
            reader.SkipInt32(1);

            List<Texture2D> textures = new List<Texture2D>();

            int i = 0;
            while (reader.BaseStream.Position < texturesSize)
            {
                short textureID = reader.ReadInt16();
                reader.SkipInt16(0);
                short width = reader.ReadInt16();
                short height = reader.ReadInt16();
                reader.SkipInt16(512);
                reader.SkipInt16(512);
                int subgroupsCount = reader.ReadInt32(); //1 more?

                reader.SkipInt16();
                reader.SkipInt16();
                reader.SkipBytes(12); //Skips 0 0 0

                int texLength = 0;
                for (int j = 0; j != subgroupsCount; j++)
                {
                    //Subgroup thingie
                    /*short subgroupID = */
                    reader.SkipInt16();
                    reader.SkipInt16();
                    reader.SkipInt16(0);
                    reader.SkipInt16(0);
                    reader.SkipInt16(512);
                    reader.SkipInt16(512);
                    reader.SkipInt16(256);
                    reader.SkipInt16(0);

                    texLength = reader.ReadInt32();
                    /*int texAndHeaderLength = */
                    reader.SkipInt32();
                    reader.SkipInt32(0);
                    reader.SkipUInt32(0x99000000);
                }

                Texture2D text = new Texture2D(width, height, TextureFormat.DXT1, false);
                text.LoadRawTextureData(reader.ReadBytes(texLength));
                text.Apply();

                text.alphaIsTransparency = true;
                text.name = baseName + textureID.ToString("0000");

                textures.Add(text);
                i++;
            }
            reader.SkipBytes(0x10);

            return textures.ToArray();
        }

        public static Texture2D[] ReadTex32(string groupName, in MaterialRolodex.TextureGroup textureGroup)
        {
            List<Texture2D> textures = new List<Texture2D>(textureGroup.textures.Length);

            for(int i = 0; i < textureGroup.textures.Length; i++)
            {
                ref readonly MaterialRolodex.TextureGroup.Texture texstruct = ref textureGroup.textures[i];

                TextureFormat format = (texstruct.header.bitsPerPixel == 24 ? TextureFormat.RGB24 : TextureFormat.RGBA32);
                Texture2D tex = new Texture2D(texstruct.header.textureWidth, texstruct.header.textureHeight, format, false);
                tex.SetPixels32(texstruct.pixels);
                tex.Apply();

                tex.alphaIsTransparency = true;
                tex.name = groupName + i;

                textures.Add(tex);
            }

            return textures.ToArray();
        }

        public static Texture2D[] ReadTex32(string baseName, BinaryReader reader)
        {
            reader.SkipBytes(12); //Skips -1 0 32
            /*int texGroupLength = */reader.ReadInt32();
            reader.SkipBytes(4); //Skips 0
            int texCount = reader.ReadInt32();
            reader.SkipBytes(8); //Skips 0 0

            List<Texture2D> textures = new List<Texture2D>(texCount);

            for (int i = 0; i != texCount; i++)
            {
                reader.SkipBytes(8); //Skips -1 0
                short width = reader.ReadInt16();
                short height = reader.ReadInt16();
                byte bits = reader.ReadByte();
                byte buffer = reader.ReadByte();
                reader.SkipBytes(2); //skips 0x0000
                int lengthOfTex = reader.ReadInt32();
                /*int nextDataRelativeOffset = */reader.ReadInt32();
                reader.SkipBytes(24 + buffer); //skips -1 0 0 0 0 0 + buffer

                Color32[] _pixels = null;
                TextureFormat format = TextureFormat.RGBA32;
                if (bits == 32 || bits == 24)
                {
                    format = (bits == 24 ? TextureFormat.RGB24 : TextureFormat.RGBA32);
                    _pixels = new Color32[lengthOfTex / 4];
                    for (int j = 0; j != lengthOfTex / 4; j++)
                    {
                        _pixels[j] = reader.ReadBGRA();
                    }
                }
                else if (bits == 16)
                {
                    format = TextureFormat.RGBA32;
                    _pixels = new Color32[lengthOfTex / 2];
                    for (int j = 0; j != lengthOfTex / 2; j ++)
                    {
                        _pixels[j] = reader.ReadRGBA5551();
                    }
                }

                Texture2D text = new Texture2D(width, height, format, false);
                text.SetPixels32(_pixels);
                text.Apply();

                text.alphaIsTransparency = true;
                text.name = baseName + i;

                textures.Add(text);

            }

            return textures.ToArray();
        }
	}
}