using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using SH.Core;
using SH.Unity.Shared;

namespace SH.Unity.SH2
{
    public class MaterialRolodex : MaterialRolodexBase
    {
        public Material GetWithSH2ID(int id, MaterialType matType)
        {
            return texMatPairs.Where(x => x.texture.name.Contains(id.ToString("0000"))).FirstOrDefault().GetOrCreate(matType, this);
        }

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
