using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
    public struct MapAssetPaths
    {
        string mapName;
        string genericPath;
        SHGame game;

        public MapAssetPaths(string hardAssetPath, SHGame forgame)
        {
            mapName = Path.GetFileNameWithoutExtension(hardAssetPath);
            if (forgame == SHGame.SH3PC || forgame == SHGame.SH3PCdemo)
            {
                genericPath = Path.GetDirectoryName(hardAssetPath).Substring(hardAssetPath.LastIndexOf("/data/data/") + 1).Replace("\\", "/") + "/";
            }
            else
            {
                genericPath = Path.GetDirectoryName(hardAssetPath).Substring(hardAssetPath.LastIndexOf("/data/") + 1).Replace("\\", "/") + "/";
            }
            game = forgame;
        }

        public string GetTextureName()
        {
            return mapName + "_tex";
        }

        public string GetHardAssetPath()
        {
            string path = CustomPostprocessor.GetHardDataPathFor(game);
            return path + genericPath + mapName + ".map";
        }

        public string GetExtractAssetPath()
        {
            string path = CustomPostprocessor.GetExtractDataPathFor(game);
            return path + genericPath + mapName + ".asset";
        }

        public string GetPrefabPath()
        {
            string path = CustomPostprocessor.GetExtractDataPathFor(game);
            return path + genericPath + "Prefabs/" + mapName + ".prefab";
        }

        public TexAssetPaths GetHardTextureTRPaths()
        {
            string path = CustomPostprocessor.GetHardDataPathFor(game);
            if (path.Contains("cc/cc")) // Done for SH3, Check for SH2
            {
                return new TexAssetPaths(path + genericPath + "cc01TR.tex", game);
            }
            return new TexAssetPaths(path + genericPath + mapName + "TR.tex", game);
        }

        public TexAssetPaths GetHardTextureGBPaths()
        {
            string path = CustomPostprocessor.GetHardDataPathFor(game);
            string mapcode = genericPath.Substring(genericPath.Length - 3, 2);
            return new TexAssetPaths(path + genericPath + mapcode + "GB.tex", game);
        }
    }

    public class Map : MonoBehaviour 
    {
        public int Unknown1;
        public int Unknown2;
        public short TotalTextures;
        public short LocalTextureBaseIndex;
        public short LocalTextureCount;
        public short LocalTextureBaseIndexModifier;

        public static Map ReadMap(MapAssetPaths asset)
        {
            GameObject subGO = Scene.BeginEditingPrefab(asset.GetPrefabPath(), "Map");

            try
            {
                Map scene = subGO.AddComponent<Map>();

                BinaryReader reader = new BinaryReader(new FileStream(asset.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));

                //Header
                int marker = reader.PeekInt32(); //marker

                if (marker != -1) //SH2
                {
                    ReadSH2Map(reader, scene, asset);
                    Scene.FinishEditingPrefab(asset.GetPrefabPath(), subGO);
                }
                else if (marker == -1) //SH3
                {
                    ReadSH3Map(reader, scene, asset);
                    Scene.FinishEditingPrefab(asset.GetPrefabPath(), subGO);
                }

                return scene;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        static void ReadSH2Map(BinaryReader reader, Map scene, MapAssetPaths paths)
        {
            GameObject subGO = scene.gameObject;

            /*int fileID = */reader.ReadInt32();
            /*int fileSize = */reader.ReadInt32();
            /*int Unknown1 = */reader.ReadInt32();
            reader.SkipInt32(0);

            //Textures
            Texture[] textures = TextureUtils.ReadDDS(paths.GetTextureName(), reader);


            //Meshes
            reader.SkipInt32(1);
            reader.SkipInt32(); //Length from magic to bottom
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            long magicPosition = reader.BaseStream.Position;
            reader.SkipInt32(0x20010730); //date?
            reader.SkipInt32(1);
            int materialsOffset = reader.ReadInt32() + (int)magicPosition; 
            int meshCount = reader.ReadInt32();

            reader.SkipInt32(0);
            reader.SkipInt32(); // Length of elements from 0^
            reader.SkipInt32(20);
            reader.SkipInt32(0);

            reader.SkipInt32(0);
            reader.SkipInt32(1);
            reader.SkipInt32(8);
            
            long v1offset = reader.BaseStream.Position;
            reader.ReadVector3(); //V1
            reader.SkipInt32(0);
            reader.ReadVector3(); //V2
            reader.SkipInt32(0);
            /*int headerLength = */reader.ReadInt32(); //From v1 to vertexLength

            int indicesOffset = reader.ReadInt32() + (int)v1offset;
            /*int indicesLength = */reader.ReadInt32();
            /*int Unknown = */reader.ReadInt32();
            reader.SkipInt32(meshCount);

            List<MeshGroupSH2> groups = new List<MeshGroupSH2>();
            for (int i = 0; i != meshCount; i++)
            {
                groups.Add(MeshGroupSH2.Initialise(reader, subGO));
            }

            int vertexLength = reader.ReadInt32();
            reader.SkipInt32(1);
            reader.SkipInt32(0);
            int elementLength = reader.ReadInt32();
            reader.SkipInt32(vertexLength);

            int vertexElementsCount = vertexLength / elementLength;
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Color32> colors = new List<Color32>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i != vertexElementsCount; i++)
            {
                verts.Add(reader.ReadVector3());
                norms.Add(reader.ReadVector3());
                if (elementLength == 36)
                {
                    colors.Add(reader.ReadBGRA());
                }
                uvs.Add(reader.ReadVector2());
            }

            reader.BaseStream.Position = indicesOffset;

            List<short[]> indices = new List<short[]>(groups.Count);

            //stupid
            for (int i = 0; i != groups.Count; i++)
            {
                indices.Add(null);
            }

            for(int i = 0; i != groups.Count; i++)
            {
                MeshGroupSH2 group = groups[i];
                indices[group.MainID] = new short[group.indexCount];
                for (int j = 0; j != group.indexCount ; j++)
                {
                    indices[group.MainID][j] = reader.ReadInt16();
                }
                Debug.Log("End of i = " + reader.BaseStream.Position.ToString("X"));
            }

            reader.BaseStream.Position = materialsOffset;

            //Mesh renderer
            MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(paths.GetExtractAssetPath());
            rolodex.AddTextures(textures);
            

            MeshRenderer mr = subGO.AddComponent<MeshRenderer>();
            Material[] mats = new Material[groups.Count];
            for (int i = 0; i != groups.Count; i++)
            {
                reader.SkipInt16();
                MaterialRolodex.TexMatsPair tmp = rolodex.GetWithSH2ID(reader.ReadInt16());
                mats[i] = tmp.GetOrCreateDiffuse();
                reader.SkipBytes(12);
            }
            mr.sharedMaterials = mats;

            reader.Close();

            //Mesh filter
            subGO.AddComponent<MeshFilter>().sharedMesh = MeshUtils.MakeIndexedStrip(verts, indices, norms, uvs, colors);

            foreach (MeshFilter mf in subGO.GetComponentsInChildren<MeshFilter>())
            {
                AssetDatabase.AddObjectToAsset(mf.sharedMesh, paths.GetExtractAssetPath());
            }
        }

        void OnDrawGizmosSelected()
        {
            //Normal debug
            /*Gizmos.color = Color.red;
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            Vector3[] norms = mesh.normals;
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i != norms.Length; i++)
            {
                Vector3 localpos = transform.TransformPoint(verts[i]);
                
                Gizmos.DrawLine(localpos, localpos + (norms[i]*0.1f));
            }*/

        }

        static void ReadSH3Map(BinaryReader reader, Map scene, MapAssetPaths paths)
        {
            GameObject subGO = scene.gameObject;

            reader.SkipInt32(-1);
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
            /*int transformOffset = */
            reader.ReadInt32();
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
            Texture2D[] textures = TextureUtils.ReadTex32(paths.GetTextureName(), reader);

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
            Matrix4x4Utils.SetTransformFromMatrix(subGO.transform, ref subGO.GetComponentInChildren<Skybox>().Matrix);

            reader.Close();

            //Asset bookkeeping
            MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(paths.GetExtractAssetPath());
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
                    goodRolodex = CustomPostprocessor.ProcessTEX(paths.GetHardTextureTRPaths());
                }
                else if (group.TextureGroup == 1)
                {
                    goodRolodex = CustomPostprocessor.ProcessTEX(paths.GetHardTextureGBPaths());
                }
                else
                {
                    Debug.LogWarning("Unknown texture group " + group.TextureGroup + " on " + group.gameObject);
                }

                if (goodRolodex == null)
                {
                    Debug.LogWarning("Couldn't find rolodex for group " + group.TextureGroup + " on " + paths.GetHardAssetPath());
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
                AssetDatabase.AddObjectToAsset(mf.sharedMesh, paths.GetExtractAssetPath());
            }
        }
    }
}