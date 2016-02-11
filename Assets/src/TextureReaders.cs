using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ShiningHill
{
	public static class TextureReaders 
	{
        public static Texture2D[] ReadTex32(BinaryReader reader)
        {
            reader.SkipBytes(12); //Skips -1 0 32
            int texGroupLength = reader.ReadInt32();
            reader.SkipBytes(4); //Skips 0
            int texCount = reader.ReadInt32();
            reader.SkipBytes(8); //Skips 0 0

            List<Texture2D> textures = new List<Texture2D>(texCount);

            for (int i = 0; i != texCount; i++)
            {
                reader.SkipBytes(8); //Skips -1 0
                short width = reader.ReadInt16();
                short height = reader.ReadInt16();
                reader.SkipByte(); //skips 0x20
                byte buffer = reader.ReadByte();
                reader.SkipBytes(2); //skips 0x0000
                int lengthOfTex = reader.ReadInt32();
                int nextDataRelativeOffset = reader.ReadInt32();
                reader.SkipBytes(24 + buffer); //skips -1 0 0 0 0 0 + buffer
                List<Color32> _pixels = new List<Color32>(lengthOfTex / 4);
                for (int j = 0; j != lengthOfTex; j += 4)
                {
                    _pixels.Add(reader.ReadColor32());
                }
                Texture2D text = new Texture2D(width, height, TextureFormat.RGBA32, false);
                text.SetPixels32(_pixels.ToArray());
                text.Apply();
                textures.Add(text);
                _pixels.Clear();
            }

            return textures.ToArray();
        }
	}
}