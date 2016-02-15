using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

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
            string prefabPath = path.Replace(".map", ".prefab");
            string assetPath = path.Replace(".map", ".asset");

            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            GameObject prefabGo = null;
            GameObject map = null;

            if(prefab == null)
            {
                prefabGo = new GameObject("Scene");
                prefabGo.isStatic = true;
            }
            else
            {
                prefabGo = (GameObject)GameObject.Instantiate(prefab);
                PrefabUtility.DisconnectPrefabInstance(prefabGo);
                Transform existingMap = prefabGo.transform.FindChild("Map");
                if(existingMap != null)
                {
                    DestroyImmediate(existingMap.gameObject);
                }
            }

            prefabGo.transform.localScale = Vector3.one;
            map = new GameObject("Map");
            map.transform.SetParent(prefabGo.transform);
            map.isStatic = true;

            try
            {
                Scene scene = map.AddComponent<Scene>();

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
                    sky = Skybox.Deserialise(reader, map);
                } while (sky.NextSkyboxOffset != 0);

                //Read meshgroups
                int next;
                do
                {
                    next = MeshGroup.Deserialise(reader, map);
                } while (next != 0);

                //reader.BaseStream.Position = transformOffset;
                //Matrix4x4 mat4x4 = reader.ReadMatrix4x4();
                Matrix4x4Utils.SetTransformFromMatrix(map.transform, ref map.GetComponentInChildren<Skybox>().Matrix);

                reader.Close();

                //Asset bookkeeping
                MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(assetPath);
                rolodex.AddTextures(textures);

                int baseIndex = 0;

                MeshGroup[] groups = map.GetComponentsInChildren<MeshGroup>();
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

                foreach (MeshFilter mf in map.GetComponentsInChildren<MeshFilter>())
                {
                    AssetDatabase.AddObjectToAsset(mf.sharedMesh, assetPath);
                }

                prefabGo.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

                if (prefab != null)
                {
                    PrefabUtility.ReplacePrefab(prefabGo, prefab);
                }
                else
                {
                    PrefabUtility.CreatePrefab(prefabPath, prefabGo);
                }
                
                AssetDatabase.SaveAssets();

                DestroyImmediate(prefabGo, false);

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