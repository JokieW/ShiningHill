using SH.Core;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Analytics;

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

        public static Mesh MakeIndexedStrip(List<Vector3> vertices, Dictionary<int, ushort[]> indices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
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
            int subMesh = 0;
            foreach(KeyValuePair<int, ushort[]> kvp in indices)
            {
                ushort[] groupIndices = kvp.Value;
                List<int> _tris = new List<int>();

                for (int i = 1; i < groupIndices.Length - 1; i += 2)
                {
                    _tris.Add(groupIndices[i]);
                    _tris.Add(groupIndices[i + 1]);
                    _tris.Add(groupIndices[i - 1]);

                    if (i + 2 < groupIndices.Length)
                    {
                        _tris.Add(groupIndices[i]);
                        _tris.Add(groupIndices[i + 2]);
                        _tris.Add(groupIndices[i + 1]);
                    }
                }

                mesh.SetTriangles(_tris, subMesh++);
            }
            return mesh;
        }

        public static Mesh MakeIndexedStrip(List<Vector3> vertices, IReadOnlyList<ushort> indices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
        {
            CollectionPool.Request(out List<int> _tris);

            int toRead = indices.Count;
            int read = 2;
            int i = 0;

            int memory = indices[i++] << 0x10;
            int shift = 0x0;
            uint mask = 0xFFFF0000;
            ushort currentIndex = indices[i++];

            do
            {
                unchecked
                {
                    memory = (memory & (int)mask) + (currentIndex << shift);
                    mask ^= 0xFFFFFFFF;
                    shift ^= 0x10;

                    read++;
                    currentIndex = indices[i++];

                    _tris.Add((ushort)(memory >> 0x10));
                    _tris.Add((ushort)memory);
                    _tris.Add(currentIndex);
                }
            } while (read < toRead);

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

            mesh.SetTriangles(_tris, 0);
            CollectionPool.Return(ref _tris);

            return mesh;
        }

        public static Mesh MakeStrip(List<Vector3> vertices, IReadOnlyList<ushort> indices, int length, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
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

            CollectionPool.Request(out List<int> _tris);
            for (int i = 0; i < indices.Count; i++)
            {
                _tris.Add(indices[i]);
                _tris.Add(indices[i + 1]);
                _tris.Add(indices[i + 2]);
            }
            mesh.SetTriangles(_tris, 0);
            CollectionPool.Return(ref _tris);

            return mesh;
        }

        public static int Unstrip(int from, ushort indexAdjust, int stripLength, int stripCount, IReadOnlyList<ushort> indices, List<int> triangles)
        {
            int totalRead = 0;
            int i = from;
            for (int i_strips = 0; i_strips < stripCount; i_strips++)
            {
                unchecked
                {
                    int memory = (ushort)(indices[i++] - indexAdjust) << 0x10;
                    int mask = (int)0xFFFF0000;
                    ushort currentIndex = (ushort)(indices[i++] - indexAdjust);
                    int read = 2;

                    while (read < stripLength)
                    {
                        memory = (memory & mask) + (currentIndex << (0x10 & mask));
                        mask ^= (int)0xFFFFFFFF;

                        read++;
                        currentIndex = (ushort)(indices[i++] - indexAdjust);

                        triangles.Add((ushort)(memory >> 0x10));
                        triangles.Add((ushort)memory);
                        triangles.Add(currentIndex);
                    }
                    totalRead += read;
                }
            }

            return totalRead;
        }

        public static Mesh MakeIndexed(List<Vector3> vertices, ushort[] indices, List<Vector3> normals = null, List<Vector2> uvs = null, List<Color32> colors = null, bool isBacksided = false)
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

            mesh.SetTriangles(indices, 0);
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
	}
}
