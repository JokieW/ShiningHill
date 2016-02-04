using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{
	public class MeshPart : MonoBehaviour 
	{
        public int NextSceneGeoOffset;
        public int HeaderLength; //64
        public int Length;
        public int Unknown1;

        public int VertexCount;
        public int ObjectType;
        public string OcclusionGroup;
        public int Unknown3;

        public static MeshPart Deserialise(BinaryReader reader, GameObject parent, string path)
        {
            GameObject go = new GameObject("Mesh Part");
            MeshPart part = go.AddComponent<MeshPart>();
            part.transform.SetParent(parent.transform);

            long offset = reader.BaseStream.Position;

            part.NextSceneGeoOffset = reader.ReadInt32();
            part.HeaderLength = reader.ReadInt32();
            part.Length = reader.ReadInt32();
            part.Unknown1 = reader.ReadInt32();

            part.VertexCount = reader.ReadInt32();
            part.ObjectType = reader.ReadInt32(); //1 = static, 2 = can be or not there, 3 = can move
            int val = reader.ReadInt32();
            part.OcclusionGroup = "0x" + val.ToString("X") + " 0b" + Convert.ToString(val, 2);
            part.Unknown3 = reader.ReadInt32();

            for (int i = 0; i != 8; i++)
            {
                int value = reader.ReadInt32();
                if(value != 0)
                {
                    Debug.LogWarning("Unexpected number in mesh part "+offset.ToString()+" : "+value);
                }
            }

            go.isStatic = part.ObjectType != 3;

            List<Vector3> _verts = new List<Vector3>();
            List<Vector3> _norms = new List<Vector3>();
            List<Vector2> _uvs = new List<Vector2>();
            List<Color32> _colors = new List<Color32>();
            for (int i = 0; i != part.VertexCount; i++)
            {
                Vector3 temp = reader.ReadVector3();
                temp.y = -temp.y;
                _verts.Add(temp);

                temp = reader.ReadVector3();
                temp.x = -temp.x;
                temp.z = -temp.z;
                _norms.Add(temp);

                _uvs.Add(reader.ReadVector2());
                _colors.Add(reader.ReadColor32());
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(_verts);
            mesh.SetNormals(_norms);
            mesh.SetUVs(0, _uvs);
            mesh.SetColors(_colors);

            List<int> _tris = new List<int>();
            for (int i = 1; i < part.VertexCount-1; i += 2)
            {
                _tris.Add(i);
                _tris.Add(i - 1);
                _tris.Add(i + 1);

                if (i + 2 < part.VertexCount)
                {
                    _tris.Add(i);
                    _tris.Add(i + 1);
                    _tris.Add(i + 2);
                }
            }

            mesh.SetTriangles(_tris, 0);

            AssetDatabase.CreateAsset(mesh, path + "/" + offset.ToString() + ".asset");

            go.AddComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath<Mesh>(path + "/" + offset.ToString()+".asset");
            go.AddComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DefaultMaterial.mat");

            return part;
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

            /*Gizmos.color = _gizmoColor;
            foreach (Vector3 v3 in GetComponent<MeshFilter>().mesh.vertices)
            {
                Gizmos.DrawSphere(v3, 10.0f);
            }*/

        }


	}
}