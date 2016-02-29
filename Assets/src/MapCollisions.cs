using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
    [Serializable]
	public class MapCollisions : MonoBehaviour 
	{
        [SerializeField]
        public List<CollisionPane> panes = new List<CollisionPane>();
        public int DispalyKind = -1;

		public static MapCollisions ReadCollisions(string path)
        {
            string assetPath = path.Replace(".cld", ".asset");
            GameObject subGO = Scene.BeginEditingPrefab(path, "Collisions");

            try
            {
                MapCollisions cols = subGO.AddComponent<MapCollisions>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

                Vector2 origin = reader.ReadVector2();
                reader.SkipInt32(160);
                reader.SkipInt32(880);
                reader.SkipInt32(160);
                reader.SkipInt32(80);
                /*int vertexCount = */reader.ReadInt32();
                reader.SkipInt32(0);

                List<int> offsets = new List<int>();
                
                for (int i = 0; i != 85; i++)
                {
                    offsets.Add(reader.ReadInt32());
                }
                /*int offset;
                while ((offset = reader.ReadInt32()) != 0 && offset != -1)
                {
                    offsets.Add(offset);
                }*/

                Matrix4x4 transMat = cols.GetComponentInParent<Scene>().GetSH3ToUnityMatrix();
                foreach (int off in offsets)
                {
                    reader.BaseStream.Position = off - 4;
                    int peek = reader.ReadInt32();
                    if (peek == 0)
                    {
                        while (true)
                        {
                            peek = reader.PeekInt32();
                            if (peek != 0)
                            {
                                cols.panes.Add(new CollisionPane(reader, transMat));
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                int[] groups = cols.panes.Select(x => x.group).Distinct().ToArray();
                foreach (int group in groups)
                {
                    List<MeshCombineUtility.MeshInstance> meshes = new List<MeshCombineUtility.MeshInstance>();
                    GameObject colGO = new GameObject("Collision " + group);
                    colGO.isStatic = true;
                    foreach (CollisionPane pane in cols.panes.Where(x => x.group == group))
                    {
                        if (pane.type == 257)
                        {
                            MeshCombineUtility.MeshInstance inst = new MeshCombineUtility.MeshInstance();
                            inst.mesh = MeshUtils.MakeSquareInverted(pane.vectors.ToList());
                            inst.subMeshIndex = 0;
                            inst.transform = Matrix4x4.identity;
                            meshes.Add(inst);
                            
                        }
                        else if (pane.type == 1)
                        {
                            MeshCombineUtility.MeshInstance inst = new MeshCombineUtility.MeshInstance();
                            inst.mesh = MeshUtils.MakeStrippedInverted(pane.vectors.ToList());
                            inst.subMeshIndex = 0;
                            inst.transform = Matrix4x4.identity;
                            meshes.Add(inst);
                        }
                        else if (pane.type == 769)
                        {
                            CapsuleCollider cc = colGO.AddComponent<CapsuleCollider>();
                            cc.center = pane.vectors[0] + (pane.offset * 0.5f);
                            cc.radius = pane.radius;
                            cc.height = pane.offset.y;
                            cc.direction = 1;
                        }
                    }

                    Mesh mesh = MeshCombineUtility.Combine(meshes.ToArray(), false);
                    mesh.RecalculateNormals();
                    mesh.name = "collisionMesh_" + group;
                    MeshCollider mc = colGO.AddComponent<MeshCollider>();
                    mc.sharedMesh = mesh;
                    colGO.transform.SetParent(subGO.transform);
                }

                reader.Close();

                foreach (MeshCollider mc in subGO.GetComponentsInChildren<MeshCollider>())
                {
                    AssetDatabase.AddObjectToAsset(mc.sharedMesh, assetPath);
                }

                Scene.FinishEditingPrefab(path, subGO);

                return cols;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        [Serializable]
        public class CollisionPane
        {
            [SerializeField]
            public int type;
            [SerializeField]
            public int group;
            [SerializeField]
            public Vector3[] vectors;
            [SerializeField]
            public Vector3 offset;
            [SerializeField]
            public float radius;

            public CollisionPane(BinaryReader reader, Matrix4x4 transMat)
            {
                type = reader.ReadInt32();
                int vectorsToRead = reader.ReadInt32(); //Not neccessarely
                group = reader.ReadInt32(); //More like group
                reader.SkipInt32(0);

                if (type == 769)
                {
                    Vector3 vertex = reader.ReadVector3YInverted();
                    vectors = new Vector3[] { transMat.MultiplyPoint(vertex) };
                    reader.SkipSingle(1.0f);
                    offset = transMat.MultiplyPoint(reader.ReadVector3YInverted());
                    radius = reader.ReadSingle() * Scene.GLOBAL_SCALE;
                }
                else if (type == 257 || type == 1)
                {
                    List<Vector3> v3s = new List<Vector3>();
                    for (int i = 0; i != vectorsToRead; i++)
                    {
                        if (type == 1 && i != 0 && i % 3 == 0)
                        {
                            reader.SkipBytes(16);
                            continue;
                        }
                        Vector3 vertex = reader.ReadVector3YInverted();
                        v3s.Add(transMat.MultiplyPoint(vertex));
                        reader.SkipSingle(1.0f);
                    }
                    vectors = v3s.ToArray();
                    offset = Vector3.zero;
                    radius = 0.0f;
                }
                else
                {
                    Debug.LogWarning("UNHANDLED COLLIDER TYPE " + type + " FIX IT");
                }
            }
        }
	}
}