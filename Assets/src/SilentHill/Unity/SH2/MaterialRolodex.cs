using System.Collections.Generic;
using System.IO;
using System.Linq;

using System;

using UnityEngine;

using SH.GameData.SH2;
using SH.GameData.Shared;
using UnityEditor;

namespace SH.Unity.SH2
{
    public class MaterialRolodex : ScriptableObject
    {
        [Serializable]
        public struct MaterialStruct
        {
            public FileGeometry.MapMaterial matinfo;

            [Hex] public int materialId;
            [Hex] public int textureId;
            public Material material;
        }

        public TextureRolodex texturesRolodex;
        [SerializeField]
        public List<MaterialStruct> materials = new List<MaterialStruct>();

        public void AddMaterials(FileGeometry fileGeo)
        {
            for(int i = 0; i < fileGeo.mapMaterials.Length; i++)
            {
                FileGeometry.MapMaterial mapMaterial = fileGeo.mapMaterials[i];

                CheckVersionsNeededForMapMaterial(fileGeo, i, out bool needsOpaque, out bool needsTransparent);

                if(needsOpaque && needsTransparent)
                {
                    Debug.LogError("Oh fuck");
                }

                Texture2D texture = texturesRolodex.GetTexture(mapMaterial.textureId);
                Material mat = null;
                if(mapMaterial.mode == 0)
                {
                    mat = CreateSelfIllum(texture, mapMaterial.materialColor, needsOpaque, needsTransparent);
                }
                else if (mapMaterial.mode == 1)
                {
                    mat = CreateCutout(texture, mapMaterial.materialColor, needsOpaque, needsTransparent);
                }
                else if (mapMaterial.mode == 2)
                {
                    mat = CreateSpecular(texture, mapMaterial.materialColor, mapMaterial.overlayColor, mapMaterial.specularity, needsOpaque, needsTransparent);
                }
                else if (mapMaterial.mode == 3)
                {
                    Debug.LogError("Mode 3 wanted");
                    mat = CreateSelfIllum(texture, Color.white, needsOpaque, needsTransparent);
                }
                else if (mapMaterial.mode == 4)
                {
                    mat = CreateDiffuse(texture, texture == null ? (Color)mapMaterial.materialColor : Color.white, needsOpaque, needsTransparent);
                }
                else if (mapMaterial.mode == 5)
                {
                    Debug.LogError("Mode 5 wanted");
                }
                else if (mapMaterial.mode == 6)
                {
                    Debug.LogError("Mode 6 wanted");
                    mat = CreateDiffuse(texture, mapMaterial.materialColor, needsOpaque, needsTransparent);
                }
                else
                {
                    Debug.LogError("Mode " + mapMaterial.mode + " wanted");
                }

                materials.Add(new MaterialStruct()
                {
                    matinfo = mapMaterial,
                    material = mat,
                    materialId = i,
                    textureId = mapMaterial.textureId
                });

                if (mat != null)
                {
                    AssetDatabase.AddObjectToAsset(mat, this);
                }
            }
        }

        protected bool GetMaterialStruct(int id, out MaterialStruct matStruct)
        {
            if (materials != null)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    MaterialStruct mat = materials[i];
                    if(mat.materialId == id)
                    {
                        matStruct = mat;
                        return true;
                    }
                }
            }

