using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SilentParty
{
	public class MeshPart : MonoBehaviour 
	{
        public int NextSceneGeoOffset;
        public int HeaderLength; //48
        public int Length;
        public int Unknown1;

        public int VertexCount; //Also id thing//
        public int SubId;
        public int Unknown2;
        public int Unknown3;

        public int Unknown4;
        public int Unknown5;
        public int Unknown6;
        public int Unknown7;

        public int Unknown8;
        public int Unknown9;
        public int Unknown10;
        public int Unknown11;

        //Non serialisation data
        private static Color[] _gizmoColors = new Color[] { Color.red, Color.yellow, Color.green, Color.magenta, Color.cyan, Color.grey, Color.blue, Color.black, Color.white };
        [SerializeField]
        private Color _gizmoColor;
        private static int _colorIdentifier;

        public static MeshPart Deserialise(BinaryReader reader, GameObject parent)
        {
            GameObject go = new GameObject("Mesh Part");
            MeshPart part = go.AddComponent<MeshPart>();
            part.transform.SetParent(parent.transform);

            part._gizmoColor = _gizmoColors[_colorIdentifier%9];
            _colorIdentifier++;

            part.NextSceneGeoOffset = reader.ReadInt32();
            part.HeaderLength = reader.ReadInt32();
            part.Length = reader.ReadInt32();
            part.Unknown1 = reader.ReadInt32();

            part.VertexCount = reader.ReadInt32();
            part.SubId = reader.ReadInt32();
            part.Unknown2 = reader.ReadInt32();
            part.Unknown3 = reader.ReadInt32();

            part.Unknown4 = reader.ReadInt32();
            part.Unknown5 = reader.ReadInt32();
            part.Unknown6 = reader.ReadInt32();
            part.Unknown7 = reader.ReadInt32();

            part.Unknown8 = reader.ReadInt32();
            part.Unknown9 = reader.ReadInt32();
            part.Unknown10 = reader.ReadInt32();
            part.Unknown11 = reader.ReadInt32();

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
                temp.y = -temp.y;
                _norms.Add(temp);

                _uvs.Add(reader.ReadVector2());
                _colors.Add(reader.ReadColor32());
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(_verts);
            mesh.SetNormals(_norms);
            mesh.SetUVs(1, _uvs);
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

            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("White", typeof(Material));

            return part;
        }

        
        void OnDrawGizmos()
        {
            /*Gizmos.color = _gizmoColor;
            foreach (Vector3 v3 in GetComponent<MeshFilter>().mesh.vertices)
            {
                Gizmos.DrawSphere(v3, 10.0f);
            }*/

        }


	}
}