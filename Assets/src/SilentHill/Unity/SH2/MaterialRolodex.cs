using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using SH.Core;
using SH.Unity.Shared;
using SH.GameData.SH2;

namespace SH.Unity.SH2
{
    public class MaterialRolodex : MaterialRolodexBase
    {
        public Material GetWithSH2ID(int id, MaterialType matType)
        {
            TexMatsPair pair = texMatPairs.Where(x => x.texture.name.Contains(id.ToString("X4"))).FirstOrDefault();
            return pair?.GetOrCreate(matType, this);
        }

        public static Texture2D[] ReadTexDXT1(string baseName, FileTex texFile)
        {
            Texture2D[] textures = new Texture2D[texFile.textures.Length];

            for(int i = 0; i < textures.Length; i++)
            {
                FileTex.DXTTexture dxt = texFile.textures[i];
                Texture2D newTexture = new Texture2D(dxt.header.width, dxt.header.height, (dxt.subgroups[0].field_1C & 0x02) == 0 ? TextureFormat.DXT1 : TextureFormat.DXT5, false);
                newTexture.LoadRawTextureData(dxt.pixels);
                newTexture.Apply();

                newTexture.alphaIsTransparency = true;
                newTexture.name = baseName + dxt.header.textureId.ToString("X4");

                textures[i] = newTexture;
            }
            
            return textures;
        }

        protected override Material CreateDiffuse(Texture tex)
        {
            Material mat = new Material(Shader.Find("Legacy Shaders/Diffuse"));
            mat.mainTexture = tex;
            mat.name = tex.name + "_diffuse";
            return mat;
        }

        protected override Material CreateTransparent(Texture tex)
        {

            Material mat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
            mat.mainTexture = tex;
            mat.name = tex.name + "_transparent";
            return mat;
        }

        protected override Material CreateCutout(Texture tex)
        {
            Material mat = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse"));
            mat.mainTexture = tex;
            mat.name = tex.name + "_cutout";
            return mat;
        }

        protected override Material CreateSelfIllum(Texture tex)
        {
            Material mat = new Material(Shader.Find("Legacy Shaders/Self-Illum/Diffuse"));
            mat.mainTexture = tex;
            mat.SetTexture("_EmissionMap", tex);
            mat.name = tex.name + "_selfIllum";
            return mat;
        }
    }
}
