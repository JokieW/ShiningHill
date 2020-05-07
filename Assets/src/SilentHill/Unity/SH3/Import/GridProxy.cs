using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

using SH.DataFormat.SH3;
using SH.Unity.Shared;
using SH.DataFormat.Shared;
using SH.Core;

namespace SH.Unity.SH3
{
    [CustomEditor(typeof(GridProxy))]
    [CanEditMultipleObjects]
    public class GridProxyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            //Unpack
            if (GUILayout.Button("Unpack"))
            {
                try
                {
                    AssetUtil.StartAssetEditing();
                    for (int i = 0; i < targets.Length; i++)
                    {
                        GridProxy proxy = (GridProxy)targets[i];
                    }
                }
                finally
                {
                    AssetUtil.StopAssetEditing();
                }
            }

            //Pack
            if (GUILayout.Button("Pack"))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    GridProxy proxy = (GridProxy)targets[i];
                    proxy.Pack();
                }
            }
        }
    }

    public class GridProxy : BaseImportProxy
    {
        public LevelProxy level;
        public string gridName;
        public UnityEngine.Object map;
        public UnityEngine.Object cam;
        public UnityEngine.Object cld;
        public UnityEngine.Object kg2;
        public UnityEngine.Object ded;
        public UnityEngine.Object TRtex;

        public GameObject prefab;
        public MaterialRolodex localTextures;
        public MaterialRolodex TRTextures;

        /*
         * string path = CustomPostprocessor.GetHardDataPathFor(game);
                if (path.Contains("cc/cc")) // Done for SH3, Check for SH2
                {
                    return new TexAssetPaths(path + genericPath + "cc01TR.tex", game);
                }
                return new TexAssetPaths(path + genericPath + mapName + "TR.tex", game);
                */

        public string fullName
        {
            get => level.levelName + gridName;
        }

        public MaterialRolodex GetTextureGroup(int group)
        {
            if (group == 3) return localTextures;
            if (group == 2) return TRTextures;
            if (group == 1) return level.GBTextures;
            throw new System.InvalidOperationException("Unknown texture group " + group);
        }

        public void MakePrefab()
        {
            MakePrefab(null);
        }

        public void MakePrefab(GameObject map)
        {
            UnpackPath dest = UnpackPath.GetDirectory(this).AddToPath("scene/").WithDirectoryAndName(UnpackDirectory.Unity, fullName + ".prefab", true);

            GameObject prefabinstance;
            if (dest.FileExists())
            {
                prefabinstance = PrefabUtility.LoadPrefabContents(dest);
                foreach (Transform child in prefabinstance.transform)
                {
                    if (map != null && child.name == "Geometry")
                    {
                        GameObject.DestroyImmediate(child.gameObject);
                        break;
                    }
                }
            }
            else
            {
                prefabinstance = new GameObject(fullName);
                prefabinstance.isStatic = true;
            }

            if (map != null)
            {
                map.transform.SetParent(prefabinstance.transform, true);
            }

            if (dest.FileExists())
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(prefabinstance, dest);
                PrefabUtility.UnloadPrefabContents(prefabinstance);
            }
            else
            {
                prefab = PrefabUtility.SaveAsPrefabAsset(prefabinstance, dest);
                DestroyImmediate(prefabinstance);
            }
            EditorUtility.IsDirty(this);
        }

        public void Unpack()
        {
            UnityEngine.Profiling.Profiler.BeginSample("UnpackGrid");
            MapGeometry mapFile = GetMapFile(this);
            UnpackMaterialRolodexes(this, mapFile);
            GameObject map = UnpackMap(this, mapFile);

            UnityEngine.Profiling.Profiler.BeginSample("MakePrefab");
            try
            {
                MakePrefab(map);
            }
            catch
            {
                GameObject.DestroyImmediate(map);
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.EndSample();
                throw;
            }
            UnityEngine.Profiling.Profiler.EndSample();
            EditorUtility.SetDirty(this);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void Pack()
        {

        }

        private static MapGeometry GetMapFile(GridProxy grid)
        {
            UnityEngine.Profiling.Profiler.BeginSample("GetMapFile");
            if (grid.map == null)
            {
                UnityEngine.Profiling.Profiler.EndSample();
                return null;
            }

            MapGeometry mapFile = null;
            using (FileStream file = new FileStream(UnpackPath.GetPath(grid.map), FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {
                mapFile = new MapGeometry(reader);
            }
            UnityEngine.Profiling.Profiler.EndSample();
            return mapFile;
        }

        private static void UnpackMaterialRolodexes(GridProxy grid, MapGeometry mapFile)
        {
            UnityEngine.Profiling.Profiler.BeginSample("GetMapFile");
            grid.localTextures = MaterialRolodex.CreateInstance<MaterialRolodex>();
            UnityEngine.Profiling.Profiler.BeginSample("CreateAsset1");
            AssetDatabase.CreateAsset(grid.localTextures, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mats.asset", true));
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.BeginSample("AddTextures");
            grid.localTextures.AddTextures(TextureUtil.ReadTex32(grid.fullName + "_tex_", in mapFile.textureGroup));
            UnityEngine.Profiling.Profiler.EndSample();

            if (grid.TRtex != null)
            {
                UnityEngine.Profiling.Profiler.BeginSample("_TRtex");
                TextureGroup trGroup;
                using (FileStream file = new FileStream(UnpackPath.GetPath(grid.TRtex), FileMode.Open, FileAccess.ReadWrite))
                using (BinaryReader reader = new BinaryReader(file))
                {
                    TextureGroup.ReadTextureGroup(reader, out trGroup);
                }
                grid.TRTextures = MaterialRolodex.CreateInstance<MaterialRolodex>();
                AssetDatabase.CreateAsset(grid.TRTextures, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "TR_mats.asset", true));
                grid.TRTextures.AddTextures(TextureUtil.ReadTex32(grid.fullName + "TR_tex_", in trGroup));
                UnityEngine.Profiling.Profiler.EndSample();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        private static GameObject UnpackMap(GridProxy grid, MapGeometry mapFile)
        {
            UnityEngine.Profiling.Profiler.BeginSample("UnpackMap");
            GameObject mapGo = new GameObject("Geometry");
            mapGo.isStatic = true;
            try
            {
                {
                    MapGeometryComponent m = mapGo.AddComponent<MapGeometryComponent>();
                    m.header = mapFile.mainHeader;
                    m.eventMatrices = mapFile.eventMatrices;
                }

                UnityEngine.Profiling.Profiler.BeginSample("Do skyboxes");
                //Do skyboxes
                for (int i = 0; i < mapFile.skyboxes.Length; i++)
                {
                    ref readonly MapGeometry.Skybox__ skybox = ref mapFile.skyboxes[i];
                    GameObject skyGo = null;
                    try
                    {
                        skyGo = new GameObject("Skybox");
                        SkyboxComponent sky = skyGo.AddComponent<SkyboxComponent>();
                        sky.header = skybox;
                        sky.boundingBox = skybox.GetBoundingBox();
                        skyGo.transform.SetParent(mapGo.transform);
                    }
                    catch
                    {
                        DestroyImmediate(skyGo);
                        throw;
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();

                UnityEngine.Profiling.Profiler.BeginSample("SetTransformFromMatrix");
                UnpackPath meshAssetPath = UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mesh.asset", true);
                if (meshAssetPath.FileExists())
                {
                    AssetDatabase.DeleteAsset(meshAssetPath);
                }
                UnityEngine.Profiling.Profiler.EndSample();

                //Do Meshes
                UnityEngine.Profiling.Profiler.BeginSample("Do Meshes");
                for (int i = 0; i < mapFile.meshGroups.Length; i++)
                {
                    ref readonly MapGeometry.MeshGroup meshGroupStruct = ref mapFile.meshGroups[i];

                    GameObject meshGroupGo = new GameObject("Mesh Group");
                    {
                        MeshGroupComponent meshGroup = meshGroupGo.AddComponent<MeshGroupComponent>();
                        meshGroup.header = meshGroupStruct.header;
                        meshGroupGo.isStatic = true;
                        meshGroupGo.transform.SetParent(mapGo.transform, false);
                    }

                    //Do sub meshes
                    UnityEngine.Profiling.Profiler.BeginSample("Do sub meshes");
                    for (int j = 0; j < meshGroupStruct.subs.Length; j++)
                    {
                        ref readonly MapGeometry.SubMeshGroup subMeshGroupStruct = ref meshGroupStruct.subs[j];

                        GameObject subMeshGroupGo = new GameObject("SubMesh Group");
                        {
                            SubMeshGroupComponent subMeshGroup = subMeshGroupGo.AddComponent<SubMeshGroupComponent>();
                            subMeshGroup.header = subMeshGroupStruct.header;
                            subMeshGroupGo.isStatic = true;
                            subMeshGroupGo.transform.SetParent(meshGroupGo.transform);
                        }

                        //Do sub sub meshes
                        UnityEngine.Profiling.Profiler.BeginSample("Do sub sub meshes");
                        for (int k = 0; k < subMeshGroupStruct.subsubs.Length; k++)
                        {
                            ref readonly MapGeometry.SubSubMeshGroup subSubMeshGroupStruct = ref subMeshGroupStruct.subsubs[k];

                            GameObject subSubMeshGroupGo = new GameObject("SubSubMesh Group");
                            {
                                SubSubMeshGroupComponent subSubMeshGroup = subSubMeshGroupGo.AddComponent<SubSubMeshGroupComponent>();
                                subSubMeshGroup.header = subSubMeshGroupStruct.header;
                                subSubMeshGroupGo.isStatic = true;
                                subSubMeshGroupGo.transform.SetParent(subMeshGroupGo.transform);
                            }

                            //Do mesh parts
                            UnityEngine.Profiling.Profiler.BeginSample("Do mesh parts");
                            for (int l = 0; l < subSubMeshGroupStruct.parts.Length; l++)
                            {
                                ref readonly MapGeometry.MeshPart meshPartStruct = ref subSubMeshGroupStruct.parts[l];

                                GameObject meshPartGO = new GameObject("Mesh Part");
                                {
                                    MeshPartComponent meshPart = meshPartGO.AddComponent<MeshPartComponent>();
                                    meshPart.header = meshPartStruct.header;
                                    meshPartGO.isStatic = meshPartStruct.header.objectType != 3;
                                    meshPartGO.transform.SetParent(subSubMeshGroupGo.transform);
                                }

                                UnityEngine.Profiling.Profiler.BeginSample("new List");
                                List<Vector3> verts = new List<Vector3>(meshPartStruct.vertices.Length);
                                List<Vector3> norms = new List<Vector3>(meshPartStruct.vertices.Length);
                                List<Vector2> uvs = new List<Vector2>(meshPartStruct.vertices.Length);
                                List<Color32> colors = new List<Color32>(meshPartStruct.vertices.Length);
                                UnityEngine.Profiling.Profiler.EndSample();
                                UnityEngine.Profiling.Profiler.BeginSample("meshPartStruct.vertices.Length");
                                for (int m = 0; m != meshPartStruct.vertices.Length; m++)
                                {
                                    ref MapGeometry.MeshPart.VertexInfo vertex = ref meshPartStruct.vertices[m];
                                    verts.Add(vertex.position);
                                    norms.Add(-vertex.normal);
                                    uvs.Add(vertex.uv);
                                    colors.Add(vertex.color);
                                }
                                UnityEngine.Profiling.Profiler.EndSample();

                                UnityEngine.Profiling.Profiler.BeginSample("MakeStrippedInverted");
                                Mesh mesh = MeshUtil.MakeStrippedInverted(verts, norms, uvs, colors);
                                UnityEngine.Profiling.Profiler.EndSample();
                                mesh.name = grid.fullName + "_mesh_" + i + "_" + j + "_" + k + "_" + l;

                                UnityEngine.Profiling.Profiler.BeginSample("FileExists");
                                if (meshAssetPath.FileExists())
                                {
                                    UnityEngine.Profiling.Profiler.BeginSample("AddObjectToAsset");
                                    AssetDatabase.AddObjectToAsset(mesh, meshAssetPath);
                                    UnityEngine.Profiling.Profiler.EndSample();
                                }
                                else
                                {
                                    UnityEngine.Profiling.Profiler.BeginSample("CreateAsset");
                                    AssetDatabase.CreateAsset(mesh, meshAssetPath);
                                    UnityEngine.Profiling.Profiler.EndSample();
                                }
                                UnityEngine.Profiling.Profiler.EndSample();
                                meshPartGO.AddComponent<MeshFilter>().sharedMesh = mesh;

                                int baseIndex = 0;
                                if (meshGroupStruct.header.textureGroup == 3)
                                {
                                    baseIndex = mapFile.mainHeader.localTextureBaseIndex + mapFile.mainHeader.localTextureBaseIndexModifier;
                                }

                                UnityEngine.Profiling.Profiler.BeginSample("SH3MaterialRolodex");
                                MaterialRolodex rolodex = grid.GetTextureGroup(meshGroupStruct.header.textureGroup);
                                MeshRenderer renderer = meshPartGO.AddComponent<MeshRenderer>();
                                renderer.sharedMaterial = rolodex.GetWithSH3Index(meshGroupStruct.header.textureIndex, baseIndex, MaterialRolodex.SH3MaterialToType(subMeshGroupStruct, subSubMeshGroupStruct));
                                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                                UnityEngine.Profiling.Profiler.EndSample();


                            }
                            UnityEngine.Profiling.Profiler.EndSample();
                        }
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }
            catch
            {
                GameObject.DestroyImmediate(mapGo);
                UnityEngine.Profiling.Profiler.EndSample();
                throw;
            }

            Matrix4x4Util.SetTransformFromMatrix(mapGo.transform, ref mapGo.GetComponentInChildren<SkyboxComponent>().header.matrix);

            UnityEngine.Profiling.Profiler.EndSample();
            return mapGo;
        }
    }
}
