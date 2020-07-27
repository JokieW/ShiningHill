using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

using SH.Core;
using System.Collections.ObjectModel;

namespace SH.GameData.Shared
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public unsafe struct CollisionHeader
    {
        public Vector2 origin;
        public int floorGroupLength;
        public int wallGroupLength;

        public int something__GroupLength;
        public int furniture__GroupLength;
        public int radialGroupLength;
        public int padding;

        public CollisionOffsetTable offsetTable;

        public void UpdateFrom(Vector2 origin, int[][][] indicesList, CollisionFace[] faces0, CollisionFace[] faces1, CollisionFace[] faces2, CollisionFace[] faces3, CollisionCylinder[] cylinders)
        {
            this.floorGroupLength = (faces0.Length + 1) * 0x50;
            this.wallGroupLength = (faces1.Length + 1) * 0x50;
            this.something__GroupLength = (faces2.Length + 1) * 0x50;
            this.furniture__GroupLength = (faces3.Length + 1) * 0x50;
            this.radialGroupLength = (cylinders.Length + 1) * 0x30;
            this.padding = 0x00;

            this.offsetTable = new CollisionOffsetTable();

            int fileOffset = 0x0174;
            for (int i = 0; i < CollisionOffsetTable.IndicesPerGroup; i++)
            {
                offsetTable.group0IndicesOffsets[i] = fileOffset;
                fileOffset += 0x04 + (0x04 * indicesList[0][i].Length);
            }
            for (int i = 0; i < CollisionOffsetTable.IndicesPerGroup; i++)
            {
                offsetTable.group1IndicesOffsets[i] = fileOffset;
                fileOffset += 0x04 + (0x04 * indicesList[1][i].Length);
            }
            for (int i = 0; i < CollisionOffsetTable.IndicesPerGroup; i++)
            {
                offsetTable.group2IndicesOffsets[i] = fileOffset;
                fileOffset += 0x04 + (0x04 * indicesList[2][i].Length);
            }
            for (int i = 0; i < CollisionOffsetTable.IndicesPerGroup; i++)
            {
                offsetTable.group3IndicesOffsets[i] = fileOffset;
                fileOffset += 0x04 + (0x04 * indicesList[3][i].Length);
            }
            for (int i = 0; i < CollisionOffsetTable.IndicesPerGroup; i++)
            {
                offsetTable.group4IndicesOffsets[i] = fileOffset;
                fileOffset += 0x04 + (0x04 * indicesList[4][i].Length);
            }

            if (fileOffset % 0x10 != 0)
            {
                fileOffset += 0x10 - (fileOffset % 0x10);
            }

            offsetTable.group0VertexOffset = fileOffset;
            fileOffset += 0x50 + (0x50 * faces0.Length);

            offsetTable.group1VertexOffset = fileOffset;
            fileOffset += 0x50 + (0x50 * faces1.Length);

            offsetTable.group2VertexOffset = fileOffset;
            fileOffset += 0x50 + (0x50 * faces2.Length);

            offsetTable.group3VertexOffset = fileOffset;
            fileOffset += 0x50 + (0x50 * faces3.Length);

            offsetTable.group4VertexOffset = fileOffset;
            fileOffset += 0x30 + (0x30 * cylinders.Length);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 0, Size = 0x154)]
    public unsafe struct CollisionOffsetTable
    {
        public const int OffsetsCount = 0x55;
        public const int IndicesPerGroup = 0x10;
        [FieldOffset(0x00)]
        public fixed int allOffsets[OffsetsCount];
        [FieldOffset(IndicesPerGroup * 0x00)]
        public fixed int group0IndicesOffsets[IndicesPerGroup];
        [FieldOffset(IndicesPerGroup * 0x04)]
        public fixed int group1IndicesOffsets[IndicesPerGroup];
        [FieldOffset(IndicesPerGroup * 0x08)]
        public fixed int group2IndicesOffsets[IndicesPerGroup];
        [FieldOffset(IndicesPerGroup * 0x0C)]
        public fixed int group3IndicesOffsets[IndicesPerGroup];
        [FieldOffset(IndicesPerGroup * 0x10)]
        public fixed int group4IndicesOffsets[IndicesPerGroup];

        [FieldOffset((IndicesPerGroup * 0x14) + 0x00)]
        public int group0VertexOffset;
        [FieldOffset((IndicesPerGroup * 0x14) + 0x04)]
        public int group1VertexOffset;
        [FieldOffset((IndicesPerGroup * 0x14) + 0x08)]
        public int group2VertexOffset;
        [FieldOffset((IndicesPerGroup * 0x14) + 0x0C)]
        public int group3VertexOffset;

        [FieldOffset(IndicesPerGroup * 0x15)]
        public int group4VertexOffset;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct CollisionFace
    {
        public int field_00; //Has flags; 0x01 always 1?, 0x02 always 0?, 0x04 is 1 if quad, 0 if triangle
        public int field_04; //Always 4 in sh3, sh2?
        public int field_08;
        public int padding;

        public Vector4 vertex0;
        public Vector4 vertex1;
        public Vector4 vertex2;
        public Vector4 vertex3;

        public CollisionFace(int field_00, int field_08, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            this.field_00 = field_00;
            this.field_04 = 0x04;
            this.field_08 = field_08;
            this.padding = 0x00;
            this.vertex0 = new Vector4(v0.x, v0.y, v0.z, 1.0f);
            this.vertex1 = new Vector4(v1.x, v1.y, v1.z, 1.0f);
            this.vertex2 = new Vector4(v2.x, v2.y, v2.z, 1.0f);
            this.vertex3 = new Vector4(v3.x, v3.y, v3.z, 1.0f);
        }

        public string GetLabel()
        {
            return
                field_00.ToString("X") + "\n" + 
                field_08.ToString("X");
        }

        public bool isTriangle
        {
            get => (field_00 & 0x100) == 0;
        }

        public bool isQuad
        {
            get => (field_00 & 0x100) != 0;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct CollisionCylinder
    {
        public int field_00;
        public int field_04; //Always 4 in sh3, sh2?
        public int field_08;
        public int padding;

        public Vector4 position;
        public Vector3 height;
        public float radius;

        public string GetLabel()
        {
            return
                field_00.ToString("X") + "\n" +
                field_08.ToString("X");
        }
    }


    [Serializable]
    public unsafe class FileCollisions
    {
        public CollisionHeader header;
        public int[][][] groupIndicesLists; //[group][subgroup][indices]
        public CollisionFace[] group0Faces;
        public CollisionFace[] group1Faces;
        public CollisionFace[] group2Faces;
        public CollisionFace[] group3Faces;
        public CollisionCylinder[] group4Cylinders;

        public CollisionFace[] IndexToFaceArray(int index)
        {
            if (index == 0) return group0Faces;
            if (index == 1) return group1Faces;
            if (index == 2) return group2Faces;
            if (index == 3) return group3Faces;
            throw new IndexOutOfRangeException();
        }

        public void SetFaceArrayAtIndex(int index, CollisionFace[] array)
        {
            if (index == 0) { group0Faces = array; return; }
            if (index == 1) { group1Faces = array; return; }
            if (index == 2) { group2Faces = array; return; }
            if (index == 3) { group3Faces = array; return; }
            throw new IndexOutOfRangeException();
        }

        public Mesh GetAsMesh()
        {
            CollectionPool.Request(out List<Vector3> vertices);
            CollectionPool.Request(out List<int> triangles);
            int triangleIndex = 0;

            for(int i = 0; i < 4; i++)
            {
                CollisionFace[] faces = IndexToFaceArray(i);

                for (int j = 0; j < faces.Length; j++)
                {
                    CollisionFace face = faces[j];
                    vertices.Add(face.vertex0);
                    vertices.Add(face.vertex1);
                    vertices.Add(face.vertex2);
                    vertices.Add(face.vertex3);

                    triangles.Add(triangleIndex);
                    triangles.Add(triangleIndex + 1);
                    triangles.Add(triangleIndex + 2);

                    triangles.Add(triangleIndex + 2);
                    triangles.Add(triangleIndex + 3);
                    triangles.Add(triangleIndex);
                    triangleIndex += 4;
                }
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            CollectionPool.Return(ref vertices);
            CollectionPool.Return(ref triangles);
            return mesh;
        }

        public static FileCollisions ReadCollisionFile(string path)
        {
            FileCollisions collisionFile = null;
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {
                collisionFile = new FileCollisions(reader);
            }

            return collisionFile;
        }

        private FileCollisions()
        {
            header = new CollisionHeader();
            groupIndicesLists = new int[5][][];
            for (int i = 0; i < 5; i++)
            {
                int[][] subgroup = new int[CollisionOffsetTable.IndicesPerGroup][];
                for (int j = 0; j < CollisionOffsetTable.IndicesPerGroup; j++)
                {
                    subgroup[j] = new int[0];
                }
                groupIndicesLists[i] = subgroup;
            }

            group0Faces = new CollisionFace[0];
            group1Faces = new CollisionFace[0];
            group2Faces = new CollisionFace[0];
            group3Faces = new CollisionFace[0];
            group4Cylinders = new CollisionCylinder[0];
            header.UpdateFrom(Vector2.zero, groupIndicesLists, group0Faces, group1Faces, group2Faces, group3Faces, group4Cylinders);
        }

        public FileCollisions(BinaryReader reader)
        {
            header = reader.ReadStruct<CollisionHeader>();

            int i = 0;
            {
                List<int> indicesBuffer = new List<int>(0x20);
                groupIndicesLists = new int[5][][];
                for (int j = 0; j < 5; j++)
                {
                    groupIndicesLists[j] = new int[CollisionOffsetTable.IndicesPerGroup][];
                    for (int k = 0; k < CollisionOffsetTable.IndicesPerGroup; k++)
                    {
                        reader.BaseStream.Position = header.offsetTable.allOffsets[i++];
                        indicesBuffer.Clear();
                        int index;
                        while ((index = reader.ReadInt32()) != -1)
                        {
                            indicesBuffer.Add(index);
                        }
                        groupIndicesLists[j][k] = indicesBuffer.ToArray();
                    }
                }
            }

            {
                List<CollisionFace> faceBuffer = new List<CollisionFace>(0x20);
                for (int j = 0; j < 4; j++)
                {
                    reader.BaseStream.Position = header.offsetTable.allOffsets[i++];
                    faceBuffer.Clear();
                    CollisionFace face;
                    while((face = reader.ReadStruct<CollisionFace>()).field_00 != 0 || face.field_04 != 0 || face.field_08 != 0)
                    {
                        faceBuffer.Add(face);
                    }
                    SetFaceArrayAtIndex(j, faceBuffer.ToArray());
                }
            }

            {
                List<CollisionCylinder> cylinderBuffer = new List<CollisionCylinder>(0x08);
                reader.BaseStream.Position = header.offsetTable.allOffsets[i++];
                CollisionCylinder cylinder;
                while ((cylinder = reader.ReadStruct<CollisionCylinder>()).field_00 != 0 || cylinder.field_04 != 0 || cylinder.field_08 != 0)
                {
                    cylinderBuffer.Add(cylinder);
                }
                group4Cylinders = cylinderBuffer.ToArray();
            }
        }

        public static FileCollisions MakeEmpty()
        {
            FileCollisions mc = new FileCollisions();
            mc.UpdateHeader(Vector2.zero);
            return mc;
        }

        public static FileCollisions MakeDebug()
        {
            FileCollisions mc = new FileCollisions();
            CollisionFace[] floor = new CollisionFace[1]
            {
                new CollisionFace(1, 80,
                    new Vector3(-12575.0f, 0.0f, -15000.0f),
                    new Vector3(-12575.0f, 0.0f, -25000.0f),
                    new Vector3(-27400.0f, 0.0f, -25000.0f),
                    new Vector3(-27400.0f, 0.0f, -15000.0f))
            };
            int[] floorIndices = new int[1] { 0 };
            mc.group0Faces = floor;
            mc.groupIndicesLists[0][0] = floorIndices;

            CollisionFace[] walls = new CollisionFace[4]
            {
                new CollisionFace(1, 0x80,
                    new Vector3(-12600.0f, 0.0f, -15000.0f),
                    new Vector3(-12600.0f, -2500.0f, -15000.0f),
                    new Vector3(-12600.0f, -2500.0f, -25000.0f),
                    new Vector3(-12600.0f, 0.0f, -25000.0f)),
                new CollisionFace(1, 0x80,
                    new Vector3(-12600.0f, 0.0f, -25000.0f),
                    new Vector3(-12600.0f, -2500.0f, -25000.0f),
                    new Vector3(-22600.0f, -2500.0f, -25000.0f),
                    new Vector3(-22600.0f, 0.0f, -25000.0f)),
                new CollisionFace(1, 0x80,
                    new Vector3(-22600.0f, 0.0f, -25000.0f),
                    new Vector3(-22600.0f, -2500.0f, -25000.0f),
                    new Vector3(-22600.0f, -2500.0f, -15000.0f),
                    new Vector3(-22600.0f, 0.0f, -15000.0f)),
                new CollisionFace(1, 0x80,
                    new Vector3(-22600.0f, 0.0f, -15000.0f),
                    new Vector3(-22600.0f, -2500.0f, -15000.0f),
                    new Vector3(-12600.0f, -2500.0f, -15000.0f),
                    new Vector3(-12600.0f, 0.0f, -15000.0f)),
            };
            int[] wallIndices = new int[4] { 0, 1, 2, 3 };
            mc.group1Faces = walls;
            mc.groupIndicesLists[1][0] = wallIndices;

            mc.UpdateHeader(new Vector2(-20000.0f, -20000.0f));
            return mc;
        }

        public void UpdateHeader(Vector2 origin)
        {
            header.UpdateFrom(origin, groupIndicesLists, group0Faces, group1Faces, group2Faces, group3Faces, group4Cylinders);
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteStruct(in header);
            int i = 0;
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < CollisionOffsetTable.IndicesPerGroup; k++)
                {
                    int[] indices = groupIndicesLists[j][k];
                    writer.BaseStream.Position = header.offsetTable.allOffsets[i++];
                    for(int l = 0; l < indices.Length; l++)
                    {
                        writer.Write(indices[l]);
                    }
                    writer.Write(-1);
                }
            }

            for (int j = 0; j < 4; j++)
            {
                writer.BaseStream.Position = header.offsetTable.allOffsets[i++];
                CollisionFace[] faces = IndexToFaceArray(j);
                for(int k = 0; k < faces.Length; k++)
                {
                    writer.WriteStruct(in faces[k]);
                }
                writer.WriteStruct<CollisionFace>(default);
            }

            {
                writer.BaseStream.Position = header.offsetTable.allOffsets[i++];
                for (int j = 0; j < group4Cylinders.Length; j++)
                {
                    writer.WriteStruct(in group4Cylinders[j]);
                }
                writer.WriteStruct<CollisionCylinder>(default);
            }
        }
    }
}