            matStruct = default;
            return false;
        }

        public Material GetMaterial(int id)
        {
            Material mat = null;
            if(GetMaterialStruct(id, out MaterialStruct matStruct))
            {
                mat = matStruct.material;
            }
            return mat;
        }

        private static void CheckVersionsNeededForMapMaterial(FileGeometry fileGeo, int materialId, out bool needsOpaque, out bool needsTransparent)
        {
            needsOpaque = false;
            needsTransparent = false;
            for (int i = 0; i < fileGeo.geometries.Length; i++)
            {
                FileGeometry.Geometry geo = fileGeo.geometries[i];
                for (int j = 0; j < 2; j++)
                {
                    FileGeometry.Geometry.MeshGroup meshGroup = (j == 0 ? geo.opaqueGroup : geo.transparentGroup);
                    if (meshGroup != null)
                    {
                        for (int k = 0; k < meshGroup.subMeshGroups.Length; k++)
                        {
                            FileGeometry.Geometry.MeshGroup.SubMeshGroup subMeshGroup = meshGroup.subMeshGroups[k];
                            for (int l = 0; l < subMeshGroup.subSubMeshGroups.Length; l++)
                            {
                                FileGeometry.Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup subSubMeshGroup = subMeshGroup.subSubMeshGroups[l];
                                if (subSubMeshGroup.header.materialIndex == materialId)
                                {
                                    if (j == 0)
                                    {
                                        needsOpaque = true;
                                    }
                                    else
                                    {
                                        needsTransparent = true;
                                    }
                                    l = subMeshGroup.subSubMeshGroups.Length;
                                    k = meshGroup.subMeshGroups.Length;
                                }
                            }
                        }
                    }
                }
                if (needsTransparent == false && geo.mapDecals != null)
                {
                    for (int j = 0; j < geo.mapDecals.decals.Length; j++)
                    {
                        FileGeometry.Geometry.MapDecals.Decal decal = geo.mapDecals.decals[j];
                        for (int k = 0; k < decal.subDecals.Length; k++)
                        {
                            FileGeometry.Geometry.MapDecals.Decal.SubDecal subDecal = decal.subDecals[k];
                            if (subDecal.materialIndex == materialId)
                            {
                                needsTransparent = true;
                                k = decal.subDecals.Length;
                                j = geo.mapDecals.decals.Length;
                            }
                        }
                    }
                }

                if (needsOpaque && needsTransparent)
                {
                    return;
                }
            }
        }

        private static Material CreateDiffuse(Texture2D tex, Color mainColor, bool opaque, bool transparent)
        {
            Material mat = null;
            if (opaque)
            {
                mat = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse"));
                mat.name = (tex != null ? tex.name : "0000") + "_diffuse";
            }
            if (transparent)
            {
                mat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
                mat.name = (tex != null ? tex.name : "0000") + "_diffuse_t";
            }

            if(mat != null)
            {
                mat.mainTexture = tex;
                mat.color = mainColor;
            }

            return mat;
        }

        private static Material CreateCutout(Texture2D tex, Color mainColor, bool opaque, bool transparent)
        {
            Material mat = null;
            if (opaque)
            {
                mat = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse"));
                mat.name = (tex != null ? tex.name : "0000") + "_cutout";
            }
            if (transparent)
            {
                mat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
                mat.name = (tex != null ? tex.name : "0000") + "_cutout_t";
            }

            if (mat != null)
            {
                mat.mainTexture = tex;
                if (tex == null)
                {
                    mat.color = mainColor;
                }
            }

            return mat;
        }

        private static Material CreateSpecular(Texture2D tex, Color mainColor, Color specularColor, float specularity, bool opaque, bool transparent)
        {
            Material mat = null;
            if (opaque)
            {
                mat = new Material(Shader.Find("Legacy Shaders/Specular"));
                mat.name = (tex != null ? tex.name : "0000") + "_specular";
            }
            if (transparent)
            {
                mat = new Material(Shader.Find("Legacy Shaders/Transparent/Specular"));
                mat.name = (tex != null ? tex.name : "0000") + "_specular_t";
            }

            if (mat != null)
            {
                mat.mainTexture = tex;
                mat.color = mainColor;
                mat.SetColor("_SpecColor", specularColor);
                mat.SetFloat("_RangeShininess", specularity / 100.0f);
            }

            return mat;
        }

        private static Material CreateSelfIllum(Texture2D tex, Color mainColor, bool opaque, bool transparent)
        {
            Material mat = null;
            if (opaque)
            {
                mat = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/VertexLit"));
                mat.SetFloat("_Cutoff", 0.0f);
                mat.name = (tex != null ? tex.name : "0000") + "_vertexlit";
            }
            if (transparent)
            {
                mat = new Material(Shader.Find("Legacy Shaders/Transparent/VertexLit"));
                mat.name = (tex != null ? tex.name : "0000") + "_vertexlit_t";
            }

            if (mat != null)
            {
                mat.SetColor("_Color", Color.white);
                mat.SetColor("_SpecColor", Color.white);
                mat.SetColor("_Emission", mainColor);
                mat.SetTexture("_MainTex", tex);
            }

            return mat;
        }
    }
}
