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
        public MapMaterial[] mapMaterials;

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
            public MeshGroup opaqueGroup;
            public MeshGroup transparentGroup;
            public MapDecals mapDecals;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public int geometryId;
                [Hex] public int meshGroupSize;
                [Hex] public int offsetToOpaqueGroup;
                [Hex] public int offsetToTransparentGroup;

                [Hex] public int offsetToDecals;
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

                        [Hex] public int offsetToVertexSectionHeader;
                        [Hex] public int offsetToIndices;
                        [Hex] public int indicesLength;
                        [Hex] public int unused; //looks like a length or offset, setting to garbage does nothing

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
                            [Hex] public ushort stripLength;
                            [Hex] public byte invertReading;
                            [Hex] public byte stripCount;
                            [Hex] public ushort firstVertex;
                            [Hex] public ushort lastVertex;
                        }
                    }
                }
            }

            [Serializable]
            public class MapDecals
            {
                public int[] offsetToDecals;
                public Decal[] decals;

                [Serializable]
                public class Decal
                {
                    public Header header;
                    public SubDecal[] subDecals;
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
                        [Hex] public int decalCount;
                    }

                    [Serializable]
                    [StructLayout(LayoutKind.Sequential, Pack = 0)]
                    public struct SubDecal
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
                public Color32 color;
                public Vector2 uv;

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
        public struct MapMaterial
        {
            [Hex] public short mode; //0 = emissive, 1 = cutout, 2 = specular, 4 = diffuse (use opaque or transparent varieties depedngind on mesh group
            [Hex] public short textureId; //maps to the id of the DXTTexture.Header
            public ColorBGRA materialColor; //not sure
            public ColorBGRA overlayColor;
            public float specularity; // 0.0 to 100.0
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
                    long meshGroupOffset = j == 0 ? geo.header.offsetToOpaqueGroup : geo.header.offsetToTransparentGroup;
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
                            geo.opaqueGroup = meshGroup;
                        }
                        else
                        {
                            geo.transparentGroup = meshGroup;
                        }
                    }
                }

                //Get decalss
                if (geo.header.offsetToDecals != 0x00000000)
                {
                    long positiontest5 = reader.BaseStream.Position;
                    geo.mapDecals = new Geometry.MapDecals();
                    long decalsBaseOffset = reader.BaseStream.Position;

                    geo.mapDecals.offsetToDecals = reader.ReadInt32(reader.ReadInt32() /* offsets count */);
                    geo.mapDecals.decals = new Geometry.MapDecals.Decal[geo.mapDecals.offsetToDecals.Length];
                    for (int k = 0; k < geo.mapDecals.decals.Length; k++)
                    {
                        reader.BaseStream.Position = decalsBaseOffset + geo.mapDecals.offsetToDecals[k];
                        Geometry.MapDecals.Decal decal = new Geometry.MapDecals.Decal();
                        decal.header = reader.ReadStruct<Geometry.MapDecals.Decal.Header>();
                        decal.subDecals = reader.ReadStruct<Geometry.MapDecals.Decal.SubDecal>(decal.header.decalCount);
                        decal.vertexSectionsHeader = reader.ReadStruct<Geometry.VertexSectionsHeader>();
                        decal.vertexSections = reader.ReadStruct<Geometry.VertexSectionHeader>(decal.vertexSectionsHeader.vertexSectionCount);
                        decal.vertices = new byte[decal.vertexSections.Length][];
                        for (int j = 0; j < decal.vertexSections.Length; j++)
                        {
                            decal.vertices[j] = reader.ReadBytes(decal.vertexSections[j].sectionLength);
                        }
                        decal.indices = reader.ReadUInt16(decal.header.indicesLength / sizeof(ushort));
                        reader.AlignToLine();
                        geo.mapDecals.decals[k] = decal;
                    }
                }

                geometries[i] = geo;
            }

            //Get materials
            reader.BaseStream.Position = fileBaseOffset + header.meshSize;
            mapMaterials = reader.ReadStruct<MapMaterial>(header.materialCount);
        }

        public override FileMap.SubFileHeader GetSubFileHeader()
        {
            return subFileHeader;
        }
    }
}
