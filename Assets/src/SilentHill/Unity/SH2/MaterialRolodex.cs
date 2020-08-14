using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using Nvidia.TextureTools;

using SH.Core;
using SH.Unity.Shared;
using SH.GameData.SH2;
using System;
using UnityEditor;
using System.Runtime.InteropServices;
using SH.GameData.Shared;

namespace SH.Unity.SH2
{
    public class MaterialRolodex : ScriptableObject
    {
        [SerializeField]
        protected List<TexMatsPair> texMatPairs = new List<TexMatsPair>();

        [Serializable]
        protected class TexMatsPair
        {
            [SerializeField]
            public Texture texture;
            [SerializeField]
            [Hex] public int textureFlags;
            [SerializeField]
            public Material[] materials;

            public TexMatsPair(Texture tex, int textureFlags)
            {
                this.texture = tex;
                this.textureFlags = textureFlags;
                this.materials = new Material[(int)MaterialType.__Count];
            }

            public Material GetOrCreate(MaterialType matType, MaterialRolodex rolodex)
            {
                Material mat = materials[(int)matType];
                if (mat == null)
                {
                    if (matType == MaterialType.Diffuse) mat = rolodex.CreateDiffuse(texture);
                    else if (matType == MaterialType.Cutout) mat = rolodex.CreateCutout(texture);
                    else if (matType == MaterialType.Transparent) mat = rolodex.CreateTransparent(texture);
                    else if (matType == MaterialType.SelfIllum) mat = rolodex.CreateSelfIllum(texture);
                    else throw new InvalidOperationException();

                    AssetDatabase.AddObjectToAsset(mat, rolodex);
                    materials[(int)matType] = mat;
                }
                return mat;
            }
        }

        public enum MaterialType
        {
            Diffuse = 0,
            Cutout = 1,
            Transparent = 2,
            SelfIllum = 3,

            __Count = 4
        }

        public Material GetOrCreateMaterial(int id)
        {
            TexMatsPair pair = texMatPairs.Where(x => x.texture.name.Contains(id.ToString("X4"))).FirstOrDefault();
            MaterialType matType = MaterialType.Diffuse;
            if ((pair?.textureFlags & 0x02) != 0 )
            {
                matType = MaterialType.Transparent;
            }
            return pair?.GetOrCreate(matType, this);
        }

        public Material GetOrCreateMaterial(int id, MaterialType matType)
        {
            TexMatsPair pair = texMatPairs.Where(x => x.texture.name.Contains(id.ToString("X4"))).FirstOrDefault();
            return pair?.GetOrCreate(matType, this);
        }

        public void ReadAndAddTexDXT1(string baseName, FileTex texFile)
        {
            (FileTex.DXTTexture, Texture2D)[] textures = ReadTexDXT1(baseName, texFile);
            for (int i = 0; i < textures.Length; i++)
            {
                (FileTex.DXTTexture, Texture2D) tex = textures[i];
                texMatPairs.Add(new TexMatsPair(tex.Item2, tex.Item1.subgroups[0].field_1C));
                AssetDatabase.AddObjectToAsset(tex.Item2, AssetDatabase.GetAssetPath(this));
            }
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

        protected Material CreateDiffuse(Texture tex)
        {
            Material mat = new Material(Shader.Find("Legacy Shaders/Diffuse"));
            mat.mainTexture = tex;
            mat.name = tex.name + "_diffuse";
            return mat;
        }

        protected Material CreateTransparent(Texture tex)
        {

            Material mat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
            mat.mainTexture = tex;
            mat.name = tex.name + "_transparent";
            return mat;
        }

        protected Material CreateCutout(Texture tex)
        {
            Material mat = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse"));
            mat.mainTexture = tex;
            mat.name = tex.name + "_cutout";
            return mat;
        }

        protected Material CreateSelfIllum(Texture tex)
        {
            Material mat = new Material(Shader.Find("Legacy Shaders/Self-Illum/Diffuse"));
            mat.mainTexture = tex;
            mat.SetTexture("_EmissionMap", tex);
            mat.name = tex.name + "_selfIllum";
            return mat;
        }
    }
}
