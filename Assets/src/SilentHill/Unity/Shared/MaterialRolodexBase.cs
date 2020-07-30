using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SH.Unity.Shared
{
    [CreateAssetMenu]
	public abstract class MaterialRolodexBase : ScriptableObject 
	{
        [SerializeField]
        protected List<TexMatsPair> texMatPairs = new List<TexMatsPair>();

        public MaterialRolodexBase()
        {
        }

        protected abstract Material CreateDiffuse(Texture tex);
        protected abstract Material CreateTransparent(Texture tex);
        protected abstract Material CreateCutout(Texture tex);
        protected abstract Material CreateSelfIllum(Texture tex);

        public void AddTextures(Texture[] texs)
        {
            for(int i = 0; i < texs.Length; i++)
            {
                Texture tex = texs[i];
                texMatPairs.Add(new TexMatsPair(tex));
                AssetDatabase.AddObjectToAsset(tex, AssetDatabase.GetAssetPath(this));
            }
        }

        [Serializable]
        protected class TexMatsPair
        {
            [SerializeField]
            public Texture texture;
            [SerializeField]
            public Material[] materials;

            public TexMatsPair(Texture tex)
            {
                texture = tex;
                materials = new Material[(int)MaterialType.__Count];
            }

            public Material GetOrCreate(MaterialType matType, MaterialRolodexBase rolodex)
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
    }
}
