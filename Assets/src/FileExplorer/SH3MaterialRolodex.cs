using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShiningHill
{
    public class SH3MaterialRolodex : MaterialRolodex
    {
        public Material GetWithSH3Index(int index, int baseIndex, MapFile.MaterialType matType)
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
