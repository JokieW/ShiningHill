using System.Collections.Generic;

using UnityEngine;

using SH.GameData.SH3;
using SH.Unity.Shared;

namespace SH.Unity.SH3
{
    public class MaterialRolodex : MaterialRolodexBase
    {
        public Material GetWithSH3Index(int index, MaterialType matType)
        {
            if (index < 0) { index += texMatPairs.Count; }
            if (index >= texMatPairs.Count) { index -= texMatPairs.Count; }
            return texMatPairs[index].GetOrCreate(matType, this);
        }

        public static MaterialType SH3MaterialToType(in MapGeometry.SubMeshGroup subMeshGroup, in MapGeometry.SubSubMeshGroup subSubMeshGroup)
        {
            if (subMeshGroup.header.transparencyType == 1)
            {
                return MaterialType.Transparent;
            }
            else if (subMeshGroup.header.transparencyType == 3)
            {
                return MaterialType.Cutout;
            }
            else if (subSubMeshGroup.header.illuminationType == 8)
            {
                return MaterialType.SelfIllum;
            }
            else
            {
                return MaterialType.Diffuse;
            }
        }

        public static Texture2D[] ReadTex32(string groupName, in TextureGroup textureGroup)
        {
            List<Texture2D> textures = new List<Texture2D>(textureGroup.textures.Length);

            for (int i = 0; i < textureGroup.textures.Length; i++)
            {
                ref readonly TextureGroup.Texture texstruct = ref textureGroup.textures[i];

                TextureFormat format = (texstruct.header.bitsPerPixel == 24 ? TextureFormat.RGB24 : TextureFormat.RGBA32);
                Texture2D tex = new Texture2D(texstruct.header.textureWidth, texstruct.header.textureHeight, format, false);
                tex.SetPixelData(texstruct.pixels, 0);
                tex.Apply();

                tex.alphaIsTransparency = true;
                tex.name = groupName + i;

                textures.Add(tex);
            }

            return textures.ToArray();
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
            Material mat = new Material(Shader.Find("Legacy Shaders/Self-Illumin/Diffuse"));
            mat.mainTexture = tex;
            mat.SetTexture("_EmissionMap", tex);
            mat.name = tex.name + "_selfIllum";
            return mat;
        }
    }
}
