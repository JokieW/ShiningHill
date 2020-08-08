using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

using SH.Core;
using SH.GameData.Shared;
using System.Linq.Expressions;

namespace SH.GameData.SH2
{
    [Serializable]
    public class FileGeometry : FileMapSubFile
    {
        public FileMap.SubFileHeader subFileHeader;
        public Header header;
        public Geometry[] geometries;
        public MeshMaterial[] materials;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            [Hex] public uint magicBytes; //0x20010730
            [Hex] public int geometryCount; 
            [Hex] public int meshSize;
            [Hex] public int materialCount;
        }

        [Serializable]
        public class Geometry
        {
            public Header header;
            public MeshGroup meshGroup0;
            public MeshGroup meshGroup1;
            public MapDecorations mapDecorations;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public int field_00; //Next mesh group?
                [Hex] public int meshGroupSize;
                [Hex] public int offsetToFirstMeshGroup; //saw 0x14, offset to first mesh group?
                [Hex] public int offsetToSecondMeshGroup;

                [Hex] public int offsetToDecorations;
            }

            [Serializable]
            public class MeshGroup
            {
                [Hex] public int subMeshGroupCount;
                public int[] subMeshGroupOffsets;
                public SubMeshGroup[] subMeshGroups;

                [Serializable]
                public class SubMeshGroup
                {
                    public Header header;
                    public SubSubMeshGroup[] subSubMeshGroups;
                    public VertexSectionsHeader vertexSectionsHeader;
                    public VertexSectionHeader[] vertexSections;
                    public byte[][] vertices;
                    public ushort[] indices;

                    [Serializable]
                    [StructLayout(LayoutKind.Sequential, Pack = 0)]
                    public struct Header
                    {
                        public Vector4 boundingBoxA;
                        public Vector4 boundingBoxB;

                        [Hex] public int field_20; //Looks like flags?
                        [Hex] public int offsetToIndices;
                        [Hex] public int indicesLength;
                        [Hex] public int field_2C; //looks like a length or offset but cant find from where

                        [Hex] public int subSubMeshGroupCount;
                    }

                    [Serializable]
                    public class SubSubMeshGroup
                    {
                        public Header header;
                        public MeshPart[] meshParts;

                        [Serializable]
                        [StructLayout(LayoutKind.Sequential, Pack = 0)]
                        public struct Header
                        {
                            [Hex] public int materialIndex;
                            [Hex] public int sectionId;
                            [Hex] public int meshPartCount;
                        }

                        [Serializable]
                        [StructLayout(LayoutKind.Sequential, Pack = 0)]
                        public struct MeshPart
                        {
                            [Hex] public short stripLength;
                            [Hex] public byte field_02; //saw 0, 1
                            [Hex] public byte stripCount;
                            [Hex] public ushort firstVertex;
                            [Hex] public ushort lastVertex;
                        }
                    }
                }
            }

            [Serializable]
            public class MapDecorations
            {
                public int[] offsetToDecorations;
                public Decoration[] decorations;

                [Serializable]
                public class Decoration
                {
                    public Header header;
                    public SubDecoration[] subDecorations;
                    public VertexSectionsHeader vertexSectionsHeader;
                    public VertexSectionHeader[] vertexSections;
                    public byte[][] vertices;
                    public ushort[] indices;

                    [Serializable]
                    [StructLayout(LayoutKind.Sequential, Pack = 0)]
                    public struct Header
                    {
                        public Vector4 boundingBoxA;
                        public Vector4 boundingBoxB;

                        [Hex] public int field_20;
                        [Hex] public int offsetToIndices;
                        [Hex] public int indicesLength;
                        [Hex] public int decorationCount;
                    }

