using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
    public struct MapShadowsAssetPaths
    {
        string kg2Name;
        string genericPath;
        SHGame game;

        public MapShadowsAssetPaths(string hardAssetPath, SHGame forgame)
        {
            kg2Name = Path.GetFileNameWithoutExtension(hardAssetPath);
            genericPath = Path.GetDirectoryName(hardAssetPath).Substring(hardAssetPath.LastIndexOf("/data/data/") + 1).Replace("\\", "/") + "/";
            game = forgame;
        }

        public string GetHardAssetPath()
        {
            string path = CustomPostprocessor.GetHardDataPathFor(game);
            return path + genericPath + kg2Name + ".kg2";
        }

        public string GetExtractAssetPath()
        {
            string path = CustomPostprocessor.GetExtractDataPathFor(game);
            return path + genericPath + kg2Name + ".asset";
        }

        public string GetPrefabPath()
        {
            string path = CustomPostprocessor.GetExtractDataPathFor(game);
            return path + genericPath + "Prefabs/" + kg2Name + ".prefab";
        }
    }
    [Serializable]
	public class MapShadows : MonoBehaviour 
	{

        public static MapShadows ReadShadowCasters(MapShadowsAssetPaths paths)
        {
            GameObject subGO = Scene.BeginEditingPrefab(paths.GetPrefabPath(), "Shadows");

            try
            {
                MapShadows casters = subGO.AddComponent<MapShadows>();

                BinaryReader reader = new BinaryReader(new FileStream(paths.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));

                if (reader.BaseStream.Length != 0)
                {
                    //Master header
                    reader.SkipInt32();
                    short casterCount = reader.ReadInt16();
                    reader.SkipBytes(10, 0);
                    
                    //Reading casters
                    for (int i = 0; i != casterCount; i++)
                    {
                        //Caster header
                        reader.SkipInt32(0);
                        /*short index = */reader.ReadInt16();
                        short shapeCounts = reader.ReadInt16();
                        reader.SkipBytes(16, 0);
                        /*Vector3 mainPivot = */reader.ReadShortVector3();
                        /*short casterGroup = */reader.ReadInt16();
                        Matrix4x4 mainMatrix = reader.ReadMatrix4x4();
                        Vector3 debugPosition = Matrix4x4Utils.ExtractTranslationFromMatrix(ref mainMatrix);
                        Vector3 currentNormal = Vector3.zero;

                        //reading shapes
                        for (int j = 0; j != shapeCounts; j++)
                        {
                            short countOfPoints = reader.ReadInt16();
                            short UnknownS1 = reader.ReadInt16();
                            /*short UnknownS2 = */reader.ReadInt16();
                            reader.SkipInt16(countOfPoints);
                            /*Vector3 pivot = */reader.ReadShortVector3();
                            short shapeGroup = reader.ReadInt16();

                            List<Vector3> _verts = new List<Vector3>();
                            List<Vector3> _norms = new List<Vector3>();

                            for (int k = 0; k != countOfPoints; )
                            {
                                Vector3 v = reader.ReadShortVector3();
                                short flag = reader.ReadInt16();

                                if (flag == 0)
                                {
                                    currentNormal = Vector3.Normalize(v);
                                }
                                else
                                {
                                    _verts.Add(v + debugPosition);
                                    _norms.Add(currentNormal);
                                    k++;
                                }
                            }

                            Mesh mesh = null;
                            if (UnknownS1 == 6)
                            {
                                mesh = MeshUtils.MakeStripped(_verts, _norms, null, null, true);
                            }
                            else if (UnknownS1 == 5)
                            {
                                mesh = MeshUtils.MakeStrippedInverted(_verts, _norms);
                            }
                            else
                            {
                                mesh = MeshUtils.MakeSquare(_verts, _norms, null, null, true);
                            }

                            mesh.name = "shadowMesh_" + shapeGroup;
                            GameObject go = new GameObject("Shadow mesh");
                            go.transform.SetParent(subGO.transform);
                            go.AddComponent<MeshFilter>().sharedMesh = mesh;
                            MeshRenderer mr = go.AddComponent<MeshRenderer>();
                            mr.sharedMaterial = MaterialRolodex.defaultDiffuse;
                            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;


                            if (reader.BaseStream.Position % 16 != 0)
                            {
                                reader.SkipBytes(8, 0);
                            }
                        }
                    }
                }

                reader.Close();

                foreach (MeshFilter mf in subGO.GetComponentsInChildren<MeshFilter>())
                {
                    AssetDatabase.AddObjectToAsset(mf.sharedMesh, paths.GetExtractAssetPath());
                }

                Scene.FinishEditingPrefab(paths.GetPrefabPath(), subGO);

                return casters;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }
	}
}