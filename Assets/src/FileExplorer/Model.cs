using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            genericPath = Path.GetDirectoryName(hardAssetPath).Substring(hardAssetPath.LastIndexOf("/data/") + 1).Replace("\\", "/") + "/";
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
        [ContextMenu("Load")]
        public void Doit()
        {
            LoadModel(new ModelAssetPaths(path, SHGame.SH3PC));
        }

        public string path = "Assets/sh3pc/data/pcchr/wp/rwp_fir.mdl";

        public static void LoadModel(ModelAssetPaths paths)
        {
            //GameObject subGO = Scene.BeginEditingPrefab(paths.GetPrefabPath(), "Model");
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
                int matricesOffset = reader.ReadInt32();
                int matricesCount = reader.ReadInt32();

                int offsetToAfterMatrices = reader.ReadInt32();
                reader.SkipInt32(0);
                reader.SkipInt32(offsetToAfterMatrices + 0x10);
                reader.SkipInt32(offsetToAfterMatrices + 0x10);

                int meshCount = reader.ReadInt32();
                int offsetMesh = reader.ReadInt32();
                reader.SkipInt32(0);
                reader.SkipInt32(textureOffset);

                int unknown2 = reader.ReadInt32(); // texture to use?
                reader.SkipInt32(offsetToAfterMatrices + 0x10);
                reader.SkipInt32(1);
                reader.SkipInt32(offsetToAfterMatrices + 0x20);

                reader.SkipInt32(0);
                reader.SkipInt32(0);
                reader.SkipInt32(offsetToAfterMatrices + 0x30);
                reader.SkipInt32(0);

                reader.SkipInt32(offsetToAfterMatrices + 0x30);
                reader.SkipInt32(0);
                reader.SkipInt32(offsetToAfterMatrices + 0x30);
                reader.SkipInt32(offsetToAfterMatrices + 0x30);

                reader.SkipInt32(offsetToAfterMatrices + 0x40);
                reader.SkipInt32(1);
                reader.SkipInt32(0);
                reader.SkipInt32(0);

                reader.SkipBytes(16, 0);

                Matrix4x4[] modelMatrices = new Matrix4x4[matricesCount];
                for (int i = 0; i != matricesCount; i++)
                {
                    modelMatrices[i] = reader.ReadMatrix4x4();
                }
                testdata = modelMatrices;

                reader.SkipInt32(0xFF);
                reader.SkipBytes(12, 0);

                reader.SkipBytes(176, 0);

                reader.BaseStream.Position = textureOffset;
                Texture2D[] textures = TextureUtils.ReadTex32(paths.GetTextureName(), reader);

                Mesh[] meshes = new Mesh[2];
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
                    int meshID = reader.ReadInt32(); //mesh id?

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
                    Vector2 unknown4 = reader.ReadVector2();

                    Matrix4x4 meshMatrix = reader.ReadMatrix4x4();

                    reader.SkipInt32();
                    reader.SkipBytes(12, 0);

                    reader.SkipInt32();
                    reader.SkipInt32(0x1fc);
                    reader.SkipInt32(0x61);
                    reader.SkipInt32(0x0);

                    List<Vector3> verts = new List<Vector3>(valuesCount);
                    List<Vector3> rots = new List<Vector3>(valuesCount);
                    List<byte> matIds = new List<byte>(valuesCount);
                    List<Vector3> norms = new List<Vector3>(valuesCount);
                    List<Vector2> uvs = new List<Vector2>(valuesCount);
                    for (int i = 0; i != valuesCount; i++)
                    {
                        verts.Add(reader.ReadVector3());
                        rots.Add(reader.ReadVector3());
                        byte b1 = reader.ReadByte();
                        matIds.Add(reader.ReadByte());
                        reader.SkipByte(b1);
                        reader.SkipByte(b1);
                        norms.Add(-reader.ReadVector3());
                        uvs.Add(reader.ReadVector2());
                    }

                    for (int i = 0; i != valuesCount; i++)
                    {
                        Matrix4x4 mat = modelMatrices[matIds[i]];
                        verts[i] = mat.MultiplyPoint(verts[i]);
                        rots[i] = mat.MultiplyPoint(rots[i]);
                        norms[i] = mat.MultiplyPoint(norms[i]);
                    }

                    reader.BaseStream.Position = indicesOffset + meshOrigin;
                    List<short> indices = new List<short>(indicesCount);
                    for (int i = 0; i != indicesCount; i++)
                    {
                        indices.Add((short)reader.ReadInt32());
                    }

                    meshes[meshi] = MeshUtils.MakeIndexedStripInverted(verts, indices, norms, uvs);
                }
                reader.Close();

                GameObject go = new GameObject("gun");

                for (int i = 0; i != meshCount; i++)
                {
                    GameObject mesh = new GameObject();
                    MeshRenderer mr = mesh.AddComponent<MeshRenderer>();
                    MeshFilter mf = mesh.AddComponent<MeshFilter>();
                    mf.mesh = meshes[i];
                    mr.material.mainTexture = textures[0];
                    mesh.transform.parent = go.transform;
                }


                //Scene.FinishEditingPrefab(paths.GetPrefabPath(), subGO);
                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (reader != null) reader.Close();
                //Scene.FinishEditingPrefab(paths.GetPrefabPath(), subGO);
            }
        }

        void OnDrawGizmos()
        {
            if (testdata != null)
            {
                for (int i = 0; i != testdata.Length; i++)
                {
                    Matrix4x4 t = testdata[i];
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
        }

        static public Matrix4x4[] testdata;
    }

    

    public struct ModelMeshValue
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 normal;
        public Vector2 uv;
    }
}
