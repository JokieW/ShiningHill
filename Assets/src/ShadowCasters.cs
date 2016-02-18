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
	public class ShadowCasters : MonoBehaviour 
	{

        public List<Vector4> points = new List<Vector4>();

        public static ShadowCasters ReadShadowCasters(string path)
        {
            string prefabPath = path.Replace(".kg2", ".prefab");
            string assetPath = path.Replace(".kg2", ".asset");

            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            GameObject prefabGo = null;
            GameObject shadows = null;

            if(prefab == null)
            {
                prefabGo = new GameObject("Scene");
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
                ShadowCasters casters = shadows.AddComponent<ShadowCasters>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

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
                    Debug.Log("****** CASTER GROUP " + casterGroup + " ******");
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
                        Debug.Log("* SHAPE GROUP " + shapeGroup + " U1 " + UnknownS1 + " U2 " + UnknownS2 + "*");

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

            //Color debug

            //Gizmos.color = Color.red;
            //for (int i = 0; i != points.Count; i++)
            {
                //Vector3 p1 = transform.parent.TransformPoint(points[0]);
                /*Vector3 p2 = transform.parent.TransformPoint(points[i+1]);
                Vector3 p3 = transform.parent.TransformPoint(points[i+2]);
                Vector3 p4 = transform.parent.TransformPoint(points[i+3]);*/

                /*if (i < 10)
                    Gizmos.color = new Color(Gizmos.color.r + 0.1f, 0.0f, 0.0f);
                else
                    Gizmos.color = new Color(0.0f, Gizmos.color.g + 0.1f, 0.0f);*/

                //Gizmos.DrawCube(p1, new Vector3(0.01f, 0.01f, 0.05f));
                /*Gizmos.DrawSphere(p2, 0.005f);

                Gizmos.DrawLine(p1, p2);

                if (i + 3 != points.Count)
                {
                    Vector3 p3 = transform.parent.TransformPoint(points[i + 3]);
                    Gizmos.DrawSphere(p3, 0.005f);
                    Gizmos.DrawLine(p2, p3);
                }*/

                
                /*Gizmos.DrawSphere(p2, 0.02f);
                Gizmos.DrawSphere(p3, 0.02f);
                Gizmos.DrawSphere(p4, 0.02f);
                
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p3, p2);
                Gizmos.DrawLine(p3, p4);
                Gizmos.DrawLine(p1, p4);*/
            }

            
            Vector3 lastWeird = Vector3.zero;
            foreach(Vector4 v in points)
            {
                Vector3 p1 = transform.parent.TransformPoint(v);
                if(v.w ==  0)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(p1, 0.002f);
                    lastWeird = p1;
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(p1, 0.005f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(p1, p1 + (Vector3.Normalize(lastWeird) * 0.02f));
                }
            }
            /*for (int i = 0; i != weirds.Count; i++)
            {
                Vector3 p1 = transform.parent.TransformPoint(weirds[i]);
                /*Vector3 p2 = transform.parent.TransformPoint(points[i+1]);
                Vector3 p3 = transform.parent.TransformPoint(points[i+2]);
                Vector3 p4 = transform.parent.TransformPoint(points[i+3]);*/

                /*if (i < 10)
                    Gizmos.color = new Color(Gizmos.color.r + 0.1f, 0.0f, 0.0f);
                else
                    Gizmos.color = new Color(0.0f, Gizmos.color.g + 0.1f, 0.0f);*/

                //Gizmos.DrawCube(p1, new Vector3(1.05f, 1.01f, 1.01f));
                /*Gizmos.DrawSphere(p2, 0.005f);

                Gizmos.DrawLine(p1, p2);

                if (i + 3 != points.Count)
                {
                    Vector3 p3 = transform.parent.TransformPoint(points[i + 3]);
                    Gizmos.DrawSphere(p3, 0.005f);
                    Gizmos.DrawLine(p2, p3);
                }*/


                /*Gizmos.DrawSphere(p2, 0.02f);
                Gizmos.DrawSphere(p3, 0.02f);
                Gizmos.DrawSphere(p4, 0.02f);
                
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p3, p2);
                Gizmos.DrawLine(p3, p4);
                Gizmos.DrawLine(p1, p4);*/
            //}

        }
	}
}