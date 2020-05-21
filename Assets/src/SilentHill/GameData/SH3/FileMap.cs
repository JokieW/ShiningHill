using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

using SH.Core;
using SH.GameData.Shared;
using SH.Native;

namespace SH.GameData.SH3
{
    [Serializable]
    public class MapGeometry
    {
        public Header mainHeader; //at root
        public ObjectTransform[] transforms; //next line after mainHeader, pointed at by firstObjectTransformOffset
        public MeshGroup[] meshGroups; //next line after transforms, ordered meshPartGBTexOffset -> meshPartTRTexOffset -> meshPartLocalTexOffset
        public Matrix4x4[] interestPoints; //optional, next line after meshgroups, pointed at by interestPointsOffset
        public LightDecal lightDecal; //optional, next line after interestPoints, pointed at by 
        public TextureGroup textureGroup; //at next xxxxxx00 or xxxxxx80 line after lightdecals, pointed at by textureGroupOffset

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            [Hex] public int magicByte; // -1 0xFFFFFFFF
            [Hex] public int field_04;
            [Hex] public int field_08;
            [Hex] public int mainHeaderSize;

            [Hex] public int textureGroupOffset;
            [Hex] public int field_14;
            [Hex] public int firstObjectTransformOffset;
            [Hex] public int meshPartGBTexOffset;

            [Hex] public int meshPartTRTexOffset;
            [Hex] public int meshPartLocalTexOffset;
            [Hex] public int field_28;
            [Hex] public int field_2C;

            [Hex] public int textureGroupOffset2__;
            [Hex] public int interestPointsOffset;
            [Hex] public int lightDecalsOffset;
            [Hex] public int field_3C;

            public short meshPartCount;
            public short meshPartGBTexCount;
            public short meshPartLocalTexCount;
            public short meshPartTRTexCount;
            [Hex] public int field_48;
            [Hex] public int field_4C;
        }


        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct ObjectTransform
        {
            [Hex] public int nextObjectTransformOffset; //0 if no more
            [Hex] public int headerLength;
            [Hex] public int objectTransformLength;
            [Hex] public int field_0C;

            public int objectType;
            [Hex] public int partID;
            [Hex] public int field_18;
            [Hex] public int field_1C;

            public Matrix4x4 transform;
            public unsafe fixed float boundingBox[4 * 8];

            public unsafe Vector4[] GetBoundingBox()
            {
                Vector4[] v4s = new Vector4[8];
                for (int i = 0, j = 0; i < 4 * 8; i += 4, j++)
                {
                    v4s[j] = new Vector4(boundingBox[i + 0], boundingBox[i + 1], boundingBox[i + 2], boundingBox[i + 3]);
                }
                return v4s;
            }
        }

        [Serializable]
        public class MeshGroup
        {
            public Header header;
            public SubMeshGroup[] subs;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public int nextGroupOffset; //0 when done
                [Hex] public int headerLength;
                [Hex] public int totalLength; //from offset of MeshGroup, to nextGroupOffset
                [Hex] public int field_0C;

                public int textureGroup; //3 local, 2 TR, 1 GB
                public int textureIndex;
                public int subMeshGroupCount;
                [Hex] public int field_1C;

