using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{
    [CreateAssetMenu]
	public class MaterialRolodex : ScriptableObject 
	{
        //Script assigned
        private static Material _defaultDiffuse;
        public static Material defaultDiffuse
        {
            get
            {
                if (_defaultDiffuse == null)
                {
                    _defaultDiffuse = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DefaultDiffuseMaterial.mat");
                }
                return _defaultDiffuse;
            }
        }

        private static Material _defaultSelfIllum;
        public static Material defaultSelfIllum
        {
            get
            {
                if (_defaultSelfIllum == null)
                {
                    _defaultSelfIllum = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DefaultSelfIllumMaterial.mat");
                }
                return _defaultSelfIllum;
            }
        }

        private static Material _defaultTransparent;
        public static Material defaultTransparent
        {
            get
            {
                if (_defaultTransparent == null)
                {
                    _defaultTransparent = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DefaultTransparentMaterial.mat");
                }
                return _defaultTransparent;
            }
        }

        public List<TexMatsPair> texMatPairs = new List<TexMatsPair>();

        private MaterialRolodex(){}
        public static MaterialRolodex GetOrCreateAt(string path)
        {
            MaterialRolodex mr = AssetDatabase.LoadAssetAtPath<MaterialRolodex>(path);
            if (mr == null)
            {
                mr = MaterialRolodex.CreateInstance<MaterialRolodex>();
            }
            else
            {
                mr.texMatPairs.Clear();
            }
            return mr;
        }

        public void AddTexture(Texture tex)
        {
            texMatPairs.Add(new TexMatsPair(tex));
        }
        public void AddTextures(Texture[] texs)
        {
            foreach (Texture tex in texs)
            {
                texMatPairs.Add(new TexMatsPair(tex));
            }
        }

        public void SaveFile(string path)
        {
            AssetDatabase.StartAssetEditing();
            AssetDatabase.CreateAsset(this, path);
            foreach (TexMatsPair tmp in texMatPairs)
            {
                AssetDatabase.AddObjectToAsset(tmp.texture, path);
                AssetDatabase.AddObjectToAsset(tmp.diffuse, path);
                AssetDatabase.AddObjectToAsset(tmp.transparent, path);
                AssetDatabase.AddObjectToAsset(tmp.selfIllum, path);
            }
            AssetDatabase.StopAssetEditing();
        }

        [Serializable]
        public class TexMatsPair
        {
            [SerializeField]
            public Texture texture;
            [SerializeField]
            public Material diffuse;
            [SerializeField]
            public Material transparent;
            [SerializeField]
            public Material selfIllum;

            public TexMatsPair(Texture tex)
            {
                texture = tex;
                diffuse = new Material(defaultDiffuse);
                diffuse.mainTexture = tex;
                diffuse.name = tex.name + "_diffuse";
                transparent = new Material(defaultTransparent);
                transparent.mainTexture = tex;
                transparent.name = tex.name + "_transparent";
                selfIllum = new Material(defaultSelfIllum);
                selfIllum.mainTexture = tex;
                selfIllum.name = tex.name + "_selfIllum";
            }
        }
	}
}