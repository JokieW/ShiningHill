using ShiningHill;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SH2GridProxy : BaseImportProxy
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

    public SH3MaterialRolodex GetTextureGroup(int group)
    {
        if (group == 3) return localTextures;
        if (group == 2) return TRTextures;
        if (group == 1) return level.GBTextures;
        throw new System.InvalidOperationException("Unknown texture group " + group);
    }

    public void MakePrefab()
    {
        if (prefab != null)
        {
            DestroyImmediate(prefab);
        }

        UnpackPath dest = UnpackPath.GetDirectory(this).WithDirectoryAndName(UnpackDirectory.Unity, gridName + ".prefab");
        if(dest.FileExists())
        {
            AssetDatabase.DeleteAsset(dest);
        }

        prefab = new GameObject("Grid " + gridName);
        prefab.isStatic = true;
    }

    public void Unpack()
    {
        MakePrefab();
        try
        {
            //GameObject mapGO = UnpackMap(this);
        }
        catch
        {

        }
    }


    private static GameObject UnpackMap(SH3GridProxy grid)
    {
        if (grid.map == null) return null;

        MapFile mapFile = null;
        using (FileStream file = new FileStream(UnpackPath.GetPath(grid.map), FileMode.Open, FileAccess.ReadWrite))
        using (BinaryReader reader = new BinaryReader(file))
        {
            mapFile = new MapFile(reader);
        }
        grid.localTextures = new SH3MaterialRolodex();
        grid.localTextures.AddTextures(TextureUtils.ReadTex32(grid.gridName + "_tex", in mapFile.textureGroup));

        GameObject mapGo = new GameObject("Map");
        try
        {
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

            Matrix4x4Utils.SetTransformFromMatrix(mapGo.transform, ref mapGo.GetComponentInChildren<ShiningHill.Skybox>().header.matrix);

            //Do Meshes
            for (int i = 0; i < mapFile.meshGroups.Length; i++)
            {
                ref readonly MapFile.MeshGroup meshGroupStruct = ref mapFile.meshGroups[i];

                GameObject meshGroupGo = new GameObject("Mesh Group");
                {
                    MeshGroup meshGroup = meshGroupGo.AddComponent<MeshGroup>();
                    meshGroup.header = meshGroupStruct.header;
                    meshGroupGo.isStatic = true;
                    meshGroupGo.transform.SetParent(mapGo.transform);
                }

                //Do sub meshes
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
                        for (int l = 0; l < subSubMeshGroupStruct.parts.Length; k++)
                        {
                            ref readonly MapFile.MeshPart meshPartStruct = ref subSubMeshGroupStruct.parts[l];

                            GameObject meshPartGO = new GameObject("Mesh Part");
                            {
                                MeshPart meshPart = meshPartGO.AddComponent<MeshPart>();
                                meshPart.header = meshPartStruct.header;
                                meshPartGO.isStatic = meshPartStruct.header.objectType != 3;
                                meshPartGO.transform.SetParent(subSubMeshGroupGo.transform);
                            }

                            List<Vector3> verts = new List<Vector3>(meshPartStruct.vertices.Length);
                            List<Vector3> norms = new List<Vector3>(meshPartStruct.vertices.Length);
                            List<Vector2> uvs = new List<Vector2>(meshPartStruct.vertices.Length);
                            List<Color32> colors = new List<Color32>(meshPartStruct.vertices.Length);
                            for (int m = 0; m != meshPartStruct.vertices.Length; m++)
                            {
                                ref MapFile.MeshPart.VertexInfo vertex = ref meshPartStruct.vertices[m];
                                verts.Add(vertex.position);
                                norms.Add(vertex.normal);
                                uvs.Add(vertex.uv);
                                colors.Add(vertex.color);
                            }

                            Mesh mesh = MeshUtils.MakeStripped(verts, norms, uvs, colors);
                            mesh.name = "mesh_" + i + "_" + j + "_" + k + "_" + l;
                            meshPartGO.AddComponent<MeshFilter>().sharedMesh = mesh;

                            int baseIndex = 0;
                            if (meshGroupStruct.header.textureGroup == 3)
                            {
                                baseIndex = mapFile.mainHeader.localTextureBaseIndex + mapFile.mainHeader.localTextureBaseIndexModifier;
                            }

                            SH3MaterialRolodex rolodex = grid.GetTextureGroup(meshGroupStruct.header.textureGroup);
                            MeshRenderer renderer = meshPartGO.AddComponent<MeshRenderer>();
                            if (subMeshGroupStruct.header.transparencyType == 1)
                            {
                                renderer.sharedMaterial = rolodex.GetWithSH3Index(meshGroupStruct.header.textureIndex, baseIndex, MapFile.MaterialType.Transparent);
                            }
                            else if (subMeshGroupStruct.header.transparencyType == 3)
                            {
                                renderer.sharedMaterial = rolodex.GetWithSH3Index(meshGroupStruct.header.textureIndex, baseIndex, MapFile.MaterialType.Cutout);
                            }
                            else if (subSubMeshGroupStruct.header.illuminationType == 8)
                            {
                                renderer.sharedMaterial = rolodex.GetWithSH3Index(meshGroupStruct.header.textureIndex, baseIndex, MapFile.MaterialType.SelfIllum);
                            }
                            else
                            {
                                renderer.sharedMaterial = rolodex.GetWithSH3Index(meshGroupStruct.header.textureIndex, baseIndex, MapFile.MaterialType.Diffuse);
                            }
                            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        }
                    }
                }
            }
        }
        catch { GameObject.DestroyImmediate(mapGo); throw; }
        return mapGo;
    }
}
