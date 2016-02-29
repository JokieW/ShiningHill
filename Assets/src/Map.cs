using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Eppy;

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
            
            GameObject subGO = Scene.BeginEditingPrefab(path, "Map");
            

            try
            {
                Map scene = subGO.AddComponent<Map>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

                //Header
                int marker = reader.PeekInt32(); //marker

                if (marker != -1) //SH2
                {
                    ReadSH2Map(reader, scene, path);
                    Scene.FinishEditingPrefab(path, subGO);
                }
                else if (marker == -1) //SH3
                {
                    ReadSH3Map(reader, scene, path);
                    Scene.FinishEditingPrefab(path, subGO);
                }

                return scene;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        static void ReadSH2Map(BinaryReader reader, Map scene, string path)
        {
            GameObject subGO = scene.gameObject;
            string assetPath = path.Replace(".map", ".asset");

            int fileID = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            int Unknown1 = reader.ReadInt32();
            reader.SkipInt32(0);

            //Textures
            Texture[] textures = TextureUtils.ReadDDS(Path.GetFileName(path).Replace(".map", "_tex"), reader);


            //Meshes
            reader.SkipInt32(1);
            reader.SkipInt32(); //Length from magic to bottom
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            long magicPosition = reader.BaseStream.Position;
            reader.SkipInt32(0x20010730); //Magic number?
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
            reader.ReadVector3YInverted(); //V1
            reader.SkipInt32(0);
            reader.ReadVector3YInverted(); //V2
            reader.SkipInt32(0);
            int headerLength = reader.ReadInt32(); //From v1 to vertexLength

            int indicesOffset = reader.ReadInt32() + (int)v1offset;
            int indicesLength = reader.ReadInt32();
            int Unknown = reader.ReadInt32();
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
                verts.Add(reader.ReadVector3YInverted() * Scene.GLOBAL_SCALE);
                norms.Add(reader.ReadVector3YInverted());
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
            MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(assetPath);
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
                AssetDatabase.AddObjectToAsset(mf.sharedMesh, assetPath);
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

        static void ReadSH3Map(BinaryReader reader, Map scene, string path)
        {
            GameObject subGO = scene.gameObject;
            string assetPath = path.Replace(".map", ".asset");

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
                    goodRolodex = MaterialRolodex.GetOrCreateAt(path.Replace(name, name.Substring(0, 2) + "GB.tex"));
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
        }
    }
}