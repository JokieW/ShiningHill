using UnityEngine;

using SH.DataFormat.SH3;
using SH.Unity.Shared;

namespace SH.Unity.SH3
{
    public class MaterialRolodex : MaterialRolodexBase
    {
        public Material GetWithSH3Index(int index, int baseIndex, MaterialType matType)
        {
            TexMatsPair pair;
            if (index < 0)
            {
                pair = texMatPairs[texMatPairs.Count + index];
            }
            else
            {
                pair = texMatPairs[index - baseIndex];
            }
            return pair.GetOrCreate(matType, this);
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
