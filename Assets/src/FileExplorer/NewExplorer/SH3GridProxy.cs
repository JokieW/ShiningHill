using ShiningHill;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SH3GridProxy))]
[CanEditMultipleObjects]
public class SH3GridProxyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //Unpack
        if (GUILayout.Button("Unpack"))
        {
            try
            {
                ExplorerUtil.StartAssetEditing();
                for (int i = 0; i < targets.Length; i++)
                {
                    SH3GridProxy proxy = (SH3GridProxy)targets[i];
                }
            }
            finally
            {
                ExplorerUtil.StopAssetEditing();
            }
        }

        //Pack
        if (GUILayout.Button("Pack"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                SH3GridProxy proxy = (SH3GridProxy)targets[i];
                proxy.Pack();
            }
        }
    }
}

public class SH3GridProxy : BaseImportProxy
{
    public SH3LevelProxy level;
    public string gridName;
    public UnityEngine.Object map;
    public UnityEngine.Object cam;
    public UnityEngine.Object cld;
    public UnityEngine.Object kg2;
    public UnityEngine.Object ded;
    public UnityEngine.Object TRtex;

    public GameObject prefab;
    public SH3MaterialRolodex localTextures;
    public SH3MaterialRolodex TRTextures;

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

    public SH3MaterialRolodex GetTextureGroup(int group)
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
        if(dest.FileExists())
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
        MapFile mapFile = GetMapFile(this);
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

    private static MapFile GetMapFile(SH3GridProxy grid)
    {
        UnityEngine.Profiling.Profiler.BeginSample("GetMapFile");
        if (grid.map == null)
        {
            UnityEngine.Profiling.Profiler.EndSample();
            return null;
        }

        MapFile mapFile = null;
        using (FileStream file = new FileStream(UnpackPath.GetPath(grid.map), FileMode.Open, FileAccess.ReadWrite))
        using (BinaryReader reader = new BinaryReader(file))
        {
            mapFile = new MapFile(reader);
        }
        UnityEngine.Profiling.Profiler.EndSample();
        return mapFile;
    }

