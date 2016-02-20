using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
    public class Map : MonoBehaviour 
    {
        public int Unknown1;
        public int Unknown2;
        public short TotalTextures;
        public short LocalTextureBaseIndex;
        public short LocalTextureCount;
        public short LocalTextureBaseIndexModifier;

        public static Map ReadMap(string path)
        {
            string assetPath = path.Replace(".map", ".asset");
            GameObject subGO = Map.BeginEditingPrefab(path, "Map");

            try
            {
                Map scene = subGO.AddComponent<Map>();

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
                int transformOffset = reader.ReadInt32();
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
                    sky = Skybox.Deserialise(reader, subGO);
                } while (sky.NextSkyboxOffset != 0);

                //Read meshgroups
                int next;
                do
                {
                    next = MeshGroup.Deserialise(reader, subGO);
                } while (next != 0);

                //reader.BaseStream.Position = transformOffset;
                //Matrix4x4 mat4x4 = reader.ReadMatrix4x4();
                Matrix4x4Utils.SetTransformFromSH3Matrix(subGO.transform, ref subGO.GetComponentInChildren<Skybox>().Matrix);

                reader.Close();

                //Asset bookkeeping
                MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(assetPath);
                rolodex.AddTextures(textures);

                int baseIndex = 0;

                MeshGroup[] groups = subGO.GetComponentsInChildren<MeshGroup>();
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
                        string trpath; 
                        if (path.Contains("cc/cc"))
                        {
                            trpath = path.Substring(0, path.IndexOf(".map") - 2) + "01TR.tex";
                        }
                        else
                        {
                            trpath = path.Replace(".map", "TR.tex");
                        }
                        goodRolodex = MaterialRolodex.GetOrCreateAt(trpath);
                    }
                    else if (group.TextureGroup == 1)
                    {
                        string name = Path.GetFileName(path);
                        goodRolodex = MaterialRolodex.GetOrCreateAt(path.Replace(name, name.Substring(0, 2)+"GB.tex"));
                    }
                    else
                    {
                        Debug.LogWarning("Unknown texture group " + group.TextureGroup + " on " + group.gameObject);
                    }

                    if (goodRolodex == null)
                    {
                        Debug.LogWarning("Couldn't find rolodex for group " + group.TextureGroup + " on " + path);
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

                foreach (MeshFilter mf in subGO.GetComponentsInChildren<MeshFilter>())
                {
                    AssetDatabase.AddObjectToAsset(mf.sharedMesh, assetPath);
                }

                Map.FinishEditingPrefab(path, subGO);

                return scene;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        public static GameObject BeginEditingPrefab(string path, string childName)
        {
            string prefabPath = path.Replace(Path.GetExtension(path), ".prefab");

            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            GameObject prefabGo = null;
            GameObject subGO = null;

            if (prefab == null)
            {
                prefabGo = new GameObject("Area");
                prefabGo.isStatic = true;
            }
            else
            {
                prefabGo = (GameObject)GameObject.Instantiate(prefab);
                PrefabUtility.DisconnectPrefabInstance(prefabGo);
                Transform existingMap = prefabGo.transform.FindChild(childName);
                if (existingMap != null)
                {
                    DestroyImmediate(existingMap.gameObject);
                }
            }

            prefabGo.transform.localScale = Vector3.one;
            subGO = new GameObject(childName);
            subGO.transform.SetParent(prefabGo.transform);
            subGO.isStatic = true;

            return subGO;
        }

        public static void FinishEditingPrefab(string path, GameObject subGO)
        {
            string prefabPath = path.Replace(Path.GetExtension(path), ".prefab");
            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            GameObject prefabGO = subGO.transform.parent.gameObject;
            prefabGO.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

            if (prefab != null)
            {
                PrefabUtility.ReplacePrefab(prefabGO, prefab);
            }
            else
            {
                PrefabUtility.CreatePrefab(prefabPath, prefabGO);
            }

            AssetDatabase.SaveAssets();

            DestroyImmediate(prefabGO, false);
        }
    }
}