                    [Serializable]
                    [StructLayout(LayoutKind.Sequential, Pack = 0)]
                    public struct SubDecoration
                    {
                        [Hex] public int materialIndex;
                        [Hex] public int sectionId;
                        [Hex] public int stripLength;
                        [Hex] public int stripCount;
                    }
                }
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct VertexSectionsHeader
            {
                [Hex] public int verticesLength;
                [Hex] public int vertexSectionCount;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct VertexSectionHeader
            {
                [Hex] public int sectionStart;
                [Hex] public int vertexSize; //either 0x14, 0x20 (+ uv) or 0x24 (+ color)
                [Hex] public int sectionLength;
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
            public struct Vertex18
            {
                public Vector3 position;
                public Vector2 uv;
                public Color32 color;

                public unsafe static void ExtractToBuffers(byte[] source, List<Vector3> vertices, List<Vector2> uvs, List<Color32> colors)
                {
                    int v18Count = source.Length / Marshal.SizeOf<Vertex18>();

                    fixed (void* sourcePtr = source)
                    {
                        Vertex18* v18Ptr = (Vertex18*)sourcePtr;
                        for (int i = 0; i < v18Count; i++)
                        {
                            Vertex18 v18 = *v18Ptr++;
                            vertices.Add(v18.position);
                            uvs.Add(v18.uv);
                            colors.Add(v18.color);
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
                        for (int i = 0; i < v20Count; i++)
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
                public Color32 color;
                public Vector2 uv;

                public unsafe static void ExtractToBuffers(byte[] source, List<Vector3> vertices, List<Vector3> normals, List<Color32> colors, List<Vector2> uvs)
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

            public static void UnpackVertices(int vertexSize, byte[] rawVertices, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<Color32> colors)
            {
                if (vertexSize == 0x0)
                {
                    return;
                }
                else if (vertexSize == 0x14)
                {
                    Vertex14.ExtractToBuffers(rawVertices, vertices, uvs);
                }
                else if (vertexSize == 0x18)
                {
                    Vertex18.ExtractToBuffers(rawVertices, vertices, uvs, colors);
                }
                else if (vertexSize == 0x20)
                {
                    Vertex20.ExtractToBuffers(rawVertices, vertices, normals, uvs);
                }
                else if (vertexSize == 0x24)
                {
                    Vertex24.ExtractToBuffers(rawVertices, vertices, normals, colors, uvs);
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct MeshMaterial
        {
            [Hex] public short field_00; //saw 1 and 2
            [Hex] public short textureId; //maps to the id of the DXTTexture.Header
            [Hex] public int field_04; //saw FFB2B2B2B2, FFFFFFFF, FF000000
            [Hex] public int field_08; //specularity? only saw FF000000 for field_00 = 1 and FFFFFFFF for field_00 = 2
            public float field_0C;
        }

        public void ReadFile(BinaryReader reader)
        {
            long fileBaseOffset = reader.BaseStream.Position;
            header = reader.ReadStruct<Header>();

            geometries = new Geometry[header.geometryCount];
            for (int i = 0; i < geometries.Length; i++)
            {
                long positiontest1 = reader.BaseStream.Position;
                Geometry geo = new Geometry();
                geo.header = reader.ReadStruct<Geometry.Header>();

                for (int j = 0; j < 2; j++)
                {
                    long positiontest2 = reader.BaseStream.Position;
                    long meshGroupOffset = j == 0 ? geo.header.offsetToFirstMeshGroup : geo.header.offsetToSecondMeshGroup;
                    if (meshGroupOffset != 0x00000000)
                    {
                        Geometry.MeshGroup meshGroup = new Geometry.MeshGroup();
                        meshGroup.subMeshGroupCount = reader.ReadInt32();
                        meshGroup.subMeshGroupOffsets = reader.ReadInt32(meshGroup.subMeshGroupCount);
                        meshGroup.subMeshGroups = new Geometry.MeshGroup.SubMeshGroup[meshGroup.subMeshGroupCount];
                        for (int k = 0; k < meshGroup.subMeshGroupCount; k++)
                        {
                            long positiontest3 = reader.BaseStream.Position;
                            Geometry.MeshGroup.SubMeshGroup subMeshGroup = new Geometry.MeshGroup.SubMeshGroup();
                            subMeshGroup.header = reader.ReadStruct<Geometry.MeshGroup.SubMeshGroup.Header>();
                            subMeshGroup.subSubMeshGroups = new Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup[subMeshGroup.header.subSubMeshGroupCount];
                            for (int l = 0; l < subMeshGroup.subSubMeshGroups.Length; l++)
                            {
                                long positiontest4 = reader.BaseStream.Position;
                                Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup subSubMeshGroup = new Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup();
                                subSubMeshGroup.header = reader.ReadStruct<Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup.Header>();
                                subSubMeshGroup.meshParts = reader.ReadStruct<Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup.MeshPart>(subSubMeshGroup.header.meshPartCount);

                                subMeshGroup.subSubMeshGroups[l] = subSubMeshGroup;
                            }

                            subMeshGroup.vertexSectionsHeader = reader.ReadStruct<Geometry.VertexSectionsHeader>();
                            subMeshGroup.vertexSections = reader.ReadStruct<Geometry.VertexSectionHeader>(subMeshGroup.vertexSectionsHeader.vertexSectionCount);
                            subMeshGroup.vertices = new byte[subMeshGroup.vertexSections.Length][];
                            for (int l = 0; l < subMeshGroup.vertexSections.Length; l++)
                            {
                                subMeshGroup.vertices[l] = reader.ReadBytes(subMeshGroup.vertexSections[l].sectionLength);
                            }
                            subMeshGroup.indices = reader.ReadUInt16(subMeshGroup.header.indicesLength / sizeof(ushort));
                            reader.AlignToLine();
                            meshGroup.subMeshGroups[k] = subMeshGroup;
                        }

                        if(j == 0)
                        {
                            geo.meshGroup0 = meshGroup;
                        }
                        else
                        {
                            geo.meshGroup1 = meshGroup;
                        }
                    }
                }

                //Get decorations
                if (geo.header.offsetToDecorations != 0x00000000)
                {
                    long positiontest5 = reader.BaseStream.Position;
                    geo.mapDecorations = new Geometry.MapDecorations();
                    long decorationsBaseOffset = reader.BaseStream.Position;

                    geo.mapDecorations.offsetToDecorations = reader.ReadInt32(reader.ReadInt32() /* offsets count */);
                    geo.mapDecorations.decorations = new Geometry.MapDecorations.Decoration[geo.mapDecorations.offsetToDecorations.Length];
                    for (int k = 0; k < geo.mapDecorations.decorations.Length; k++)
                    {
                        reader.BaseStream.Position = decorationsBaseOffset + geo.mapDecorations.offsetToDecorations[k];
                        Geometry.MapDecorations.Decoration decoration = new Geometry.MapDecorations.Decoration();
                        decoration.header = reader.ReadStruct<Geometry.MapDecorations.Decoration.Header>();
                        decoration.subDecorations = reader.ReadStruct<Geometry.MapDecorations.Decoration.SubDecoration>(decoration.header.decorationCount);
                        decoration.vertexSectionsHeader = reader.ReadStruct<Geometry.VertexSectionsHeader>();
                        decoration.vertexSections = reader.ReadStruct<Geometry.VertexSectionHeader>(decoration.vertexSectionsHeader.vertexSectionCount);
                        decoration.vertices = new byte[decoration.vertexSections.Length][];
                        for (int j = 0; j < decoration.vertexSections.Length; j++)
                        {
                            decoration.vertices[j] = reader.ReadBytes(decoration.vertexSections[j].sectionLength);
                        }
                        decoration.indices = reader.ReadUInt16(decoration.header.indicesLength / sizeof(ushort));
                        reader.AlignToLine();
                        geo.mapDecorations.decorations[k] = decoration;
                    }
                }

                geometries[i] = geo;
            }

            //Get materials
            reader.BaseStream.Position = fileBaseOffset + header.meshSize;
            materials = reader.ReadStruct<MeshMaterial>(header.materialCount);
        }

        public override FileMap.SubFileHeader GetSubFileHeader()
        {
            return subFileHeader;
        }
    }
}
