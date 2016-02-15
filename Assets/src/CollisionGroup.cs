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
	public class CollisionGroup : MonoBehaviour 
	{
        [SerializeField]
        public List<CollisionPane> panes = new List<CollisionPane>();
        public int DispalyKind = -1;

		public static CollisionGroup ReadCollisions(string path)
        {
            string prefabPath = path.Replace(".cld", ".prefab");
            string assetPath = path.Replace(".cld", ".asset");

            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            GameObject prefabGo = null;
            GameObject collisions = null;

            if(prefab == null)
            {
                prefabGo = new GameObject("Scene");
                prefabGo.isStatic = true;
            }
            else
            {
                prefabGo = (GameObject)GameObject.Instantiate(prefab);
                PrefabUtility.DisconnectPrefabInstance(prefabGo);
                Transform existingCollisions = prefabGo.transform.FindChild("Collisions");
                if (existingCollisions != null)
                {
                    DestroyImmediate(existingCollisions.gameObject);
                }
            }

            prefabGo.transform.localScale = Vector3.one;
            collisions = new GameObject("Collisions");
            collisions.transform.SetParent(prefabGo.transform);
            collisions.isStatic = true;

            try
            {
                CollisionGroup cols = collisions.AddComponent<CollisionGroup>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

                reader.SkipInt32(1198153728);
                reader.SkipInt32(1208530944);
                reader.SkipInt32(160);
                reader.SkipInt32(880);
                reader.SkipInt32(160);
                reader.SkipInt32(80);
                int vertexCount = reader.ReadInt32();
                reader.SkipInt32(0);

                List<int> offsets = new List<int>();
                int offset;
                while ((offset = reader.ReadInt32()) != 0 && offset != -1)
                {
                    offsets.Add(offset);
                }

                foreach (int off in offsets)
                {
                    reader.BaseStream.Position = off;
                    while (true)
                    {
                        int peek = reader.PeekInt32();
                        if (peek == 0 || peek == -1)
                        {
                            break;
                        }

                        cols.panes.Add(new CollisionPane(reader));
                    }
                }


                reader.Close();

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

                return cols;

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

            
            foreach (CollisionPane pane in panes)
            {
                Color c;
                if (pane.Kind == 0)
                {
                    c = Color.green;
                }
                else if (pane.Kind == 1)
                {
                    c = Color.blue;
                }
                else if (pane.Kind == 32)
                {
                    c = Color.magenta;
                }
                else if (pane.Kind == 128)
                {
                    c = Color.cyan;
                }
                else if (pane.Kind == 256)
                {
                    c = Color.red;
                }
                else if (pane.Kind == 512)
                {
                    c = Color.black;
                }
                else if (pane.Kind == 1024)
                {
                    c = Color.grey;
                }
                else if (pane.Kind == 2048)
                {
                    c = Color.white;
                }
                else if (pane.Kind == 4096)
                {
                    c = Color.yellow;
                }
                else if (pane.Kind == 8192)
                {
                    c = Color.magenta;
                }
                else if (pane.Kind == 16384)
                {
                    c = Color.green;
                }
                else if (pane.Kind == 32768)
                {
                    c = Color.black;
                }
                else if (pane.Kind == 131072)
                {
                    c = Color.red;
                }
                else
                {
                    c = Color.white;
                    //Debug.LogWarning("Unknown pane kind " + pane.Kind);
                }
                Gizmos.color = c;

                Vector3 p1 = transform.parent.TransformPoint(pane.vectors[0]);
                Vector3 p2 = transform.parent.TransformPoint(pane.vectors[1]);
                Vector3 p3 = transform.parent.TransformPoint(pane.vectors[2]);
                Vector3 p4 = transform.parent.TransformPoint(pane.vectors[3]);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p3, p2);

                if (pane.Unknown1 == 1)
                {
                    Gizmos.DrawLine(p3, p1);
                }
                else
                {
                    Gizmos.DrawLine(p3, p4);
                    Gizmos.DrawLine(p1, p4);
                }
            }

        }

        [Serializable]
        public class CollisionPane
        {
            [SerializeField]
            public int Unknown1;
            [SerializeField]
            public int Kind;
            [SerializeField]
            public Vector3[] vectors;

            public CollisionPane(BinaryReader reader)
            {
                Unknown1 = reader.ReadInt32();
                int vectorsToRead = reader.ReadInt32();
                Kind = reader.ReadInt32();
                reader.SkipInt32(0);

                List<Vector3> v3s = new List<Vector3>();
                for (int i = 0; i != vectorsToRead; i++)
                {
                    v3s.Add(reader.ReadVector3YInverted());
                    reader.SkipSingle(1.0f);
                }
                vectors = v3s.ToArray();
            }
        }
	}
}