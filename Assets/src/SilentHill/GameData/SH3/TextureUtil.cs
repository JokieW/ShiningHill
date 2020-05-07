using System.Collections.Generic;

using UnityEngine;

namespace SH.GameData.SH3
{
	public static class TextureUtil
	{
        public static Texture2D[] ReadTex32(string groupName, in TextureGroup textureGroup)
        {
            List<Texture2D> textures = new List<Texture2D>(textureGroup.textures.Length);

            for(int i = 0; i < textureGroup.textures.Length; i++)
            {
                ref readonly TextureGroup.Texture texstruct = ref textureGroup.textures[i];

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
	}
}