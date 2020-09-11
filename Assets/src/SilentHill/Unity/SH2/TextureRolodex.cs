using System.Collections.Generic;
using System.IO;
using System.Linq;

using System;

using UnityEngine;

using SH.GameData.SH2;
using SH.GameData.Shared;
using UnityEditor;

namespace SH.Unity.SH2
{
    public class TextureRolodex : ScriptableObject
    {
        [Serializable]
        public struct TextureStruct
        {
            public FileTex.DXTTexture.Header texinfoheader;
            public FileTex.DXTTexture.Subgroup[] texinfosubgroups;
            public FileTex.DXTTexture.PixelHeader texinfopixelHeader;

            [Hex] public int textureId;
            public Texture2D texture;
        }

        [SerializeField]
        public List<TextureStruct> textures = new List<TextureStruct>();

        public void AddTextures(string baseName, FileTex texFile)
        {
            (FileTex.DXTTexture, Texture2D)[] loadedTextures = ReadTexDXT1(baseName, texFile);
            for(int i = 0; i < loadedTextures.Length; i++)
            {
                (FileTex.DXTTexture, Texture2D) t = loadedTextures[i];
                TextureStruct newTexStruct = new TextureStruct()
                {
                    texinfoheader = t.Item1.header,
                    texinfosubgroups = t.Item1.subgroups,
                    texinfopixelHeader = t.Item1.pixelHeader,
                    textureId = t.Item1.header.textureId,
                    texture = t.Item2
                };

                bool replacedOldTex = false;
                for (int j = 0; j < textures.Count; j++)
                {
                    TextureStruct oldTexStruct = textures[i];
                    if (oldTexStruct.textureId == newTexStruct.textureId)
                    {
                        AssetDatabase.RemoveObjectFromAsset(oldTexStruct.texture);
                        textures[i] = newTexStruct;
                        replacedOldTex = true;
                        break;
                    }
                }

                if (!replacedOldTex)
                {
                    textures.Add(newTexStruct);
                }

                if (newTexStruct.texture != null)
                {
                    AssetDatabase.AddObjectToAsset(newTexStruct.texture, this);
                }
            }
        }

        protected bool GetTextureStruct(int id, out TextureStruct texStruct)
        {
            if (textures != null)
            {
                for (int i = 0; i < textures.Count; i++)
                {
                    TextureStruct tex = textures[i];
                    if (tex.textureId == id)
                    {
                        texStruct = tex;
                        return true;
                    }
                }
            }

            texStruct = default;
            return false;
        }

        public Texture2D GetTexture(int id)
        {
            Texture2D tex = null;
            if (GetTextureStruct(id, out TextureStruct texStruct))
            {
                tex = texStruct.texture;
            }
            return tex;
        }

        public unsafe static (FileTex.DXTTexture, Texture2D)[] ReadTexDXT1(string baseName, FileTex texFile)
        {
            (FileTex.DXTTexture, Texture2D)[] textures = new (FileTex.DXTTexture, Texture2D)[texFile == null ? 0 : texFile.textures.Length];

            for(int i = 0; i < textures.Length; i++)
            {
                FileTex.DXTTexture dxt = texFile.textures[i];
                bool isTwo = (dxt.subgroups[0].field_1C & 0x02) != 0;
                bool isFour = (dxt.subgroups[0].field_1C & 0x04) != 0;
                Texture2D newTexture = new Texture2D(dxt.header.width, dxt.header.height, isTwo || isFour ? TextureFormat.RGBA32 : TextureFormat.RGBA32, false);
                newTexture.wrapMode = TextureWrapMode.Clamp;

                if (isTwo || isFour)
                {
                    Color[] pixels = new Color[dxt.header.width * dxt.header.height];
                    ColorBC2.BufferToColorRGBA8888(pixels, dxt.pixels, dxt.header.width, dxt.header.height);
                    newTexture.SetPixels(pixels);
                }
                else
                {
                    Color[] pixels = new Color[dxt.header.width * dxt.header.height];
                    ColorBC1.BufferToColorRGBA8888(pixels, dxt.pixels, dxt.header.width, dxt.header.height);
                    newTexture.SetPixels(pixels);
                    //newTexture.LoadRawTextureData(dxt.pixels);
                }

                newTexture.Apply();

                newTexture.alphaIsTransparency = true;
                newTexture.name = baseName + dxt.header.textureId.ToString("X4");

                textures[i] = (dxt, newTexture);
            }
            
            return textures;
        }
    }
}
