using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

using SH.GameData.SH3;
using SH.Unity.Shared;
using SH.GameData.Shared;
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
                        proxy.Unpack();
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

            GameObject mapGO = null;

            if (TRtex != null)
            {
                UnpackTRTextures(this);
            }

            if (map != null)
            {
                MapGeometry mapFile = GetMapFile(this);
                UnpackLocalTextures(this, mapFile.textureGroup);
                mapGO = UnpackMap(this, mapFile);
            }

            UnityEngine.Profiling.Profiler.BeginSample("MakePrefab");
            try
            {
                MakePrefab(mapGO);
            }
            catch
            {
                GameObject.DestroyImmediate(mapGO);

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

            MapGeometry mapFile = new MapGeometry();
            using (FileStream file = new FileStream(UnpackPath.GetPath(grid.map), FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {
                mapFile.ReadFile(reader);
            }

            UnityEngine.Profiling.Profiler.EndSample();
            return mapFile;
        }

        private static void UnpackLocalTextures(GridProxy grid, in TextureGroup localTextures)
        {
            UnityEngine.Profiling.Profiler.BeginSample("UnpackLocalTextures");

            grid.localTextures = MaterialRolodex.CreateInstance<MaterialRolodex>();
            AssetDatabase.CreateAsset(grid.localTextures, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mats.asset", true));
            grid.localTextures.AddTextures(MaterialRolodex.ReadTex32(grid.fullName + "_tex_", in localTextures));

            UnityEngine.Profiling.Profiler.EndSample();
        }

        private static void UnpackTRTextures(GridProxy grid)
        {
            UnityEngine.Profiling.Profiler.BeginSample("UnpackTRTextures");

            TextureGroup trGroup;
            using (FileStream file = new FileStream(UnpackPath.GetPath(grid.TRtex), FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {
                trGroup = TextureGroup.ReadTextureGroup(reader);
            }
            grid.TRTextures = MaterialRolodex.CreateInstance<MaterialRolodex>();
            AssetDatabase.CreateAsset(grid.TRTextures, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "TR_mats.asset", true));
            grid.TRTextures.AddTextures(MaterialRolodex.ReadTex32(grid.fullName + "TR_tex_", in trGroup));

            UnityEngine.Profiling.Profiler.EndSample();
        }

        private static GameObject UnpackMap(GridProxy grid, MapGeometry mapFile)
        {
            UnityEngine.Profiling.Profiler.BeginSample("UnpackMap");
            GameObject mapGo = new GameObject("Geometry");
            mapGo.isStatic = true;
            try
            {
                for (int i = 0; i < mapFile.transforms.Length; i++)
                {
                    ref readonly MapGeometry.ObjectTransform objectTransform = ref mapFile.transforms[i];
                    if (objectTransform.objectType == 1 && objectTransform.partID == 0)
                    {
                        Matrix4x4Util.SetTransformFromMatrix(mapGo.transform, in objectTransform.transform);
                        break;
                    }
                }

                {
                    MapGeometryComponent m = mapGo.AddComponent<MapGeometryComponent>();
                    m.geometry = mapFile;
                    m.header = mapFile.mainHeader;
                    m.eventMatrices = mapFile.interestPoints;
                }

                UnityEngine.Profiling.Profiler.BeginSample("Do Object Transforms");
                //Do Object Transforms
                for (int i = 0; i < mapFile.transforms.Length; i++)
                {
                    ref readonly MapGeometry.ObjectTransform objectTransform = ref mapFile.transforms[i];
                    GameObject objectTransformGo = null;
                    try
                    {
                        objectTransformGo = new GameObject("ObjectTransform");
                        ObjectTransformComponent sky = objectTransformGo.AddComponent<ObjectTransformComponent>();
                        sky.header = objectTransform;
                        sky.boundingBox = objectTransform.GetBoundingBox();
                        objectTransformGo.transform.SetParent(mapGo.transform);
                    }
                    catch
                    {
                        DestroyImmediate(objectTransformGo);
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
                    MapGeometry.MeshGroup meshGroupStruct = mapFile.meshGroups[i];

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
                        MapGeometry.SubMeshGroup subMeshGroupStruct = meshGroupStruct.subs[j];

                        GameObject subMeshGroupGo = new GameObject("SubMesh Group");
                        {
                            SubMeshGroupComponent subMeshGroup = subMeshGroupGo.AddComponent<SubMeshGroupComponent>();
                            subMeshGroup.header = subMeshGroupStruct.header;
                            subMeshGroupGo.isStatic = true;
                            subMeshGroupGo.transform.SetParent(meshGroupGo.transform, false);
                        }

                        //Do sub sub meshes
                        UnityEngine.Profiling.Profiler.BeginSample("Do sub sub meshes");
                        for (int k = 0; k < subMeshGroupStruct.subsubs.Length; k++)
                        {
                            MapGeometry.SubSubMeshGroup subSubMeshGroupStruct = subMeshGroupStruct.subsubs[k];

                            GameObject subSubMeshGroupGo = new GameObject("SubSubMesh Group");
                            {
                                SubSubMeshGroupComponent subSubMeshGroup = subSubMeshGroupGo.AddComponent<SubSubMeshGroupComponent>();
                                subSubMeshGroup.header = subSubMeshGroupStruct.header;
                                subSubMeshGroupGo.isStatic = true;
                                subSubMeshGroupGo.transform.SetParent(subMeshGroupGo.transform, false);
                            }

                            //Do mesh parts
                            UnityEngine.Profiling.Profiler.BeginSample("Do mesh parts");
                            for (int l = 0; l < subSubMeshGroupStruct.parts.Length; l++)
                            {
                                MapGeometry.MeshPart meshPartStruct = subSubMeshGroupStruct.parts[l];

                                GameObject meshPartGO = new GameObject("Mesh Part");
                                {
                                    MeshPartComponent meshPart = meshPartGO.AddComponent<MeshPartComponent>();
                                    meshPart.header = meshPartStruct.header;
                                    meshPart.extraData = meshPartStruct.extraData;
                                    meshPartGO.isStatic = meshPartStruct.header.objectType != 3;
                                    meshPartGO.transform.SetParent(subSubMeshGroupGo.transform, false);

                                    if (meshPartStruct.header.objectType == 3)
                                    {
                                        for (int m = 0; m < mapFile.transforms.Length; m++)
                                        {
                                            ref readonly MapGeometry.ObjectTransform objectTransform = ref mapFile.transforms[m];
                                            if(objectTransform.objectType == 3 && objectTransform.partID == meshPartStruct.header.partID)
                                            {
                                                meshPartGO.transform.position = Matrix4x4Util.ExtractTranslationFromMatrix(in objectTransform.transform);
                                                meshPartGO.transform.rotation = Matrix4x4Util.ExtractRotationFromMatrix(in objectTransform.transform);
                                                meshPartGO.transform.localScale = Matrix4x4Util.ExtractScaleFromMatrix(in objectTransform.transform);
                                            }
                                        }
                                    }
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

                                UnityEngine.Profiling.Profiler.BeginSample("SH3MaterialRolodex");
                                MaterialRolodex rolodex = grid.GetTextureGroup(meshGroupStruct.header.textureGroup);

                                MeshRenderer renderer = meshPartGO.AddComponent<MeshRenderer>();
                                int textureIndex = meshGroupStruct.header.textureIndex;
                                if (meshGroupStruct.header.textureGroup == 3)
                                {
                                    textureIndex -= mapFile.mainHeader.meshPartGBTexCount + mapFile.mainHeader.meshPartTRTexCount;
                                }
                                renderer.sharedMaterial = rolodex.GetWithSH3Index(textureIndex, MaterialRolodex.SH3MaterialToType(subMeshGroupStruct, subSubMeshGroupStruct));

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

            UnityEngine.Profiling.Profiler.EndSample();
            return mapGo;
        }
    }
}
