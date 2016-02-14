using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{

    public class Scene : MonoBehaviour 
    {
        public int Unknown1;
        public int Unknown2;
        public short TotalTextures;
        public short LocalTextureBaseIndex;
        public short LocalTextureCount;
        public short LocalTextureBaseIndexModifier;

        public static Scene ReadMap(string path)
        {
            GameObject go = new GameObject("Map");
            go.isStatic = true;
            try
            {
                Scene scene = go.AddComponent<Scene>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

                //Header
                reader.SkipInt32(-1); //marker
                reader.SkipInt32(0);
                reader.SkipInt32(0);
                reader.SkipInt32(80); //Main header size
                int TextureGroupOffset = reader.ReadInt32();
                reader.SkipInt32(0);
                reader.SkipInt32(80); //Alt main header size
                reader.SkipInt32(); //Total main header size
                scene.Unknown1 = reader.ReadInt32();
                reader.SkipInt32(); //Scene star header offset
                reader.SkipInt32(0);
                reader.SkipInt32(0);
                reader.SkipInt32(); //TextureGroupOffset2
                reader.SkipInt32(); //TransformOffset
                scene.Unknown2 = reader.ReadInt32();
                reader.SkipInt32(0);
                scene.TotalTextures = reader.ReadInt16();
                scene.LocalTextureBaseIndex = reader.ReadInt16();
                scene.LocalTextureCount = reader.ReadInt16();
                scene.LocalTextureBaseIndexModifier = reader.ReadInt16();
                reader.SkipInt32(0);
                reader.SkipInt32(0);

                //Read textures
                long goBack = reader.BaseStream.Position;
                reader.BaseStream.Position = TextureGroupOffset;
                Texture2D[] textures = TextureUtils.ReadTex32(Path.GetFileName(path).Replace(".map", "_tex"), reader);

                reader.BaseStream.Position = goBack;

                //Read Skyboxes
                Skybox sky = null;
                do
                {
                    sky = Skybox.Deserialise(reader, go);
                } while (sky.NextSkyboxOffset != 0);

                //Read meshgroups
                int next;
                do
                {
                    next = MeshGroup.Deserialise(reader, go);
                } while (next != 0);

                MeshGroup[] groups = go.GetComponentsInChildren<MeshGroup>();

                reader.Close();

                //Associate materials
                string prefabPath = path.Replace(".map", ".prefab");
                string assetPath = path.Replace(".map", ".asset");

                MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(assetPath);
                rolodex.AddTextures(textures);

                int baseIndex = 0;

                foreach (MeshGroup group in groups)
                {
                    MaterialRolodex goodRolodex = null;
                    if (group.TextureGroup == 3)
                    {
                        goodRolodex = rolodex;
                        baseIndex = scene.LocalTextureBaseIndex + scene.LocalTextureBaseIndexModifier;
                    }
                    else if (group.TextureGroup == 2)
                    {
                        goodRolodex = MaterialRolodex.GetOrCreateAt(path.Replace(".map", "TR.tex"));
                    }
                    else if (group.TextureGroup == 1)
                    {
                        string name = Path.GetFileName(path);
                        goodRolodex = MaterialRolodex.GetOrCreateAt(path.Replace(name, name.Substring(0, 2)+"GB.tex"));
                    }
                    else
                    {
                        Debug.LogWarning("Unknown texture group " + group.TextureGroup + " on " + group.gameObject);
                        continue;
                    }

                    MaterialRolodex.TexMatsPair tmp = goodRolodex.GetWithSH3Index(group.TextureIndex, baseIndex);
                    foreach (SubMeshGroup subMeshGroup in group.GetComponentsInChildren<SubMeshGroup>())
                    {
                        foreach (SubSubMeshGroup subSubMeshGroup in subMeshGroup.GetComponentsInChildren<SubSubMeshGroup>())
                        {
                            foreach (MeshRenderer renderer in subSubMeshGroup.GetComponentsInChildren<MeshRenderer>())
                            {
                                if (subMeshGroup.IsTransparent == 1)
                                {
                                    renderer.sharedMaterial = tmp.GetOrCreateTransparent();
                                }
                                else if (subMeshGroup.IsTransparent == 3)
                                {
                                    renderer.sharedMaterial = tmp.GetOrCreateCutout();
                                }
                                else if (subSubMeshGroup.Illumination == 8)
                                {
                                    renderer.sharedMaterial = tmp.GetOrCreateSelfIllum();
                                }
                                else
                                {
                                    renderer.sharedMaterial = tmp.GetOrCreateDiffuse();
                                }
                            }
                        }
                    }
                }

                foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>())
                {
                    AssetDatabase.AddObjectToAsset(mf.sharedMesh, assetPath);
                }

                PrefabUtility.CreatePrefab(prefabPath, go);
                
                AssetDatabase.SaveAssets();

                DestroyImmediate(go);

                return scene;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }
    }
}