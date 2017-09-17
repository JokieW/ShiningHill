﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{
	public class MeshPart : MonoBehaviour 
	{
        public int ObjectType;
        public string OcclusionGroup;
        public int MeshFlags; // 60 is normal, 124 is decal

        public static int Deserialise(BinaryReader reader, GameObject parent)
        {
            GameObject go = new GameObject("Mesh Part");
            MeshPart part = go.AddComponent<MeshPart>();
            part.transform.SetParent(parent.transform);

            long offset = reader.BaseStream.Position;

            int NextOffset = reader.ReadInt32();
            reader.SkipInt32(64);
            reader.SkipInt32();//Length 
            reader.SkipInt32(0);

            int VertexCount = reader.ReadInt32();
            part.ObjectType = reader.ReadInt32(); //1 = static, 2 = can be or not there, 3 = can move
            int val = reader.ReadInt32();
            part.OcclusionGroup = "0x" + val.ToString("X") + " 0b" + Convert.ToString(val, 2);
            part.MeshFlags = reader.ReadInt32();

            reader.SkipBytes(32, 0);

            go.isStatic = part.ObjectType != 3;

            Matrix4x4 matrix = part.GetComponentInParent<Scene>().GetSH3ToUnityMatrix();

            List<Vector3> _verts = new List<Vector3>();
            List<Vector3> _norms = new List<Vector3>();
            List<Vector2> _uvs = new List<Vector2>();
            List<Color32> _colors = new List<Color32>();
            for (int i = 0; i != VertexCount; i++)
            {
                Vector3 temp = reader.ReadVector3();
                temp.y = -temp.y;
                _verts.Add(matrix.MultiplyPoint(temp));

                temp = reader.ReadVector3();
                temp.x = -temp.x;
                temp.z = -temp.z;
                _norms.Add(temp);

                _uvs.Add(reader.ReadVector2());
                _colors.Add(reader.ReadBGRA());
            }

            Mesh mesh = MeshUtils.MakeStripped(_verts, _norms, _uvs, _colors);

            mesh.name = "mesh_" + offset;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return NextOffset;
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