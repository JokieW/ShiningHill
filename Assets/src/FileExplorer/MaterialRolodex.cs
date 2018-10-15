using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        private static Material _defaultCutout;
        public static Material defaultCutout
        {
            get
            {
                if (_defaultCutout == null)
                {
                    _defaultCutout = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DefaultCutoutMaterial.mat");
                }
                return _defaultCutout;
            }
        }

        private static Material _defaultGizmo;
        public static Material defaultGizmo
        {
            get
            {
                if (_defaultGizmo == null)
                {
                    _defaultGizmo = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DefaultGizmoMaterial.mat");
                }
                return _defaultGizmo;
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
                AssetDatabase.CreateAsset(mr, path);
            }
            return mr;
        }

        public void AddTexture(Texture tex)
        {
            TexMatsPair tmp = texMatPairs.FirstOrDefault(x => x.textureName == tex.name);
            if (tmp != null)
            {
                EditorUtility.CopySerialized(tex, tmp.texture);
                tmp.texture.name = tmp.textureName;
            }
            else
            {
                texMatPairs.Add(new TexMatsPair(tex));
                AssetDatabase.AddObjectToAsset(tex, AssetDatabase.GetAssetPath(this));
            }
            AssetDatabase.SaveAssets();

        }
        public void AddTextures(Texture[] texs)
        {
            string path = AssetDatabase.GetAssetPath(this);
            foreach (Texture tex in texs)
            {
                TexMatsPair tmp = texMatPairs.FirstOrDefault(x => x.textureName == tex.name);
                if (tmp != null)
                {
                    EditorUtility.CopySerialized(tex, tmp.texture);
                    tmp.texture.name = tmp.textureName;
                }
                else
                {
                    texMatPairs.Add(new TexMatsPair(tex));
                    AssetDatabase.AddObjectToAsset(tex, path);
                }
            }
            AssetDatabase.SaveAssets();
        }

        public TexMatsPair GetWithSH2ID(int id)
        {
            return texMatPairs.Where(x => x.textureName.Contains(id.ToString("0000"))).FirstOrDefault();
        }

        public TexMatsPair GetWithSH3Index(int index, int baseIndex)
        {
            if (index < 0)
            {
                return texMatPairs[texMatPairs.Count + index];
            }
            return texMatPairs[index - baseIndex];
        }

        public void Cleanup()
        {
            texMatPairs.RemoveAll(x => x.texture == null);
        }

        [Serializable]
        public class TexMatsPair
        {
            public string textureName;
            [SerializeField]
            private Texture _texture;
            public Texture texture
            {
                get
                {
                    return _texture;
                }
                set
                {
                    if (value != null)
                    {
                        if (_texture != null)
                        {
                            EditorUtility.CopySerialized(value, _texture);
                        }
                        _texture = value;
                    }
                    else
                    {
                        diffuse = null;
                        transparent = null;
                        cutout = null;
                        selfIllum = null;
                        DestroyImmediate(_texture, true);
                    }

                    AssetDatabase.SaveAssets();
                }
            }

            [SerializeField]
            private Material _diffuse;
            public Material diffuse
            {
                get
                {
                    return _diffuse;
                }
                set
                {
                    if (value != null)
                    {
                        if (_diffuse != null)
                        {
                            EditorUtility.CopySerialized(value, _diffuse);
                        }
                        else
                        {
                            _diffuse = value;
                            AssetDatabase.AddObjectToAsset(_diffuse, AssetDatabase.GetAssetPath(texture));
                        }
                    }
                    else
                    {
                        if (_diffuse != null)
                        {
                            DestroyImmediate(_diffuse, true);
                        }
                    }
                    AssetDatabase.SaveAssets();
                }
            }

            public Material GetOrCreateDiffuse()
            {
                if (diffuse == null)
                {
                    Material mat = new Material(defaultDiffuse);
                    mat.mainTexture = texture;
                    mat.name = texture.name + "_diffuse";
                    diffuse = mat;
                }
                return diffuse;
            }

            [SerializeField]
            private Material _transparent;
            public Material transparent
            {
                get
                {
                    return _transparent;
                }
                set
                {
                    if (value != null)
                    {
                        if (_transparent != null)
                        {
                            EditorUtility.CopySerialized(value, _transparent);
                        }
                        else
                        {
                            _transparent = value;
                            AssetDatabase.AddObjectToAsset(_transparent, AssetDatabase.GetAssetPath(texture));
                        }
                    }
                    else
                    {
                        if (_transparent != null)
                        {
                            DestroyImmediate(_transparent, true);
                        }
                    }
                    AssetDatabase.SaveAssets();
                }
            }

            public Material GetOrCreateTransparent()
            {
                if (transparent == null)
                {
                    Material mat = new Material(defaultTransparent);
                    mat.mainTexture = texture;
                    mat.name = texture.name + "_transparent";
                    transparent = mat;
                }
                return transparent;
            }

            [SerializeField]
            private Material _cutout;
            public Material cutout
            {
                get
                {
                    return _cutout;
                }
                set
                {
                    if (value != null)
                    {
                        if (_cutout != null)
                        {
                            EditorUtility.CopySerialized(value, _cutout);
                        }
                        else
                        {
                            _cutout = value;
                            AssetDatabase.AddObjectToAsset(_cutout, AssetDatabase.GetAssetPath(texture));
                        }
                    }
                    else
                    {
                        if (_cutout != null)
                        {
                            DestroyImmediate(_cutout, true);
                        }
                    }
                    AssetDatabase.SaveAssets();
                }
            }

            public Material GetOrCreateCutout()
            {
                if (_cutout == null)
                {
                    Material mat = new Material(defaultCutout);
                    mat.mainTexture = texture;
                    mat.name = texture.name + "_cutout";
                    cutout = mat;
                }
                return cutout;
            }

            [SerializeField]
            private Material _selfIllum;
            public Material selfIllum
            {
                get
                {
                    return _selfIllum;
                }
                set
                {
                    if (value != null)
                    {
                        if (_selfIllum != null)
                        {
                            EditorUtility.CopySerialized(value, _selfIllum);
                        }
                        else
                        {
                            _selfIllum = value;
                            AssetDatabase.AddObjectToAsset(_selfIllum, AssetDatabase.GetAssetPath(texture));
                        }
                    }
                    else
                    {
                        if (_selfIllum != null)
                        {
                            DestroyImmediate(_selfIllum, true);
                        }
                    }
                    AssetDatabase.SaveAssets();
                }
            }

            public Material GetOrCreateSelfIllum()
            {
                if (selfIllum == null)
                {
                    Material mat = new Material(defaultSelfIllum);
                    mat.mainTexture = texture;
                    mat.SetTexture("_EmissionMap", texture);
                    mat.name = texture.name + "_selfIllum";
                    selfIllum = mat;
                }
                return selfIllum;
            }

            public TexMatsPair(Texture tex)
            {
                texture = tex;
                textureName = tex.name;
            }
        }
	}
}