                [Hex] public int pad_20;
                [Hex] public int pad_24;
                [Hex] public int pad_28;
                [Hex] public int pad_2C;
            }
        }

        [Serializable]
        public class SubMeshGroup
        {
            public Header header;
            public SubSubMeshGroup[] subsubs;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public int nextSubMeshGroupOffset;
                [Hex] public int headerLength;
                [Hex] public int totalLength;
                [Hex] public int field_0C;

                public short subMeshIndex;
                public short subSubMeshCount;
                [Hex] public short field_14;
                [Hex] public short transparencyType; //0 opaque, 1 transparent, 3 cutout, 8 unknown
                [Hex] public int field_18;
                [Hex] public int field_1C;

                [Hex] public int pad_20;
                [Hex] public int pad_24;
                [Hex] public int pad_28;
                [Hex] public int pad_2C;
            }
        }

        [Serializable]
        public class SubSubMeshGroup
        {
            public Header header;
            public MeshPart[] parts;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public int nextSubSubMeshGroupOffset;
                [Hex] public int headerLength;
                [Hex] public int totalLength;
                [Hex] public int field_0C;

                [Hex] public int illuminationType; //9 ambient?, 8 self-illum, 4 unknown (i.e. bu1f), 0 no illum?, 265 trnasparent mirrors
                [Hex] public int field_14;
                [Hex] public int reserved1; //Used at runtime to store data
                [Hex] public int reserved2; //Used at runtime to store data

                //example of them changing 
                //in mrfe/mrf2 books behind glass
                //mrfd one of the doors
                //mrd1 flamethrower
                public float float_20; // often 1.0
                public float float_24; // often 50.0, seen 168.0
                public float float_28; // often 0.0
                public float float_2C; // often 1.0
            }
        }

        [Serializable]
        public class MeshPart
        {
            public Header header;
            public VertexInfo[] vertices;
            public ExtraData[] extraData;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public int nextMeshPartOffset;
                [Hex] public int headerLength;
                [Hex] public int totalLength;
                [Hex] public int field_0C;

                public int vertexCount;
                public int objectType; //1 = static, 2 = can be or not there, 3 = can move
                [Hex] public int partID;
                [Hex] public int meshFlags;

                [Hex] public int unknownData; //most of the time 0, have seen 180 and 1c0 in cuf3
                [Hex] public int field_24;
                [Hex] public int field_28;
                [Hex] public int field_2C;

                [Hex] public int pad_30;
                [Hex] public int pad_34;
                [Hex] public int pad_38;
                [Hex] public int pad_3C;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct VertexInfo
            {
                public Vector3 position;
                public Vector3 normal;
                public Vector2 uv;
                public ColorBGRA color;

                public VertexInfo(Vector3 position, Vector3 normal, Vector2 uv, ColorBGRA color)
                {
                    this.position = position;
                    this.normal = normal;
                    this.uv = uv;
                    this.color = color;
                }
            }


            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct ExtraData
            {
                [Hex] public int field_00;
                [Hex] public int field_04;
                [Hex] public int field_08;
                [Hex] public int field_0C;

                [Hex] public int field_10;
                [Hex] public int field_14;
                [Hex] public int field_18;
                [Hex] public int field_1C;

                [Hex] public int field_30;
                [Hex] public int field_34;
                [Hex] public int field_38;
                [Hex] public int field_3C;
            }
        }

        [Serializable]
        public class LightDecal
        {
            public Header header;
            public Entry[] entries;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public int firstEntryOffset;
                [Hex] public int field_04;
                [Hex] public int field_08;
                [Hex] public int field_0C;

                [Hex] public int pad_10;
                [Hex] public int pad_14;
                [Hex] public int pad_18;
                [Hex] public int pad_1C;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Entry
            {
                public Header header;
                public Vector4[] instances;
                public VertexData[] instanceData;

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct Header
                {
                    public Matrix4x4 transform;

                    public short positionsCount;
                    public short dataCount;
                    [Hex] public short field_44;
                    [Hex] public short field_46;
                    [Hex] public int field_48;
                    [Hex] public int field_4C;

                    [Hex] public int offsetToPositions;
                    [Hex] public int offsetToData;
                    [Hex] public int offsetToNextEntry;
                    [Hex] public int field_5C;

                    [Hex] public int pad_60;
                    [Hex] public int pad_64;
                    [Hex] public int pad_68;
                    [Hex] public int pad_6C;
                }

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct VertexData
                {
                    public Vector4 vertexOffset;

                    public float u;
                    public float v;
                    public float uvDivider;
                    public float pad_1C;

                    public Color vertexColor; // 0.0f to 128.0f
                }
            }
        }

        public unsafe void DoHack(Mesh mesh, Texture2D texture)
        {
            {
                MeshGroup group0 = meshGroups[0];
                SubMeshGroup subgroup0 = group0.subs[0];
                SubSubMeshGroup subsubgroup0 = subgroup0.subsubs[0];
                MeshPart part = subsubgroup0.parts[0];
                List<Vector3> pr2 = new List<Vector3>();
                List<Vector3> normals2 = new List<Vector3>();
                List<Vector2> uvs2 = new List<Vector2>();

                {
                    int[] indices = mesh.GetTriangles(0);
                    ushort[] shortIndices = new ushort[indices.Length];
                    for (int i = 0; i < indices.Length; i++)
                    {
                        shortIndices[i] = (ushort)indices[i];
                    }
                    NvTriStrip.SetStitchStrips(true);
                    fixed (ushort* ptr = shortIndices)
                    {
                        NvTriStrip.GenerateStrips(ptr, (uint)shortIndices.Length, out NvTriStrip.PrimitiveGroup* prims, out ushort count, true);

                        try
                        {
                            for (int i = 0; i != count; i++)
                            {
                                NvTriStrip.PrimitiveGroup currentPrim = *(prims + i);
                                Vector3[] prs = mesh.vertices;
                                Vector3[] normalss = mesh.normals;
                                Vector2[] uvss = mesh.uv;
                                //Color32[] colorss = input.colors32;
                                List<Vector3> pr = new List<Vector3>();
                                List<Vector3> normals = new List<Vector3>();
                                List<Vector2> uvs = new List<Vector2>();
                                for (int j = 0; j != currentPrim.numIndices; j++)
                                {
                                    int index = (int)*(currentPrim.indices + j);
                                    pr.Add(prs[index]);
                                    normals.Add(normalss[index]);
                                    uvs.Add(uvss[index]);
                                    //colors.Add(colorss[index]);
                                }
                                for (int j = pr.Count - 1; j > -1; j--)
                                {
                                    pr2.Add(pr[j]);
                                    normals2.Add(normals[j]);
                                    uvs2.Add(uvs[j]);
                                }
                            }
                        }
                        finally
                        {
                            NvTriStrip.DeletePrimitives(prims);
                        }
                    }
                }

                MeshPart.VertexInfo[] verts = new MeshPart.VertexInfo[pr2.Count];
                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 v = pr2[i] * 500.0f;
                    verts[i] = new MeshPart.VertexInfo(new Vector3(v.x, -v.y, v.z), normals2[i], uvs2[i], (Color32)Color.white);
                }
                part.vertices = verts;
                subsubgroup0.parts = new MeshPart[] { part };
                subgroup0.subsubs = new SubSubMeshGroup[] { subsubgroup0 };
                group0.subs = new SubMeshGroup[] { subgroup0 };
                meshGroups = new MeshGroup[] { group0 };
            }

            {
                TextureGroup.Texture tex = textureGroup.textures[0];
                tex.pixels = texture.GetRawTextureData();
                textureGroup.textures = new TextureGroup.Texture[] { tex };
            }

            {
                int currentOffset = 0;
                currentOffset += Marshal.SizeOf<Header>();
                for (int i = 0; i < transforms.Length; i++)
                {
                    currentOffset += Marshal.SizeOf<ObjectTransform>();
                }

                mainHeader.meshPartLocalTexOffset = currentOffset;
                mainHeader.meshPartGBTexOffset = currentOffset;
                for (int i = 0; i < meshGroups.Length; i++)
                {
                    ref MeshGroup group = ref meshGroups[i];
                    group.header.nextGroupOffset = 0;
                    group.header.textureGroup = 3;
                    group.header.textureIndex = 0;
                    group.header.totalLength = Marshal.SizeOf<MeshGroup>();

                    for (int j = 0; j < group.subs.Length; j++)
                    {
                        ref SubMeshGroup subgroup = ref group.subs[j];
                        subgroup.header.nextSubMeshGroupOffset = 0;
                        subgroup.header.totalLength = Marshal.SizeOf<SubMeshGroup.Header>();
                        subgroup.header.subMeshIndex = 0;

                        for (int k = 0; k < subgroup.subsubs.Length; k++)
                        {
                            ref SubSubMeshGroup subsubgroup = ref subgroup.subsubs[k];
                            subsubgroup.header.nextSubSubMeshGroupOffset = 0;
                            subsubgroup.header.totalLength = Marshal.SizeOf<SubSubMeshGroup.Header>();
                            subsubgroup.header.illuminationType = 0;

                            for (int l = 0; l < subsubgroup.parts.Length; l++)
                            {
                                ref MeshPart part = ref subsubgroup.parts[l];
                                part.header.objectType = 1;
                                part.header.nextMeshPartOffset = 0;
                                part.header.vertexCount = part.vertices.Length;
                                part.header.totalLength = Marshal.SizeOf<MeshPart.Header>();

                                part.header.totalLength += Marshal.SizeOf<MeshPart.VertexInfo>() * part.vertices.Length;
                                subsubgroup.header.totalLength += part.header.totalLength;
                            }
                            subgroup.header.totalLength += subsubgroup.header.totalLength;
                        }
                        group.header.totalLength += subgroup.header.totalLength;
                    }

                    currentOffset += group.header.totalLength;
                }

                Shared.Util.AlignOffsetToNext(ref currentOffset);

                mainHeader.interestPointsOffset = currentOffset;
                currentOffset += (Marshal.SizeOf<Matrix4x4>() * (interestPoints.Length + 1)) + 0x20;
                mainHeader.textureGroupOffset = currentOffset;
                mainHeader.textureGroupOffset2__ = currentOffset;
                mainHeader.meshPartCount = 1;
                mainHeader.meshPartGBTexCount = 0;
                mainHeader.meshPartLocalTexCount = 1;

                textureGroup.header.textureCount = 1;
                textureGroup.header.totalLength = Marshal.SizeOf<TextureGroup.Header>();
                for (int i = 0; i < textureGroup.textures.Length; i++)
                {
                    ref TextureGroup.Texture tex = ref textureGroup.textures[i];
                    tex.header.bitsPerPixel = 32;
                    tex.header.pixelsLength = tex.pixels.Length;
                    tex.header.textureHeight = (short)texture.height;
                    tex.header.textureWidth = (short)texture.width;
                    tex.header.pixelsLength = tex.pixels.Length * Marshal.SizeOf<ColorBGRA>();
                    tex.header.totalLength = Marshal.SizeOf<TextureGroup.Texture.Header>() + tex.header.bufferSizeAfterHeader;
                    tex.header.totalLength += tex.pixels.Length * Marshal.SizeOf<ColorBGRA>();
                    textureGroup.header.totalLength += tex.header.totalLength;
                }
            }
        }

        private static ObjectTransform[] ReadObjectTransforms(BinaryReader reader)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadObjectTransforms");

            CollectionPool.Request(out List<ObjectTransform> objectTransforms);
            ObjectTransform objectTransform;
            do
            {
                objectTransform = reader.ReadStruct<ObjectTransform>();
                objectTransforms.Add(objectTransform);
            } while (objectTransform.nextObjectTransformOffset != 0);

            ObjectTransform[] result = objectTransforms.ToArray();
            CollectionPool.Return(ref objectTransforms);

            UnityEngine.Profiling.Profiler.EndSample();
            return result;
        }

        private static MeshGroup[] ReadMeshGroups(BinaryReader reader)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadMeshGroup");

            CollectionPool.Request(out List<MeshGroup> meshgroups);
            MeshGroup meshgroup;
            do
            {
                meshgroup = new MeshGroup();
                meshgroup.header = reader.ReadStruct<MeshGroup.Header>();
                meshgroup.subs = ReadSubMeshGroups(reader);

                meshgroups.Add(meshgroup);
                reader.BaseStream.Position = meshgroup.header.nextGroupOffset;
            } while (meshgroup.header.nextGroupOffset != 0);

            MeshGroup[] result = meshgroups.ToArray();
            CollectionPool.Return(ref meshgroups);

            UnityEngine.Profiling.Profiler.EndSample();
            return result;
        }

        private static SubMeshGroup[] ReadSubMeshGroups(BinaryReader reader)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadSubMeshGroup");

            CollectionPool.Request(out List<SubMeshGroup> submeshgroups);
            SubMeshGroup submeshgroup;
            do
            {
                submeshgroup = new SubMeshGroup();
                submeshgroup.header = reader.ReadStruct<SubMeshGroup.Header>();
                submeshgroup.subsubs = ReadSubSubMeshGroups(reader);

                submeshgroups.Add(submeshgroup);
                reader.BaseStream.Position = submeshgroup.header.nextSubMeshGroupOffset;
            } while (submeshgroup.header.nextSubMeshGroupOffset != 0);

            SubMeshGroup[] result = submeshgroups.ToArray();
            CollectionPool.Return(ref submeshgroups);

            UnityEngine.Profiling.Profiler.EndSample();
            return result;
        }

        private static SubSubMeshGroup[] ReadSubSubMeshGroups(BinaryReader reader)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadSubSubMeshGroup");

            CollectionPool.Request(out List<SubSubMeshGroup> subsubmeshgroups);
            SubSubMeshGroup subsubmeshgroup;
            do
            {
                subsubmeshgroup = new SubSubMeshGroup();
                subsubmeshgroup.header = reader.ReadStruct<SubSubMeshGroup.Header>();
                subsubmeshgroup.parts = ReadMeshParts(reader);

                subsubmeshgroups.Add(subsubmeshgroup);
                reader.BaseStream.Position = subsubmeshgroup.header.nextSubSubMeshGroupOffset;
            } while (subsubmeshgroup.header.nextSubSubMeshGroupOffset != 0);

            SubSubMeshGroup[] result = subsubmeshgroups.ToArray();
            CollectionPool.Return(ref subsubmeshgroups);

            UnityEngine.Profiling.Profiler.EndSample();
            return result;
        }

        private static MeshPart[] ReadMeshParts(BinaryReader reader)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadMeshPart");

            CollectionPool.Request(out List<MeshPart> meshparts);
            MeshPart meshpart;
            do
            {
                meshpart = new MeshPart();
                meshpart.header = reader.ReadStruct<MeshPart.Header>();
                meshpart.vertices = new MeshPart.VertexInfo[meshpart.header.vertexCount];
                reader.ReadStruct<MeshPart.VertexInfo>(meshpart.vertices);

                if (meshpart.header.unknownData != 0)
                {
                    meshpart.extraData = new MeshPart.ExtraData[8]; //No idea of the proper way to count
                    reader.ReadStruct<MeshPart.ExtraData>(meshpart.extraData);
                }

                meshparts.Add(meshpart);
                reader.BaseStream.Position = meshpart.header.nextMeshPartOffset;
            } while (meshpart.header.nextMeshPartOffset != 0);

            MeshPart[] result = meshparts.ToArray();
            CollectionPool.Return(ref meshparts);

            UnityEngine.Profiling.Profiler.EndSample();
            return result;
        }

        private static Matrix4x4[] ReadInterestPoints(BinaryReader reader, in MapGeometry.Header header)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadInterestPoints");

            long rangeEnd = reader.BaseStream.Length;
            if(header.lightDecalsOffset != 0)
            {
                rangeEnd = header.lightDecalsOffset;
            }
            else if(header.textureGroupOffset != 0)
            {
                rangeEnd = header.textureGroupOffset;
            }

            int count = (int)((rangeEnd - reader.BaseStream.Position) / (sizeof(float) * 16));

            Matrix4x4[] matrices = new Matrix4x4[count];
            for (int i = 0; i < matrices.Length; i++)
            {
                matrices[i] = reader.ReadStruct<Matrix4x4>();
            }

            UnityEngine.Profiling.Profiler.EndSample();
            return matrices;
        }

        private static LightDecal ReadLightData(BinaryReader reader)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadLightData");

            LightDecal lightData = new LightDecal();
            lightData.header = reader.ReadStruct<LightDecal.Header>();
            
            if(lightData.header.firstEntryOffset != 0)
            {
                CollectionPool.Request(out List<LightDecal.Entry> entries);
                reader.BaseStream.Position = lightData.header.firstEntryOffset;

                LightDecal.Entry entry;
                do
                {
                    entry = new LightDecal.Entry();
                    entry.header = reader.ReadStruct<LightDecal.Entry.Header>();

                    if (entry.header.offsetToPositions != 0)
                    {
                        entry.instances = new Vector4[entry.header.positionsCount];
                        reader.BaseStream.Position = entry.header.offsetToPositions;
                        for (int i = 0; i < entry.header.positionsCount; i++)
                        {
                            entry.instances[i] = reader.ReadVector4();
                        }
                    }

                    if (entry.header.offsetToData != 0)
                    {
                        entry.instanceData = new LightDecal.Entry.VertexData[entry.header.dataCount];
                        reader.BaseStream.Position = entry.header.offsetToData;
                        for (int i = 0; i < entry.header.dataCount; i++)
                        {
                            entry.instanceData[i] = reader.ReadStruct<LightDecal.Entry.VertexData>();
                        }
                    }

                    entries.Add(entry);
                    reader.BaseStream.Position = entry.header.offsetToNextEntry;
                } while (entry.header.offsetToNextEntry != 0);

                lightData.entries = entries.ToArray();
                CollectionPool.Return(ref entries);
            }

            UnityEngine.Profiling.Profiler.EndSample();
            return lightData;
        }

        public void ReadFile(BinaryReader reader)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReadFile");

            mainHeader = reader.ReadStruct<Header>();
            transforms = ReadObjectTransforms(reader);
            meshGroups = ReadMeshGroups(reader);

            if (mainHeader.interestPointsOffset != 0)
            {
                reader.BaseStream.Position = mainHeader.interestPointsOffset;
                interestPoints = ReadInterestPoints(reader, mainHeader);
            }

            if (mainHeader.lightDecalsOffset != 0)
            {
                reader.BaseStream.Position = mainHeader.lightDecalsOffset;
                lightDecal = ReadLightData(reader);
            }

            if (mainHeader.textureGroupOffset != 0)
            {
                reader.BaseStream.Position = mainHeader.textureGroupOffset;
                textureGroup = TextureGroup.ReadTextureGroup(reader);
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void WriteFile(BinaryWriter writer)
        {
            writer.WriteStruct(in mainHeader);

            for (int i = 0; i < transforms.Length; i++)
            {
                writer.WriteStruct(in transforms[i]);
            }

            for (int i = 0; i != meshGroups.Length; i++)
            {
                MeshGroup group = meshGroups[i];
                writer.WriteStruct(group.header);
                for (int j = 0; j < group.subs.Length; j++)
                {
                    SubMeshGroup submesh = group.subs[j];
                    writer.WriteStruct(submesh.header);
                    for (int k = 0; k < submesh.subsubs.Length; k++)
                    {
                        SubSubMeshGroup subsubmesh = submesh.subsubs[k];
                        writer.WriteStruct(subsubmesh.header);
                        for (int l = 0; l < subsubmesh.parts.Length; l++)
                        {
                            MeshPart part = subsubmesh.parts[l];
                            writer.WriteStruct(part.header);
                            for (int m = 0; m < part.vertices.Length; m++)
                            {
                                writer.WriteStruct(part.vertices[m]);
                            }
                        }
                    }
                }
            }

            writer.BaseStream.Position = mainHeader.interestPointsOffset;
            for (int i = 0; i < interestPoints.Length; i++)
            {
                writer.WriteStruct(interestPoints[i]);
            }

            writer.BaseStream.Position = mainHeader.textureGroupOffset;
            writer.WriteStruct(textureGroup.header);
            for (int i = 0; i < textureGroup.header.textureCount; i++)
            {
                TextureGroup.Texture tex = textureGroup.textures[i];
                writer.WriteStruct(tex.header);
                for (int j = 0; j < tex.header.bufferSizeAfterHeader; j++)
                {
                    writer.Write((byte)0x00);
                }
                for (int j = 0; j < tex.header.pixelsLength / 0x04; j++)
                {
                    writer.WriteStruct(tex.pixels[j]);
                }
            }
        }
    }
}
