using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

using SH.Unity.Shared;
using SH.GameData.SH2;
using SH.GameData.Shared;
using SH.Core;
using System.Collections.ObjectModel;
using System;

namespace SH.Unity.SH2
{
    [CustomEditor(typeof(GridProxy))]
    public class GridProxyEditor : BaseImportProxyEditor { }

    public class GridProxy : BaseImportProxy
    {
        public LevelProxy level;
        public string gridName;
        public UnityEngine.Object map;
        public UnityEngine.Object cam;
        public UnityEngine.Object cld;
        public UnityEngine.Object kg2;
        public UnityEngine.Object dmm;

        public GameObject prefab;
        public MaterialRolodex localTextures;

        public string fullName
        {
            get => level.levelName + gridName;
        }

        private UnpackPath prefabPath
        {
            get => UnpackPath.GetDirectory(this).AddToPath("scene/").WithDirectoryAndName(UnpackDirectory.Unity, fullName + ".prefab", true);
        }

        public void MakePrefab()
        {
            PrefabUtil.MakePrefab(ref prefab, prefabPath, null);
        }

        public override void Unpack()
        {
            CollectionPool.Request(out List<GameObject> gameObjects);
            if (map != null)
            {
                FileMap mapFile = FileMap.ReadMapFile(UnpackPath.GetPath(map));
                UnpackLocalTextures(this, mapFile.textureFile);
                gameObjects.Add(UnpackMap(mapFile, this));
            }

            if (cld != null)
            {
                FileCollisions collisionFile = FileCollisions.ReadCollisionFile(UnpackPath.GetPath(cld));
                gameObjects.Add(UnpackCollisions(collisionFile));
            }

            PrefabUtil.MakePrefab(ref prefab, prefabPath, gameObjects, true);
            CollectionPool.Return(ref gameObjects);
            EditorUtility.SetDirty(this);
        }

        private static GameObject UnpackMap(FileMap mapFile, GridProxy grid)
        {
            GameObject go = new GameObject("Geometry");
            go.isStatic = true;

            UnpackPath meshAssetPath = UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mesh.asset", true);
            if (meshAssetPath.FileExists())
            {
                AssetDatabase.DeleteAsset(meshAssetPath);
            }

            try
            {
                MapGeometryComponent mapComponent = go.AddComponent<MapGeometryComponent>();
                mapComponent.geometry = mapFile;

                Mesh mesh = null;
                CollectionPool.Request(out List<Vector3> verts);
                CollectionPool.Request(out List<Vector3> norms);
                CollectionPool.Request(out List<ColorBGRA> colors);
                CollectionPool.Request(out List<Vector2> uvs);
                try
                {
                    bool useColors = false;
                    /*if (mapFile.meshFile.meshGroup.meshGroupVerticesHeader.vertexSize == 0x14)
                    {
                        FileMap.MeshFile.MeshGroup.Vertex14.ExtractToBuffers(mapFile.meshFile.meshGroup.vertices, verts, uvs);
                    }
                    else */if (mapFile.meshFile.meshGroup.meshGroupVerticesHeader.vertexSize == 0x20)
                    {
                        FileMap.FileMesh.MeshGroup.Vertex20.ExtractToBuffers(mapFile.meshFile.meshGroup.vertices, verts, norms, uvs);
                    }
                    else if (mapFile.meshFile.meshGroup.meshGroupVerticesHeader.vertexSize == 0x24)
                    {
                        FileMap.FileMesh.MeshGroup.Vertex24.ExtractToBuffers(mapFile.meshFile.meshGroup.vertices, verts, norms, colors, uvs);
                        useColors = true;
                    }
                    else
                    {
                        throw new System.Exception();
                    }

                    FileMap.FileMesh.MeshGroup.SubMeshGroup[] groups = mapFile.meshFile.meshGroup.subMeshGroups;
                    Dictionary<int, short[]> indices = new Dictionary<int, short[]>(groups.Length);

                    for (int i = 0; i != groups.Length; i++)
                    {
                        indices.Add(groups[i].id, null);
                    }

                    for (int i = 0, k = 0; i != groups.Length; i++)
                    {
                        FileMap.FileMesh.MeshGroup.SubMeshGroup group = groups[i];
                        short[] groupIndices = indices[group.id] = new short[group.indexCount];
                        for (int j = 0; j != group.indexCount; j++)
                        {
                            groupIndices[j] = mapFile.meshFile.meshGroup.indices[k++];
                        }
                    }

                    mesh = MeshUtil.MakeIndexedStrip(verts, indices, norms, uvs,/*, useColors ? colors :*/ null);
                    mesh.name = grid.fullName + "_testmesh_" + DateTime.UtcNow.Millisecond;

                    if (meshAssetPath.FileExists())
                    {
                        AssetDatabase.AddObjectToAsset(mesh, meshAssetPath);
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(mesh, meshAssetPath);
                    }

                    MeshFilter mf = go.AddComponent<MeshFilter>();
                    mf.sharedMesh = mesh;

                    MeshRenderer mr = go.AddComponent<MeshRenderer>();
                    Material[] materials = new Material[groups.Length];
                    for(int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = grid.localTextures.GetWithSH2ID(mapFile.meshFile.materials[groups[i].id].textureId, MaterialRolodexBase.MaterialType.Diffuse);
                    }
                    mr.sharedMaterials = materials;
                }
                finally
                {
                    CollectionPool.Return(ref verts);
                    CollectionPool.Return(ref norms);
                    CollectionPool.Return(ref colors);
                    CollectionPool.Return(ref uvs);
                }
            }
            catch
            {
                GameObject.DestroyImmediate(go);
                throw;
            }

            return go;
        }
        private static void UnpackLocalTextures(GridProxy grid, in FileTex localTextures)
        {
            grid.localTextures = MaterialRolodex.CreateInstance<MaterialRolodex>();
            AssetDatabase.CreateAsset(grid.localTextures, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mats.asset", true));
            grid.localTextures.AddTextures(MaterialRolodex.ReadTexDXT1(grid.fullName + "_tex_", localTextures));
        }

        private static GameObject UnpackCollisions(FileCollisions collisionFile)
        {
            GameObject go = new GameObject("Collisions");
            go.isStatic = true;
            try
            {
                MapCollisionComponent collisionComponent = go.AddComponent<MapCollisionComponent>();
                collisionComponent.collisions = collisionFile;
            }
            catch
            {
                GameObject.DestroyImmediate(go);
                throw;
            }

            return go;
        }

        public override void Pack()
        {

        }
    }
}