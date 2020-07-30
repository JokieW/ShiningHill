using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

using SH.Core;
using SH.GameData.Shared;
using SH.GameData.SH1;

namespace SH.GameData.SH2
{

    [Serializable]
    public class FileMap
    {
        public Header header;
        public FileTex textureFile;
        public FileMesh meshFile;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            [Hex] public int magicByte; // 0x20010510
            [Hex] public int fileLength;
            [Hex] public int field_08;
            [Hex] public int field_0C;
        }
         
        [Serializable]
        public class SubFile
        {
            public SubFileHeader subFileHeader;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct SubFileHeader
            {
                public int subFileId;
                [Hex] public int fileLength;
                [Hex] public int field_08;
                [Hex] public int field_0C;
            }
        }

        [Serializable]
        public class FileMesh : SubFile
        {
            public Header header;
            public MeshGroup meshGroup;
            public MeshMaterial[] materials;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public uint magicBytes; //0x20010730
                [Hex] public int field_04;
                [Hex] public int meshSize;
                [Hex] public int materialCount;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct MeshMaterial
            {
                [Hex] public short field_00;
                [Hex] public short textureId;
                [Hex] public int field_04;
                [Hex] public int field_08;
                [Hex] public int field_0C;
            }

            [Serializable]
            public class MeshGroup
            {
                public Header1 header1;
                public Header2 header2;
                public SubMeshGroup[] subMeshGroups;
                public MeshGroupVerticesHeader meshGroupVerticesHeader;
                public byte[] vertices; //is either Vertex20 or Vertex24
                public short[] indices;

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct Header1
                {
                    [Hex] public int field_00; //Next mesh group?
                    [Hex] public int meshGroupSize;
                    [Hex] public int field_08; //saw 0x14
                    [Hex] public int field_0C;

                    [Hex] public int field_10; //to something after indices
                    [Hex] public int field_14; //saw 0x01
                    [Hex] public int field_18; //saw 0x08
                }

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct Header2
                {
                    public Vector4 boundingBoxA;
                    public Vector4 boundingBoxB;

                    [Hex] public int field_20;
                    [Hex] public int offsetToIndices;
                    [Hex] public int indicesLength;
                    [Hex] public int field_2C; //looks like a length or offset but cant find from where

                    [Hex] public int subMeshGroupCount;
                }

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct SubMeshGroup
                {
                    [Hex] public int id;
                    [Hex] public int field_04;
                    [Hex] public int field_08; // often 0x01
                    [Hex] public short indexCount;
                    [Hex] public short field_0E;

                    [Hex] public short firstIndex;
                    [Hex] public short lastIndex;
                }

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct MeshGroupVerticesHeader
                {
                    [Hex] public int verticesSize;
                    [Hex] public int field_04; // often 0x01
                    [Hex] public int field_08; // often 0
                    [Hex] public int vertexSize; //either 0x14, 0x20 (+ uv) or 0x24 (+ color)

                    [Hex] public int verticesSize2; //Always the same as verticesSize?
                }

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct Vertex14
                {
                    public Vector3 position;
                    public Vector2 uv;

                    public unsafe static void ExtractToBuffers(byte[] source, List<Vector3> vertices, List<Vector2> uvs)
                    {
                        int v14Count = source.Length / Marshal.SizeOf<Vertex14>();

                        fixed (void* sourcePtr = source)
                        {
                            Vertex14* v14Ptr = (Vertex14*)sourcePtr;
                            for (int i = 0; i < v14Count; i++)
                            {
                                Vertex14 v14 = *v14Ptr++;
                                vertices.Add(v14.position);
                                uvs.Add(v14.uv);
                            }
                        }
                    }
                }

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct Vertex20
                {
                    public Vector3 position;
                    public Vector3 normal;
                    public Vector2 uv;

                    public unsafe static void ExtractToBuffers(byte[] source, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs)
                    {
                        int v20Count = source.Length / Marshal.SizeOf<Vertex20>();

                        fixed (void* sourcePtr = source)
                        {
                            Vertex20* v20Ptr = (Vertex20*)sourcePtr;
                            for(int i = 0; i < v20Count; i++)
                            {
                                Vertex20 v20 = *v20Ptr++;
                                vertices.Add(v20.position);
                                normals.Add(v20.normal);
                                uvs.Add(v20.uv);
                            }
                        }
                    }
                }

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct Vertex24
                {
                    public Vector3 position;
                    public Vector3 normal;
                    public ColorBGRA color;
                    public Vector2 uv;

                    public unsafe static void ExtractToBuffers(byte[] source, List<Vector3> vertices, List<Vector3> normals, List<ColorBGRA> colors, List<Vector2> uvs)
                    {
                        int v24Count = source.Length / Marshal.SizeOf<Vertex24>();

                        fixed (void* sourcePtr = source)
                        {
                            Vertex24* v24Ptr = (Vertex24*)sourcePtr;
                            for (int i = 0; i < v24Count; i++)
                            {
                                Vertex24 v24 = *v24Ptr++;
                                vertices.Add(v24.position);
                                normals.Add(v24.normal);
                                colors.Add(v24.color);
                                uvs.Add(v24.uv);
                            }
                        }
                    }
                }
            }
        }

        public static FileMap ReadMapFile(string path)
        {
            FileMap mapFile = new FileMap();
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {
                mapFile.ReadFile(reader);
            }
            return mapFile;
        }

        public void ReadFile(BinaryReader reader)
        {
            header = reader.ReadStruct<Header>();

            textureFile = new FileTex();
            textureFile.subFileHeader = reader.ReadStruct<SubFile.SubFileHeader>();
            textureFile.ReadFile(reader);

            meshFile = new FileMesh();
            meshFile.subFileHeader = reader.ReadStruct<SubFile.SubFileHeader>();
            long magicPosition = reader.BaseStream.Position;
            meshFile.header = reader.ReadStruct<FileMesh.Header>();

            FileMesh.MeshGroup meshGroup = meshFile.meshGroup = new FileMesh.MeshGroup();
            meshGroup.header1 = reader.ReadStruct<FileMesh.MeshGroup.Header1>();
            meshGroup.header2 = reader.ReadStruct<FileMesh.MeshGroup.Header2>();

            meshGroup.subMeshGroups = new FileMesh.MeshGroup.SubMeshGroup[meshGroup.header2.subMeshGroupCount];
            reader.ReadStruct(meshGroup.subMeshGroups);
            meshGroup.meshGroupVerticesHeader = reader.ReadStruct<FileMesh.MeshGroup.MeshGroupVerticesHeader>();

            meshGroup.vertices = new byte[meshGroup.meshGroupVerticesHeader.verticesSize];
            reader.ReadBytes(meshGroup.vertices);

            meshGroup.indices = new short[meshGroup.header2.indicesLength / sizeof(short)];
            reader.ReadInt16(meshGroup.indices);

            
            reader.BaseStream.Position = magicPosition + meshFile.header.meshSize;
            meshFile.materials = new FileMesh.MeshMaterial[meshFile.header.materialCount];
            reader.ReadStruct(meshFile.materials);
        }

        public void WriteFile(BinaryWriter writer)
        {

        }
    }
}
