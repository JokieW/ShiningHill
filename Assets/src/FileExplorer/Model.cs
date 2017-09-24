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
            LoadModel(new ModelAssetPaths("C:/Git/ShiningHill/Assets/sh3pc/data/pcchr/wp/rwp_fir.mdl", SHGame.SH3PC));
        }

        public static void LoadModel(ModelAssetPaths paths)
        {
            //GameObject subGO = Scene.BeginEditingPrefab(paths.GetPrefabPath(), "Model");
            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(new FileStream(paths.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));

                //00
                reader.SkipInt32(0);
                int unknown1 = reader.ReadInt32();
                int unknown2 = reader.ReadInt32();
                int unknown3 = reader.ReadInt32();

                //10
                reader.SkipInt32(unknown2);
                int unknown5 = reader.ReadInt32();
                reader.SkipInt32(0);
                reader.SkipInt32(1);

                //20 30
                reader.SkipBytes(32, 0);

                //40
                reader.SkipInt32(unknown5);
                reader.SkipInt32(unknown3);
                reader.SkipInt32(0);
                reader.SkipInt32(0);

                //blah blah
                reader.SkipBytes(128, 0);

                //D0
                Matrix4x4 matrix1 = reader.ReadMatrix4x4();

                //110
                Matrix4x4 matrix2 = reader.ReadMatrix4x4();

                reader.BaseStream.Position = 0x2d0;

                array = new TestS[237];
                for (int i = 0; i != 237; i++)
                {
                    array[i] = new TestS()
                    {
                        position = reader.ReadVector3(),
                        charmingman = reader.ReadVector3(),
                        unknow = reader.ReadSingle(),
                        normal = reader.ReadVector3(),
                        uv = reader.ReadVector2()
                    };
                }

                reader.Close();

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
            if (array != null)
            {
                for (int i = 0; i != 237; i++)
                {
                    TestS t = array[i];
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(t.position, 1);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(t.position, t.position + t.normal);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(t.position, t.position + t.charmingman);
                }
            }
        }

        static public TestS[] array;
    }

    

    public struct TestS
    {
        public Vector3 position;
        public Vector3 charmingman;
        public float unknow;
        public Vector3 normal;
        public Vector2 uv;
    }
}