    private static void UnpackMaterialRolodexes(SH3GridProxy grid, MapFile mapFile)
    {
        UnityEngine.Profiling.Profiler.BeginSample("GetMapFile");
        grid.localTextures = SH3MaterialRolodex.CreateInstance<SH3MaterialRolodex>();
        UnityEngine.Profiling.Profiler.BeginSample("CreateAsset1");
        AssetDatabase.CreateAsset(grid.localTextures, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mats.asset", true));
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("AddTextures");
        grid.localTextures.AddTextures(TextureUtils.ReadTex32(grid.fullName + "_tex_", in mapFile.textureGroup));
        UnityEngine.Profiling.Profiler.EndSample();

        if (grid.TRtex != null)
        {
            UnityEngine.Profiling.Profiler.BeginSample("_TRtex");
            MaterialRolodex.TextureGroup trGroup;
            using (FileStream file = new FileStream(UnpackPath.GetPath(grid.TRtex), FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {
                SH3MaterialRolodex.ReadTextureGroup(reader, out trGroup);
            }
            grid.TRTextures = SH3MaterialRolodex.CreateInstance<SH3MaterialRolodex>();
            AssetDatabase.CreateAsset(grid.TRTextures, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "TR_mats.asset", true));
            grid.TRTextures.AddTextures(TextureUtils.ReadTex32(grid.fullName + "TR_tex_", in trGroup));
            UnityEngine.Profiling.Profiler.EndSample();
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private static GameObject UnpackMap(SH3GridProxy grid, MapFile mapFile)
    {
        UnityEngine.Profiling.Profiler.BeginSample("UnpackMap");
        GameObject mapGo = new GameObject("Geometry");
        mapGo.isStatic = true;
        try
        {
            UnityEngine.Profiling.Profiler.BeginSample("Do skyboxes");
            //Do skyboxes
            for (int i = 0; i < mapFile.skyboxes.Length; i++)
            {
                ref readonly MapFile.Skybox__ skybox = ref mapFile.skyboxes[i];
                GameObject skyGo = null;
                try
                {
                    skyGo = new GameObject("Skybox");
                    ShiningHill.Skybox sky = skyGo.AddComponent<ShiningHill.Skybox>();
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
            if(meshAssetPath.FileExists())
            {
                AssetDatabase.DeleteAsset(meshAssetPath);
            }
            UnityEngine.Profiling.Profiler.EndSample();

            //Do Meshes
            UnityEngine.Profiling.Profiler.BeginSample("Do Meshes");
            for (int i = 0; i < mapFile.meshGroups.Length; i++)
            {
                ref readonly MapFile.MeshGroup meshGroupStruct = ref mapFile.meshGroups[i];

                GameObject meshGroupGo = new GameObject("Mesh Group");
                {
                    MeshGroup meshGroup = meshGroupGo.AddComponent<MeshGroup>();
                    meshGroup.header = meshGroupStruct.header;
                    meshGroupGo.isStatic = true;
                    meshGroupGo.transform.SetParent(mapGo.transform, false);
                }

                //Do sub meshes
                UnityEngine.Profiling.Profiler.BeginSample("Do sub meshes");
                for (int j = 0; j < meshGroupStruct.subs.Length; j++)
                {
                    ref readonly MapFile.SubMeshGroup subMeshGroupStruct = ref meshGroupStruct.subs[j];

                    GameObject subMeshGroupGo = new GameObject("SubMesh Group");
                    {
                        SubMeshGroup subMeshGroup = subMeshGroupGo.AddComponent<SubMeshGroup>();
                        subMeshGroup.header = subMeshGroupStruct.header;
                        subMeshGroupGo.isStatic = true;
                        subMeshGroupGo.transform.SetParent(meshGroupGo.transform);
                    }

                    //Do sub sub meshes
                    UnityEngine.Profiling.Profiler.BeginSample("Do sub sub meshes");
                    for (int k = 0; k < subMeshGroupStruct.subsubs.Length; k++)
                    {
                        ref readonly MapFile.SubSubMeshGroup subSubMeshGroupStruct = ref subMeshGroupStruct.subsubs[k];

                        GameObject subSubMeshGroupGo = new GameObject("SubSubMesh Group");
                        {
                            SubSubMeshGroup subSubMeshGroup = subSubMeshGroupGo.AddComponent<SubSubMeshGroup>();
                            subSubMeshGroup.header = subSubMeshGroupStruct.header;
                            subSubMeshGroupGo.isStatic = true;
                            subSubMeshGroupGo.transform.SetParent(subMeshGroupGo.transform);
                        }

                        //Do mesh parts
                        UnityEngine.Profiling.Profiler.BeginSample("Do mesh parts");
                        for (int l = 0; l < subSubMeshGroupStruct.parts.Length; l++)
                        {
                            ref readonly MapFile.MeshPart meshPartStruct = ref subSubMeshGroupStruct.parts[l];

                            GameObject meshPartGO = new GameObject("Mesh Part");
                            {
                                MeshPart meshPart = meshPartGO.AddComponent<MeshPart>();
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
                                ref MapFile.MeshPart.VertexInfo vertex = ref meshPartStruct.vertices[m];
                                verts.Add(vertex.position);
                                norms.Add(-vertex.normal);
                                uvs.Add(vertex.uv);
                                colors.Add(vertex.color);
                            }
                            UnityEngine.Profiling.Profiler.EndSample();

                            UnityEngine.Profiling.Profiler.BeginSample("MakeStrippedInverted");
                            Mesh mesh = MeshUtils.MakeStrippedInverted(verts, norms, uvs, colors);
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
                            SH3MaterialRolodex rolodex = grid.GetTextureGroup(meshGroupStruct.header.textureGroup);
                            MeshRenderer renderer = meshPartGO.AddComponent<MeshRenderer>();
                            renderer.sharedMaterial = rolodex.GetWithSH3Index(meshGroupStruct.header.textureIndex, baseIndex, MapFile.SH3MaterialToType(subMeshGroupStruct, subSubMeshGroupStruct));
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

        Matrix4x4Utils.SetTransformFromMatrix(mapGo.transform, ref mapGo.GetComponentInChildren<ShiningHill.Skybox>().header.matrix);

        UnityEngine.Profiling.Profiler.EndSample();
        return mapGo;
    }
}
