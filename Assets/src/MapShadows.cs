using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
    [Serializable]
	public class MapShadows : MonoBehaviour 
	{

        public static MapShadows ReadShadowCasters(string path)
        {
            string prefabPath = path.Replace(".kg2", ".prefab");
            string assetPath = path.Replace(".kg2", ".asset");

            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            GameObject prefabGo = null;
            GameObject shadows = null;

            if(prefab == null)
            {
                prefabGo = new GameObject("Area");
                prefabGo.isStatic = true;
            }
            else
            {
                prefabGo = (GameObject)GameObject.Instantiate(prefab);
                PrefabUtility.DisconnectPrefabInstance(prefabGo);
                Transform existingShadows = prefabGo.transform.FindChild("Shadows");
                if (existingShadows != null)
                {
                    DestroyImmediate(existingShadows.gameObject);
                }
            }

            prefabGo.transform.localScale = Vector3.one;
            shadows = new GameObject("Shadows");
            shadows.transform.SetParent(prefabGo.transform);
            shadows.isStatic = true;

            try
            {
                MapShadows casters = shadows.AddComponent<MapShadows>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

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
                        short index = reader.ReadInt16();
                        short shapeCounts = reader.ReadInt16();
                        reader.SkipBytes(16, 0);
                        Vector3 mainPivot = reader.ReadShortVector3();
                        short casterGroup = reader.ReadInt16();
                        Matrix4x4 mainMatrix = reader.ReadMatrix4x4();
                        Vector3 debugPosition = Matrix4x4Utils.ExtractTranslationFromMatrix(ref mainMatrix);
                        debugPosition.y = -debugPosition.y;
                        Vector3 currentNormal = Vector3.zero;

                        //reading shapes
                        for (int j = 0; j != shapeCounts; j++)
                        {
                            short countOfPoints = reader.ReadInt16();
                            short UnknownS1 = reader.ReadInt16();
                            short UnknownS2 = reader.ReadInt16();
                            reader.SkipInt16(countOfPoints);
                            Vector3 pivot = reader.ReadShortVector3();
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
                            go.transform.SetParent(shadows.transform);
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

                foreach (MeshFilter mf in shadows.GetComponentsInChildren<MeshFilter>())
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