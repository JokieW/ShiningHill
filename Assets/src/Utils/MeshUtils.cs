using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ShiningHill
{
	public static class MeshUtils 
	{
        public static Mesh MakeStripped(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            if (uvs != null)
            {
                mesh.SetUVs(0, uvs);
            }
            if (colors != null)
            {
                mesh.SetColors(colors);
            }

            List<int> _tris = new List<int>();
            for (int i = 1; i < vertices.Count - 1; i += 2)
            {
                _tris.Add(i);
                _tris.Add(i - 1);
                _tris.Add(i + 1);

                if (isBacksided)
                {
                    _tris.Add(i + 1);
                    _tris.Add(i - 1);
                    _tris.Add(i);
                }

                if (i + 2 < vertices.Count)
                {
                    _tris.Add(i);
                    _tris.Add(i + 1);
                    _tris.Add(i + 2);

                    if (isBacksided)
                    {
                        _tris.Add(i + 2);
                        _tris.Add(i + 1);
                        _tris.Add(i);
                    }
                }
            }

            mesh.SetTriangles(_tris, 0);
            return mesh;
        }

        public static Mesh MakeStrippedInverted(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            if (uvs != null)
            {
                mesh.SetUVs(0, uvs);
            }
            if (colors != null)
            {
                mesh.SetColors(colors);
            }

            List<int> _tris = new List<int>();
            for (int i = 1; i < vertices.Count - 1; i += 2)
            {
                _tris.Add(i + 1);
                _tris.Add(i - 1);
                _tris.Add(i);

                if (isBacksided)
                {
                    _tris.Add(i);
                    _tris.Add(i - 1);
                    _tris.Add(i + 1);
                }

                if (i + 2 < vertices.Count)
                {
                    _tris.Add(i + 2);
                    _tris.Add(i + 1);
                    _tris.Add(i);

                    if (isBacksided)
                    {
                        _tris.Add(i);
                        _tris.Add(i + 1);
                        _tris.Add(i + 2);
                    }
                }
            }

            mesh.SetTriangles(_tris, 0);
            return mesh;
        }

        public static Mesh MakeSquare(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            if (uvs != null)
            {
                mesh.SetUVs(0, uvs);
            }
            if (colors != null)
            {
                mesh.SetColors(colors);
            }

            List<int> _tris = new List<int>();
            for (int i = 1; i < vertices.Count - 1; i += 2)
            {
                _tris.Add(i);
                _tris.Add(i - 1);
                _tris.Add(i + 1);

                if (isBacksided)
                {
                    _tris.Add(i + 1);
                    _tris.Add(i - 1);
                    _tris.Add(i);
                }

                if (i + 2 < vertices.Count)
                {
                    _tris.Add(i + 1);
                    _tris.Add(i - 1);
                    _tris.Add(i + 2);

                    if (isBacksided)
                    {
                        _tris.Add(i + 2);
                        _tris.Add(i - 1);
                        _tris.Add(i + 1);
                    }
                }
            }

            mesh.SetTriangles(_tris, 0);
            return mesh;
        }
	}
}