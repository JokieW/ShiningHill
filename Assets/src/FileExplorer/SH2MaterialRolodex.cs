using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShiningHill
{
    public class SH2MaterialRolodex : MaterialRolodex
    {
        public Material GetWithSH2ID(int id, MapFile.MaterialType matType)
        {
            return texMatPairs.Where(x => x.texture.name.Contains(id.ToString("0000"))).FirstOrDefault().GetOrCreate(matType, this);
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
