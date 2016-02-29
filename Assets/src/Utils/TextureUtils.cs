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
                if (bits == 32)
                {
                    _pixels = new Color32[lengthOfTex / 4];
                    for (int j = 0; j != lengthOfTex / 4; j++)
                    {
                        _pixels[j] = reader.ReadBGRA();
                    }
                }
                else if (bits == 16)
                {
                    _pixels = new Color32[lengthOfTex / 2];
                    for (int j = 0; j != lengthOfTex / 2; j ++)
                    {
                        _pixels[j] = reader.ReadRGBA5551();
                    }
                }

                Texture2D text = new Texture2D(width, height, TextureFormat.RGBA32, false);
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