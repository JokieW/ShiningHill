using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;
using System.Runtime.InteropServices;

namespace ShiningHill
{
    public class MapFile
    {
        public Header mainHeader;
        public Skybox__[] skyboxes;
        public MeshGroup[] meshGroups;
        public Matrix4x4[] eventMatrices;
        public MaterialRolodex.TextureGroup textureGroup;

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
                MaterialRolodex.TextureGroup.Texture tex = textureGroup.textures[0];
                tex.pixels = texture.GetPixels32();
                textureGroup.textures = new MaterialRolodex.TextureGroup.Texture[] { tex };
            }

            {
                int currentOffset = 0;
                currentOffset += Marshal.SizeOf<Header>();
                for (int i = 0; i < skyboxes.Length; i++)
                {
                    currentOffset += Marshal.SizeOf<Skybox__>();
                }

                mainHeader.sceneStartHeaderOffset__ = currentOffset;
                mainHeader.firstMeshGroupOffset = currentOffset;
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

                DataUtils.AlignOffsetToNext(ref currentOffset);

                mainHeader.eventMatricesOffset = currentOffset;
                currentOffset += (Marshal.SizeOf<Matrix4x4>() * (eventMatrices.Length + 1)) + 0x20;
                mainHeader.textureGroupOffset = currentOffset;
                mainHeader.textureGroupOffset2__ = currentOffset;
                mainHeader.totalTextures = 1;
                mainHeader.localTextureBaseIndex = 0;
                mainHeader.localTextureCount = 1;

                textureGroup.header.textureCount = 1;
                textureGroup.header.totalLength = Marshal.SizeOf<MaterialRolodex.TextureGroup.Header>();
                for (int i = 0; i < textureGroup.textures.Length; i++)
                {
                    ref MaterialRolodex.TextureGroup.Texture tex = ref textureGroup.textures[i];
                    tex.header.bitsPerPixel = 32;
                    tex.header.pixelsLength = tex.pixels.Length;
                    tex.header.textureHeight = (short)texture.height;
                    tex.header.textureWidth = (short)texture.width;
                    tex.header.pixelsLength = tex.pixels.Length * Marshal.SizeOf<ColorBGRA>();
                    tex.header.totalLength = Marshal.SizeOf<MaterialRolodex.TextureGroup.Texture.Header>() + tex.header.bufferSizeAfterHeader;
                    tex.header.totalLength += tex.pixels.Length * Marshal.SizeOf<ColorBGRA>();
                    textureGroup.header.totalLength += tex.header.totalLength;
                }
            }
        }

        public MapFile(BinaryReader reader)
        {
            UnityEngine.Profiling.Profiler.BeginSample("New MapFile");
            mainHeader = reader.ReadStruct<Header>();

            {
                UnityEngine.Profiling.Profiler.BeginSample("Skyboxes");
                List<Skybox__> skyboxes = new List<Skybox__>(1);
                Skybox__ skybox;
                do
                {
                    skybox = reader.ReadStruct<Skybox__>();
                    skyboxes.Add(skybox);
                } while (skybox.nextSkyboxOffset != 0);
                this.skyboxes = skyboxes.ToArray();
                UnityEngine.Profiling.Profiler.EndSample();
            }

            {
                List<MeshGroup> meshgroups = new List<MeshGroup>();
                MeshGroup meshgroup;
                do
                {
                    UnityEngine.Profiling.Profiler.BeginSample("meshgroup");
                    meshgroup = new MeshGroup();
                    meshgroup.header = reader.ReadStruct<MeshGroup.Header>();

                    List<SubMeshGroup> submeshgroups = new List<SubMeshGroup>();
                    SubMeshGroup submeshgroup;
                    do
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("submeshgroup");
                        submeshgroup = new SubMeshGroup();
                        submeshgroup.header = reader.ReadStruct<SubMeshGroup.Header>();

                        List<SubSubMeshGroup> subsubmeshgroups = new List<SubSubMeshGroup>();
                        SubSubMeshGroup subsubmeshgroup;
                        do
                        {
                            UnityEngine.Profiling.Profiler.BeginSample("subsubmeshgroup");
                            subsubmeshgroup = new SubSubMeshGroup();
                            subsubmeshgroup.header = reader.ReadStruct<SubSubMeshGroup.Header>();

                            List<MeshPart> meshparts = new List<MeshPart>();
                            MeshPart meshpart;
                            do
                            {
                                UnityEngine.Profiling.Profiler.BeginSample("meshpart");
                                try
                                {
                                    meshpart = new MeshPart();
                                    UnityEngine.Profiling.Profiler.BeginSample("reader.ReadStruct<MeshPart.Header>");
                                    meshpart.header = reader.ReadStruct<MeshPart.Header>();
                                    UnityEngine.Profiling.Profiler.EndSample();
                                    UnityEngine.Profiling.Profiler.BeginSample("new MeshPart.VertexInfo");
                                    meshpart.vertices = new MeshPart.VertexInfo[meshpart.header.vertexCount];
                                    UnityEngine.Profiling.Profiler.EndSample();
                                    UnityEngine.Profiling.Profiler.BeginSample("for (int i");
                                    reader.ReadStruct<MeshPart.VertexInfo>(meshpart.vertices);
                                    UnityEngine.Profiling.Profiler.EndSample();

                                    UnityEngine.Profiling.Profiler.BeginSample("Add(meshpart)");
                                    meshparts.Add(meshpart);
                                    UnityEngine.Profiling.Profiler.EndSample();
                                }
                                catch(Exception e)
                                {
                                    throw;
                                }

                                UnityEngine.Profiling.Profiler.EndSample();
                            } while (meshpart.header.nextMeshPartOffset != 0);
                            subsubmeshgroup.parts = meshparts.ToArray();
                            subsubmeshgroups.Add(subsubmeshgroup);

                            UnityEngine.Profiling.Profiler.EndSample();
                        } while (subsubmeshgroup.header.nextSubSubMeshGroupOffset != 0);
                        submeshgroup.subsubs = subsubmeshgroups.ToArray();
                        submeshgroups.Add(submeshgroup);

                        UnityEngine.Profiling.Profiler.EndSample();
                    } while (submeshgroup.header.nextSubMeshGroupOffset != 0);
                    meshgroup.subs = submeshgroups.ToArray();
                    meshgroups.Add(meshgroup);

                    UnityEngine.Profiling.Profiler.EndSample();
                } while (meshgroup.header.nextGroupOffset != 0);
                meshGroups = meshgroups.ToArray();
            }

            reader.BaseStream.Position = mainHeader.eventMatricesOffset;
            {
                UnityEngine.Profiling.Profiler.BeginSample("matrices");
                List<Matrix4x4> matrices = new List<Matrix4x4>();
                Matrix4x4 matrix = default;
                while ((matrix = reader.ReadStruct<Matrix4x4>()) != default)
                {
                    matrices.Add(matrix);
                }
                eventMatrices = matrices.ToArray();
                UnityEngine.Profiling.Profiler.EndSample();
            }

            UnityEngine.Profiling.Profiler.BeginSample("ReadTextureGroup");
            reader.BaseStream.Position = mainHeader.textureGroupOffset;
            SH3MaterialRolodex.ReadTextureGroup(reader, out textureGroup);
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void Write(BinaryWriter writer)
        {
            writer.WriteStruct(in mainHeader);

            for (int i = 0; i < skyboxes.Length; i++)
            {
                writer.WriteStruct(in skyboxes[i]);
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

            writer.BaseStream.Position = mainHeader.eventMatricesOffset;
            for (int i = 0; i < eventMatrices.Length; i++)
            {
                writer.WriteStruct(eventMatrices[i]);
            }

            writer.BaseStream.Position = mainHeader.textureGroupOffset;
            writer.WriteStruct(textureGroup.header);
            for (int i = 0; i < textureGroup.header.textureCount; i++)
            {
                MaterialRolodex.TextureGroup.Texture tex = textureGroup.textures[i];
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


        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            public int int_00; // -1 0xFFFFFFFF
            public int field_04;
            public int field_08;
            public int mainHeaderSize;

            public int textureGroupOffset;
            public int field_14;
            public int mainHeaderSize2__;
            public int firstMeshGroupOffset;

            public int field_20;
            public int sceneStartHeaderOffset__;
            public int field_28;
            public int field_2C;

            public int textureGroupOffset2__;
            public int eventMatricesOffset;
            public int field_38;
            public int field_3C;

            public short totalTextures;
            public short localTextureBaseIndex;
            public short localTextureCount;
            public short localTextureBaseIndexModifier;
            public int field_48;
            public int field_4C;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Skybox__
        {
            public int nextSkyboxOffset; //0 if no more
            public int headerLength; //32 for skybox
            public int skyboxLength;
            public int field_0C;

            public int field_10;
            public int field_14;
            public int field_18;
            public int field_1C;

            public Matrix4x4 matrix;
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

        public struct MeshGroup
        {
            public Header header;
            public SubMeshGroup[] subs;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                public int nextGroupOffset; //0 when done
                public int headerLength;
                public int totalLength; //from offset of MeshGroup, to nextGroupOffset
                public int field_0C;

                public int textureGroup; //3 local, 2 TR, 1 GB
                public int textureIndex;
                public int subMeshGroupCount;
                public int field_1C;

                public int pad_20;
                public int pad_24;
                public int pad_28;
                public int pad_2C;
            }
        }

        public struct SubMeshGroup
        {
            public Header header;
            public SubSubMeshGroup[] subsubs;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                public int nextSubMeshGroupOffset;
                public int headerLength;
                public int totalLength;
                public int field_0C;

                public short subMeshIndex;
                public short subSubMeshCount__;
                public short field_14;
                public short transparencyType; //0 opaque, 1 transparent, 3 cutout, 8 unknown
                public int field_18;
                public int field_1C;

                public int pad_20;
                public int pad_24;
                public int pad_28;
                public int pad_2C;
            }
        }

        public struct SubSubMeshGroup
        {
            public Header header;
            public MeshPart[] parts;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                public int nextSubSubMeshGroupOffset;
                public int headerLength;
                public int totalLength;
                public int field_0C;

                public int illuminationType; //9 ambient?, 8 self-illum, 4 unknown (i.e. bu1f), 0 no illum?, 265 trnasparent mirrors
                public int field_14;
                public int field_18;
                public int field_1C;

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

        public struct MeshPart
        {
            public Header header;
            public VertexInfo[] vertices;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                public int nextMeshPartOffset;
                public int headerLength;
                public int totalLength;
                public int field_0C;

                public int vertexCount;
                public int objectType; //1 = static, 2 = can be or not there, 3 = can move
                public int occlusionGroup;
                public int meshFlags;

                public float float_20;
                public float float_24;
                public float float_28;
                public float float_2C;

                public float float_30;
                public float float_34;
                public float float_38;
                public float float_3C;
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
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct ColorBGRA
        {
            public byte blue;
            public byte green;
            public byte red;
            public byte alpha;

            public ColorBGRA(byte b, byte g, byte r, byte a)
            {
                blue = b;
                green = g;
                red = r;
                alpha = a;
            }

            public static implicit operator Color32(ColorBGRA color)
            {
                return new Color32(color.red, color.green, color.blue, color.alpha);
            }

            public static implicit operator ColorBGRA(Color32 color)
            {
                return new ColorBGRA(color.b, color.g, color.r, color.a);
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct ColorRGBA5551
        {
            public byte one;
            public byte two;

            public static implicit operator Color32(ColorRGBA5551 color)
            {
                //Thanks de_lof
                int r = (color.two & 0x7c) << 1;
                int g = ((color.two & 0x03) << 6) | ((color.one & 0xe0) >> 2);
                int b = (color.one & 0x1f) << 3;
                int a = (color.two & 0x80) != 0 ? 255 : 0;
                r |= r >> 5;
                g |= g >> 5;
                b |= b >> 5;
                return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            }
        }

        public enum MaterialType
        {
            Diffuse = 0,
            Cutout = 1,
            Transparent = 2,
            SelfIllum = 3,

            __Count = 4
        }

        public static MaterialType SH3MaterialToType(in SubMeshGroup subMeshGroup, in SubSubMeshGroup subSubMeshGroup)
        {
            if (subMeshGroup.header.transparencyType == 1)
            {
                return MaterialType.Transparent;
            }
            else if (subMeshGroup.header.transparencyType == 3)
            {
                return MaterialType.Cutout;
            }
            else if (subSubMeshGroup.header.illuminationType == 8)
            {
                return MaterialType.SelfIllum;
            }
            else
            {
                return MaterialType.Diffuse;
            }
        }
    }

    public class Map : MonoBehaviour 
    {
        public MapFile.Header header;
        public Matrix4x4[] eventMatrices;

        //delete
#if false
        public static Map ReadMap(MapAssetPaths asset)
        {
            GameObject subGO = Scene.BeginEditingPrefab(asset.GetPrefabPath(), "Map");

            try
            {
                Map scene = subGO.AddComponent<Map>();

                BinaryReader reader = new BinaryReader(new FileStream(asset.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));

                //Header
                int marker = reader.PeekInt32(); //marker

                if (marker != -1) //SH2
                {
                    ReadSH2Map(reader, scene, asset);
                    Scene.FinishEditingPrefab(asset.GetPrefabPath(), subGO);
                }
                else if (marker == -1) //SH3
                {
                    ReadSH3Map(reader, scene, asset);
                    Scene.FinishEditingPrefab(asset.GetPrefabPath(), subGO);
                }

                return scene;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }
#endif

        //fix with new pipeline
#if false
        static void ReadSH2Map(BinaryReader reader, Map scene, MapAssetPaths paths)
        {
            GameObject subGO = scene.gameObject;

            /*int fileID = */reader.ReadInt32();
            /*int fileSize = */reader.ReadInt32();
            /*int Unknown1 = */reader.ReadInt32();
            reader.SkipInt32(0);

            //Textures
            Texture[] textures = TextureUtils.ReadDDS(paths.GetTextureName(), reader);


            //Meshes
            reader.SkipInt32(1);
            reader.SkipInt32(); //Length from magic to bottom
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            long magicPosition = reader.BaseStream.Position;
            reader.SkipInt32(0x20010730);
            reader.SkipInt32(1);
            int materialsOffset = reader.ReadInt32() + (int)magicPosition; 
            int meshCount = reader.ReadInt32();

            reader.SkipInt32(0);
            reader.SkipInt32(); // Length of elements from 0^
            reader.SkipInt32(20);
            reader.SkipInt32(0);

            reader.SkipInt32(0);
            reader.SkipInt32(1);
            reader.SkipInt32(8);
            
            long v1offset = reader.BaseStream.Position;
            reader.ReadVector3(); //V1
            reader.SkipInt32(0);
            reader.ReadVector3(); //V2
            reader.SkipInt32(0);
            /*int headerLength = */reader.ReadInt32(); //From v1 to vertexLength

            int indicesOffset = reader.ReadInt32() + (int)v1offset;
            /*int indicesLength = */reader.ReadInt32();
            /*int Unknown = */reader.ReadInt32();
            reader.SkipInt32(meshCount);

            List<MeshGroupSH2> groups = new List<MeshGroupSH2>();
            for (int i = 0; i != meshCount; i++)
            {
                groups.Add(MeshGroupSH2.Initialise(reader, subGO));
            }

            int vertexLength = reader.ReadInt32();
            reader.SkipInt32(1);
            reader.SkipInt32(0);
            int elementLength = reader.ReadInt32();
            reader.SkipInt32(vertexLength);

            int vertexElementsCount = vertexLength / elementLength;
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Color32> colors = new List<Color32>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i != vertexElementsCount; i++)
            {
                verts.Add(reader.ReadVector3());
                norms.Add(reader.ReadVector3());
                if (elementLength == 36)
                {
                    colors.Add(reader.ReadBGRA());
                }
                uvs.Add(reader.ReadVector2());
            }

            reader.BaseStream.Position = indicesOffset;

            List<short[]> indices = new List<short[]>(groups.Count);

            //stupid
            for (int i = 0; i != groups.Count; i++)
            {
                indices.Add(null);
            }

            for(int i = 0; i != groups.Count; i++)
            {
                MeshGroupSH2 group = groups[i];
                indices[group.MainID] = new short[group.indexCount];
                for (int j = 0; j != group.indexCount ; j++)
                {
                    indices[group.MainID][j] = reader.ReadInt16();
                }
                Debug.Log("End of i = " + reader.BaseStream.Position.ToString("X"));
            }

            reader.BaseStream.Position = materialsOffset;

            //Mesh renderer
            MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(paths.GetExtractAssetPath());
            rolodex.AddTextures(textures);
            

            MeshRenderer mr = subGO.AddComponent<MeshRenderer>();
            Material[] mats = new Material[groups.Count];
            for (int i = 0; i != groups.Count; i++)
            {
                reader.SkipInt16();
                MaterialRolodex.TexMatsPair tmp = rolodex.GetWithSH2ID(reader.ReadInt16());
                mats[i] = tmp.GetOrCreateDiffuse();
                reader.SkipBytes(12);
            }
            mr.sharedMaterials = mats;

            reader.Close();

            //Mesh filter
            subGO.AddComponent<MeshFilter>().sharedMesh = MeshUtils.MakeIndexedStrip(verts, indices, norms, uvs, colors);

            foreach (MeshFilter mf in subGO.GetComponentsInChildren<MeshFilter>())
            {
                AssetDatabase.AddObjectToAsset(mf.sharedMesh, paths.GetExtractAssetPath());
            }
        }
#endif

        //delete
#if false
        static void ReadSH3Map(BinaryReader reader, Map scene, MapAssetPaths paths)
        {
            GameObject subGO = scene.gameObject;

            reader.SkipInt32(-1);
            reader.SkipInt32(0);
            reader.SkipInt32(0);
            reader.SkipInt32(80); //Main header size

            int TextureGroupOffset = reader.ReadInt32();
            reader.SkipInt32(0);
            reader.SkipInt32(80); //Alt main header size
            reader.SkipInt32(); //Total main header size

            scene.Unknown1 = reader.ReadInt32();
            reader.SkipInt32(); //Scene star header offset
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            reader.SkipInt32(); //TextureGroupOffset2
            /*int transformOffset = */
            reader.ReadInt32();
            scene.Unknown2 = reader.ReadInt32();
            reader.SkipInt32(0);

            scene.TotalTextures = reader.ReadInt16();
            scene.LocalTextureBaseIndex = reader.ReadInt16();
            scene.LocalTextureCount = reader.ReadInt16();
            scene.LocalTextureBaseIndexModifier = reader.ReadInt16();
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            //Read textures
            long goBack = reader.BaseStream.Position;
            reader.BaseStream.Position = TextureGroupOffset;
            Texture2D[] textures = TextureUtils.ReadTex32(paths.GetTextureName(), reader);

            reader.BaseStream.Position = goBack;

            //Read Skyboxes
            Skybox sky = null;
            do
            {
                sky = Skybox.Deserialise(reader, subGO);
            } while (sky.NextSkyboxOffset != 0);

            //Read meshgroups
            int next;
            do
            {
                next = MeshGroup.Deserialise(reader, subGO);
            } while (next != 0);

            //reader.BaseStream.Position = transformOffset;
            //Matrix4x4 mat4x4 = reader.ReadMatrix4x4();
            Matrix4x4Utils.SetTransformFromMatrix(subGO.transform, ref subGO.GetComponentInChildren<Skybox>().Matrix);

            reader.Close();

            //Asset bookkeeping
            MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(paths.GetExtractAssetPath());
            rolodex.AddTextures(textures);

            int baseIndex = 0;

            MeshGroup[] groups = subGO.GetComponentsInChildren<MeshGroup>();
            foreach (MeshGroup group in groups)
            {
                MaterialRolodex goodRolodex = null;
                if (group.TextureGroup == 3)
                {
                    goodRolodex = rolodex;
                    baseIndex = scene.LocalTextureBaseIndex + scene.LocalTextureBaseIndexModifier;
                }
                else if (group.TextureGroup == 2)
                {
                    goodRolodex = CustomPostprocessor.ProcessTEX(paths.GetHardTextureTRPaths());
                }
                else if (group.TextureGroup == 1)
                {
                    goodRolodex = CustomPostprocessor.ProcessTEX(paths.GetHardTextureGBPaths());
                }
                else
                {
                    Debug.LogWarning("Unknown texture group " + group.TextureGroup + " on " + group.gameObject);
                }

                if (goodRolodex == null)
                {
                    Debug.LogWarning("Couldn't find rolodex for group " + group.TextureGroup + " on " + paths.GetHardAssetPath());
                    continue;
                }

                MaterialRolodex.TexMatsPair tmp = goodRolodex.GetWithSH3Index(group.TextureIndex, baseIndex);
                foreach (SubMeshGroup subMeshGroup in group.GetComponentsInChildren<SubMeshGroup>())
                {
                    foreach (SubSubMeshGroup subSubMeshGroup in subMeshGroup.GetComponentsInChildren<SubSubMeshGroup>())
                    {
                        foreach (MeshRenderer renderer in subSubMeshGroup.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (subMeshGroup.IsTransparent == 1)
                            {
                                renderer.sharedMaterial = tmp.GetOrCreateTransparent();
                            }
                            else if (subMeshGroup.IsTransparent == 3)
                            {
                                renderer.sharedMaterial = tmp.GetOrCreateCutout();
                            }
                            else if (subSubMeshGroup.Illumination == 8)
                            {
                                renderer.sharedMaterial = tmp.GetOrCreateSelfIllum();
                            }
                            else
                            {
                                renderer.sharedMaterial = tmp.GetOrCreateDiffuse();
                            }
                        }
                    }
                }
            }

            foreach (MeshFilter mf in subGO.GetComponentsInChildren<MeshFilter>())
            {
                AssetDatabase.AddObjectToAsset(mf.sharedMesh, paths.GetExtractAssetPath());
            }
        }
#endif
    }
}