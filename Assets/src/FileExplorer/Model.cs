using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ShiningHill
{
    public struct ModelAssetPaths
    {
        string mdlName;
        string genericPath;
        SHGame game;

        public ModelAssetPaths(string hardAssetPath, SHGame forgame)
        {
            mdlName = Path.GetFileNameWithoutExtension(hardAssetPath);
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
            return mdlName + "_tex";
        }

        public string GetHardAssetPath()
        {
            string path = CustomPostprocessor.GetHardDataPathFor(game);
            return path + genericPath + mdlName + ".mdl";
        }

        public string GetExtractAssetPath()
        {
            string path = CustomPostprocessor.GetExtractDataPathFor(game);
            return path + genericPath + mdlName + ".asset";
        }

        public string GetPrefabPath()
        {
            string path = CustomPostprocessor.GetExtractDataPathFor(game);
            return path + genericPath + "Prefabs/" + mdlName + ".prefab";
        }
    }

    public class Model : MonoBehaviour
    {
        public Matrix4x4[] matrices;
        public List<Vector3> rots;

        public static void LoadModel(ModelAssetPaths paths)
        {
            GameObject subGO = Scene.BeginEditingPrefab(paths.GetPrefabPath(), "Model");

            Model model = subGO.AddComponent<Model>();

            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(new FileStream(paths.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));

                //Main header
                reader.SkipInt32(0);
                int unknown1 = reader.ReadInt32();
                int textureCount = reader.ReadInt32();
                int textureOffset = reader.ReadInt32();
                
                reader.SkipInt32(textureCount);
                int offsetToModel = reader.ReadInt32();
                reader.SkipInt32(0);
                reader.SkipInt32(1); //model count?
                
                reader.SkipBytes(32, 0);
                
                reader.SkipInt32(offsetToModel);
                reader.SkipInt32(textureOffset);
                reader.SkipInt32(0);
                reader.SkipInt32(0);

                //Model header
                long modelOrigin = reader.BaseStream.Position;
                reader.SkipUInt32(0xffff0003);
                reader.SkipInt32(3);
                int staticMatricesOffset = reader.ReadInt32();
                int staticMatricesCount = reader.ReadInt32();

                int offsetToStaticMatLinkers = reader.ReadInt32();
                int extraMatricesCount = reader.ReadInt32();
                int offsetToExtraMatLinkers = reader.ReadInt32();
                int offsetToExtraMatrices = reader.ReadInt32();

                int meshCount = reader.ReadInt32();
                int offsetMesh = reader.ReadInt32();
                reader.SkipInt32(0);
                reader.SkipInt32(textureOffset);

                int unknown2 = reader.ReadInt32(); // texture to use?
                int offsetAfterExtraMatrices = reader.ReadInt32();
                reader.SkipInt32(1);
                reader.SkipInt32(offsetAfterExtraMatrices);

                reader.SkipInt32(0);
                reader.SkipInt32(0);
                reader.SkipInt32(offsetAfterExtraMatrices + 0x10);
                reader.SkipInt32(0);

                reader.SkipInt32(offsetAfterExtraMatrices + 0x10);
                reader.SkipInt32(0);
                reader.SkipInt32(offsetAfterExtraMatrices + 0x10);
                reader.SkipInt32(offsetAfterExtraMatrices + 0x10);

                reader.SkipInt32(offsetAfterExtraMatrices + 0x20);
                reader.SkipInt32(1);
                reader.SkipInt32(0);
                reader.SkipInt32(0);

                reader.SkipBytes(16, 0);

                reader.BaseStream.Position = modelOrigin + staticMatricesOffset;
                Matrix4x4[] modelMatrices = new Matrix4x4[staticMatricesCount + extraMatricesCount];
                for (int i = 0; i != staticMatricesCount; i++)
                {
                    modelMatrices[i] = reader.ReadMatrix4x4();
                }
                model.matrices = modelMatrices;
                
                reader.BaseStream.Position = modelOrigin + offsetToStaticMatLinkers;
                byte[] staticMatricesLinks = new byte[staticMatricesCount];
                for (int i = 0; i != staticMatricesLinks.Length; i++)
                {
                    byte b = reader.ReadByte();
                    staticMatricesLinks[i] = b;
                    if (b != 0xFF)
                    {
                        //modelMatrices[i] = modelMatrices[b] * modelMatrices[i];
                    }
                }
                
                reader.BaseStream.Position = modelOrigin + offsetToExtraMatLinkers;
                byte[] extraMatricesLinks = new byte[extraMatricesCount*2];
                for (int i = 0; i != extraMatricesLinks.Length; i+=2)
                {
                    byte b1 = reader.ReadByte();
                    byte b2 = reader.ReadByte();
                    extraMatricesLinks[i] = b1;
                    extraMatricesLinks[i+1] = b2;
                }
                
                reader.BaseStream.Position = modelOrigin + offsetToExtraMatrices;
                for (int i = 0; i != extraMatricesCount; i++)
                {
                    Matrix4x4 mat = reader.ReadMatrix4x4();
                    byte b1 = extraMatricesLinks[(i * 2)];
                    byte b2 = extraMatricesLinks[(i * 2) + 1];
                    if (b2 != 0xFF)
                    {
                        mat = /*modelMatrices[b1] * */modelMatrices[b2] * mat;
                    }
                    modelMatrices[staticMatricesCount + i] = mat;
                }
                
                reader.SkipBytes(176, 0);

                reader.BaseStream.Position = textureOffset;
                Texture2D[] textures = TextureUtils.ReadTex32(paths.GetTextureName(), reader);

                Mesh[] meshes = new Mesh[meshCount];
                int[] textureIDs = new int[meshCount]; 
                reader.BaseStream.Position = modelOrigin + offsetMesh;
                //Mesh Header
                for (int meshi = 0; meshi != meshCount; meshi++)
                {
                    long meshOrigin = reader.BaseStream.Position;
                    int length = reader.ReadInt32();
                    reader.SkipInt32();
                    int valuesOffset = reader.ReadInt32();
                    reader.SkipInt32();

                    int indicesCount = reader.ReadInt32();
                    reader.SkipInt32();
                    int unknown3 = reader.ReadInt32();
                    int meshFormat = reader.ReadInt32(); // 1 = 1 matrix, 2 = multiple matrix ?

                    reader.SkipInt32(unknown3);
                    reader.SkipInt32(0);
                    reader.SkipInt32();
                    reader.SkipInt32();

                    reader.SkipInt32();
                    reader.SkipInt32(1);
                    reader.SkipInt32();
                    reader.SkipInt32(0xB0);
                    
                    reader.SkipInt32(valuesOffset);
                    int valuesCount = reader.ReadInt32();
                    int indicesOffset = reader.ReadInt32();
                    reader.SkipInt32(indicesCount);

                    short textureKind = reader.ReadInt16(); //?
                    short textureKind2 = reader.ReadInt16(); //?
                    int textureId = reader.ReadInt32(); //? 
                    textureIDs[meshi] = textureId;
                    Vector2 unknown4 = reader.ReadVector2();

                    Matrix4x4 meshMatrix = reader.ReadMatrix4x4();

                    reader.SkipInt32();
                    reader.SkipBytes(12, 0);

                    reader.SkipInt32();
                    reader.SkipInt32(0x1fc);
                    reader.SkipInt32(0x61);
                    reader.SkipInt32(0x0);

                    reader.BaseStream.Position = meshOrigin + valuesOffset;
                    List<Vector3> verts = new List<Vector3>(valuesCount);
                    List<Vector3> norms = new List<Vector3>(valuesCount);
                    List<Vector2> uvs = new List<Vector2>(valuesCount);
                    if (meshFormat == 1)
                    {
                        Matrix4x4 mat = modelMatrices[0];
                        for (int i = 0; i != valuesCount; i++)
                        {
                            verts.Add(mat.MultiplyPoint(reader.ReadVector3()));
                            norms.Add(reader.ReadVector3());
                            uvs.Add(reader.ReadVector2());
                        }
                    }
                    else
                    {
                        List<Vector3> rots = new List<Vector3>(valuesCount);
                        List<byte> matIds = new List<byte>(valuesCount);
                        for (int i = 0; i != valuesCount; i++)
                        {
                            verts.Add(reader.ReadVector3());
                            rots.Add(reader.ReadVector3());
                            byte b1 = reader.ReadByte();
                            matIds.Add(reader.ReadByte());
                            reader.SkipByte(b1);
                            reader.SkipByte(b1);
                            norms.Add(reader.ReadVector3());
                            uvs.Add(reader.ReadVector2());
                        }
                        model.rots = rots;
                        for (int i = 0; i != valuesCount; i++)
                        {
                            int id = matIds[i];
                            verts[i] = modelMatrices[id].MultiplyPoint(verts[i]);
                        }
                    }

                    reader.BaseStream.Position = meshOrigin + indicesOffset;
                    List<short> indices = new List<short>(indicesCount);
                    for (int i = 0; i != indicesCount; i++)
                    {
                        indices.Add((short)reader.ReadInt32());
                    }

                    meshes[meshi] = MeshUtils.MakeIndexedStripInverted(verts, indices, norms, uvs);
                }
                reader.Close();

                MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(paths.GetExtractAssetPath());
                rolodex.AddTextures(textures);

                for (int i = 0; i != meshCount; i++)
                {
                    GameObject mesh = new GameObject();
                    MeshRenderer mr = mesh.AddComponent<MeshRenderer>();
                    MeshFilter mf = mesh.AddComponent<MeshFilter>();
                    meshes[i].name = "mesh_" + i;
                    mf.sharedMesh = meshes[i];
                    
                    MaterialRolodex.TexMatsPair tmp = rolodex.GetWithSH3Index(textureIDs[i], 1);

                    mr.sharedMaterial = tmp.GetOrCreateDiffuse();
                    mesh.transform.parent = subGO.transform;
                }

                foreach (MeshFilter mf in subGO.GetComponentsInChildren<MeshFilter>())
                {
                    AssetDatabase.AddObjectToAsset(mf.sharedMesh, paths.GetExtractAssetPath());
                }


                Scene.FinishEditingPrefab(paths.GetPrefabPath(), subGO);
                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (subGO != null) Scene.FinishEditingPrefab(paths.GetPrefabPath(), subGO);
            }
        }

        void OnDrawGizmos()
        {
            if (matrices != null)
            {
                for (int i = 0; i != matrices.Length; i++)
                {
                    Matrix4x4 t = matrices[i];
                    Gizmos.color = Color.yellow;
                    Vector3 pos = Matrix4x4Utils.ExtractTranslationFromMatrix(ref t);
                    Quaternion rot = Matrix4x4Utils.ExtractRotationFromMatrix(ref t);
                    Gizmos.DrawSphere(pos, 0.2f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(pos, pos + (rot * Vector3.forward));
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(pos, pos + (rot * Vector3.up));
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(pos, pos + (rot * Vector3.right));
                }
            }
            if (rots != null)
            {
                for (int i = 0; i != rots.Count; i++)
                {
                    Gizmos.color = Color.Lerp(Color.black, Color.white, (float)i / (float)rots.Count);
                    Gizmos.DrawSphere(rots[i], 0.02f);
                }
            }
            foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
            {
                Mesh m = mf.sharedMesh;
                Vector3[] verts = m.vertices;
                Vector3[] norms = m.normals;
                if (m != null)
                {
                    for (int i = 0; i != verts.Length; i++)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(verts[i], verts[i] + new Vector3(norms[i].x, 0.0f, 0.0f).normalized);
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(verts[i], verts[i] + new Vector3(0.0f, norms[i].y, 0.0f).normalized);
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(verts[i], verts[i] + new Vector3(0.0f, 0.0f, norms[i].z).normalized);
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(verts[i], verts[i] + (norms[i] * 0.5f));
                        Gizmos.color = Color.Lerp(Color.black, Color.white, (float)i / (float)verts.Length);
                        Gizmos.DrawSphere(verts[i] + rots[i], 0.02f);
                    }
                }
            }
        }
    }

    

    public struct ModelMeshValue
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 normal;
        public Vector2 uv;
    }
}
