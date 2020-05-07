using System.Collections.Generic;

using UnityEngine;

namespace SH.GameData.Shared
{
    public static class MeshUtil
	{
        public static void Invert(ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Vector2> uvs, ref List<Color32> colors)
        {
            List<Vector3> newVertices = new List<Vector3>(vertices.Count);
            List<Vector3> newNormals = new List<Vector3>(normals.Count);
            List<Vector2> newUvs = new List<Vector2>(uvs.Count);
            List<Color32> newColors = new List<Color32>(colors.Count);
            for (int j = vertices.Count - 1; j > -1; j--)
            {
                newVertices.Add(vertices[j]);
                newNormals.Add(normals[j]);
                newUvs.Add(uvs[j]);
                newColors.Add(colors[j]);
            }
            vertices = newVertices;
            normals = newNormals;
            uvs = newUvs;
            colors = newColors;
        }

        public static Mesh MakeIndexedStrip(List<Vector3> vertices, List<short[]> indices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            if (normals != null)
            {
                mesh.SetNormals(normals);
            }
            if (uvs != null)
            {
                mesh.SetUVs(0, uvs);
            }
            if (colors != null && colors.Count > 0)
            {
                mesh.SetColors(colors);
            }

            mesh.subMeshCount = indices.Count;
            for (int j = 0; j != indices.Count; j++)
            {
                List<int> _tris = new List<int>();

                for (int i = 1; i < indices[j].Length - 1; i += 2)
                {
                    _tris.Add(indices[j][i]);
                    _tris.Add(indices[j][i - 1]);
                    _tris.Add(indices[j][i + 1]);

                    if (i + 2 < indices[j].Length)
                    {
                        _tris.Add(indices[j][i]);
                        _tris.Add(indices[j][i + 1]);
                        _tris.Add(indices[j][i + 2]);
                    }
                }

                mesh.SetTriangles(_tris, j);
            }
            return mesh;
        }

        public static Mesh MakeIndexedStrip(List<Vector3> vertices, List<short> indices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            if (normals != null)
            {
                mesh.SetNormals(normals);
            }
            if (uvs != null)
            {
                mesh.SetUVs(0, uvs);
            }
            if (colors != null && colors.Count > 0)
            {
                mesh.SetColors(colors);
            }
            
            List<int> _tris = new List<int>();

            for (int i = 1; i < indices.Count - 1; i += 2)
            {
                _tris.Add(indices[i]);
                _tris.Add(indices[i - 1]);
                _tris.Add(indices[i + 1]);

                if (i + 2 < indices.Count)
                {
                    _tris.Add(indices[i]);
                    _tris.Add(indices[i + 1]);
                    _tris.Add(indices[i + 2]);
                }
            }

            mesh.SetTriangles(_tris, 0);
            return mesh;
        }

        public static Mesh MakeIndexedStripInverted(List<Vector3> vertices, List<short> indices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            if (normals != null)
            {
                mesh.SetNormals(normals);
            }
            if (uvs != null)
            {
                mesh.SetUVs(0, uvs);
            }
            if (colors != null && colors.Count > 0)
            {
                mesh.SetColors(colors);
            }
            
            List<int> _tris = new List<int>();

            for (int i = 1; i < indices.Count - 1; i += 2)
            {
                _tris.Add(indices[i + 1]);
                _tris.Add(indices[i - 1]);
                _tris.Add(indices[i]);

                if (i + 2 < indices.Count)
                {
                    _tris.Add(indices[i + 2]);
                    _tris.Add(indices[i + 1]);
                    _tris.Add(indices[i]);
                }
            }

            mesh.SetTriangles(_tris, 0);
            return mesh;
        }

        public static Mesh MakeStripped(List<Vector3> vertices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            if (normals != null)
            {
                mesh.SetNormals(normals);
            }
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

        /// <summary>
        /// /////////////////
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="uvs"></param>
        /// <param name="colors"></param>
        /// <param name="isBacksided"></param>
        /// <returns></returns>
        public static Mesh MakeStrippedInverted(List<Vector3> vertices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            if (normals != null)
            {
                mesh.SetNormals(normals);
            }
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

        public static Mesh MakeSquare(List<Vector3> vertices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            if (normals != null)
            {
                mesh.SetNormals(normals);
            }
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

        public static Mesh MakeSquareInverted(List<Vector3> vertices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            if (normals != null)
            {
                mesh.SetNormals(normals);
            }
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
                    _tris.Add(i - 1);
                    _tris.Add(i + 1);

                    if (isBacksided)
                    {
                        _tris.Add(i + 1);
                        _tris.Add(i - 1);
                        _tris.Add(i + 2);
                    }
                }
            }

            mesh.SetTriangles(_tris, 0);
            return mesh;
        }
	}
}
