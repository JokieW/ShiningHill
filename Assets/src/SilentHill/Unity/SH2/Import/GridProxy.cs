using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using SH.Unity.Shared;
using SH.GameData.SH2;
using SH.GameData.Shared;
using SH.Core;
using SH.Unity.SH3;
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
        public MaterialRolodex mapMaterials;

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
                UnpackLocalTextures(this, mapFile);
                GameObject go = UnpackMap(mapFile, this);
                go.AddComponent<MapFileComponent>().SetMapFile(mapFile);
                gameObjects.Add(go);
            }

            if (cld != null)
            {
                FileCollisions collisionFile = FileCollisions.ReadCollisionFile(UnpackPath.GetPath(cld));
                gameObjects.Add(UnpackCollisions(collisionFile));
            }

            Debug.Log("sdfsdf");
            if (kg2 != null)
            {
                GameObject gg = MapShadows.ReadShadowCasters(UnpackPath.GetPath(kg2));
            }

            PrefabUtil.MakePrefab(ref prefab, prefabPath, gameObjects, true);
            CollectionPool.Return(ref gameObjects);
            EditorUtility.SetDirty(this);
        }

        private static GameObject UnpackMap(FileMap mapFile, GridProxy grid)
        {
            GameObject topGo = new GameObject("Geometry") { isStatic = true };

            UnpackPath meshAssetPath = UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mesh.asset", true);
            if (meshAssetPath.FileExists())
            {
                AssetDatabase.DeleteAsset(meshAssetPath);
            }

            try
            {
                SubFileGeometry meshFile = mapFile.GetMainGeometryFile();

                if (meshFile != null)
                {
                    for (int i = 0; i < meshFile.geometries.Length; i++)
                    {
                        GameObject geometryGo = new GameObject("Geometry " + i.ToString("X")) { isStatic = true };
                        geometryGo.transform.SetParent(topGo.transform);

                        SubFileGeometry.Geometry geo = meshFile.geometries[i];
                        for (int j = 0; j < 2; j++)
                        {
                            GameObject meshGroupGo = new GameObject(j == 0 ? "Opaque Group" : "Transparent Group") { isStatic = true };
                            meshGroupGo.transform.SetParent(geometryGo.transform);

                            SubFileGeometry.Geometry.MeshGroup meshGroup = j == 0 ? geo.opaqueGroup : geo.transparentGroup;
                            if (meshGroup != null)
                            {
                                for (int k = 0; k < meshGroup.mapMeshs.Length; k++)
                                {
                                    GameObject mapMeshGo = new GameObject("MapMesh " + k.ToString("X")) { isStatic = true };
                                    mapMeshGo.transform.SetParent(meshGroupGo.transform);

                                    int indicesIndex = 0;
                                    SubFileGeometry.Geometry.MeshGroup.MapMesh mapMesh = meshGroup.mapMeshs[k];
                                    for (int l = 0; l < mapMesh.meshPartGroups.Length; l++)
                                    {
                                        GameObject meshPartGroupGo = new GameObject("MeshPartGroup " + l.ToString("X")) { isStatic = true };
                                        meshPartGroupGo.transform.SetParent(mapMeshGo.transform);

                                        SubFileGeometry.Geometry.MeshGroup.MapMesh.MeshPartGroup meshPartGroup = mapMesh.meshPartGroups[l];
                                        int sectionId = meshPartGroup.header.sectionId;
                                        int vertexSize = mapMesh.vertexSections[sectionId].vertexSize;
                                        for (int m = 0; m < meshPartGroup.meshParts.Length; m++)
                                        {
                                            SubFileGeometry.Geometry.MeshGroup.MapMesh.MeshPartGroup.MeshPart meshPart = meshPartGroup.meshParts[m];
                                            Mesh mesh = MakeSubMeshFromIndices(
                                                vertexSize,
                                                meshPart,
                                                mapMesh.vertices[sectionId],
                                                grid.fullName + "_meshpart_" + i + "_" + j + "_" + k + "_" + l + "_" + m + "_" + meshPartGroup.header.materialIndex,
                                                ref indicesIndex,
                                                mapMesh.indices);
                                            Material material = grid.mapMaterials.GetMaterial(meshPartGroup.header.materialIndex);
                                            GameObject meshPartGo = CreateMeshAssetAndSubGameObject("MeshPart " + m.ToString("X"), meshAssetPath, meshPartGroupGo, mesh, material);
                                            meshPartGo.AddComponent<MapSubMeshComponent>().subMesh = meshPartGroup;
                                        }
                                    }
                                }
                            }
                        }

                        if (geo.mapDecals != null)
                        {
                            SubFileGeometry.Geometry.MapDecals.Decal[] decals = geo.mapDecals.decals;
                            for (int k = 0; k < decals.Length; k++)
                            {
                                SubFileGeometry.Geometry.MapDecals.Decal decal = decals[k];

                                GameObject decGo = new GameObject("Decal " + k.ToString("X")) { isStatic = true };
                                decGo.transform.SetParent(geometryGo.transform);
                                decGo.AddComponent<DecalComponent>().decal = decal;

                                for (int l = 0, indicesIndex = 0; l < decal.subDecals.Length; l++)
                                {
                                    SubFileGeometry.Geometry.MapDecals.Decal.SubDecal subDecal = decal.subDecals[l];
                                    int vertexSize = decal.vertexSections[subDecal.sectionId].vertexSize;
                                    Mesh mesh = MakeSubMeshFromIndices(vertexSize, decal.vertices[subDecal.sectionId], grid.fullName + "_subdecal_" + k, subDecal.stripLength, subDecal.stripCount, ref indicesIndex, decal.indices);
                                    Material material = grid.mapMaterials.GetMaterial(subDecal.materialIndex);
                                    GameObject subGo = CreateMeshAssetAndSubGameObject("SubDecal " + l.ToString("X"), meshAssetPath, decGo, mesh, material);
                                    subGo.AddComponent<SubDecalComponent>().subDecal = subDecal;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                GameObject.DestroyImmediate(topGo);
                throw;
            }

            return topGo;
        }

        private static Mesh MakeSubMeshFromIndices(int vertexSize, byte[] vertices, string meshName, int stripLength, int stripCount, ref int indicesIndex, ushort[] indices)
        {
            Mesh mesh = MakeMeshFromVertices(vertexSize, vertices);

            CollectionPool.Request(out List<int> triangles);
            indicesIndex += MeshUtil.Unstrip(indicesIndex, 0, stripLength, stripCount, indices, triangles);
            mesh.SetTriangles(triangles, 0);
            CollectionPool.Return(ref triangles);

            mesh.name = meshName;
            return mesh;
        }

        private static Mesh MakeSubMeshFromIndices(int vertexSize, SubFileGeometry.Geometry.MeshGroup.MapMesh.MeshPartGroup.MeshPart subSubMesh, byte[] vertices, string meshName, ref int indicesIndex, ushort[] indices)
        {
            byte[] usedVertices = new byte[vertexSize * (subSubMesh.lastVertex - subSubMesh.firstVertex + 1)];
            UnsafeUtil.MemCopy(vertices, subSubMesh.firstVertex * vertexSize, usedVertices, 0, usedVertices.Length);
            Mesh mesh = MakeMeshFromVertices(vertexSize, usedVertices);

            CollectionPool.Request(out List<int> triangles);
            indicesIndex += MeshUtil.Unstrip(indicesIndex, subSubMesh.firstVertex, subSubMesh.invertReading == 0 ? subSubMesh.stripLength : subSubMesh.stripCount, subSubMesh.invertReading == 0 ? subSubMesh.stripCount : subSubMesh.stripLength, indices, triangles);
            mesh.SetTriangles(triangles, 0);
            CollectionPool.Return(ref triangles);

            mesh.name = meshName;
            return mesh;
        }

        public static Mesh MakeMeshFromVertices(int vertexSize, byte[] vertices)
        {
            CollectionPool.Request(out List<Vector3> verts);
            CollectionPool.Request(out List<Vector3> norms);
            CollectionPool.Request(out List<Color32> colors);
            CollectionPool.Request(out List<Vector2> uvs);
            try
            {
                SubFileGeometry.Geometry.UnpackVertices(vertexSize, vertices, verts, norms, uvs, colors);
                Mesh mesh = new Mesh();
                mesh.SetVertices(verts);
                if(norms.Count > 0)
                {
                    mesh.SetNormals(norms);
                }
                if (uvs.Count > 0)
                {
                    mesh.SetUVs(0, uvs);
                }
                if (colors.Count > 0)
                {
                    mesh.SetColors(colors);
                }
                return mesh;
            }
            finally
            {
                CollectionPool.Return(ref verts);
                CollectionPool.Return(ref norms);
                CollectionPool.Return(ref colors);
                CollectionPool.Return(ref uvs);
            }
        }

        private static GameObject CreateMeshAssetAndSubGameObject(string gameObjectName, UnpackPath meshAssetPath, GameObject parentGameObject, Mesh mesh, Material material)
        {
            if (meshAssetPath.FileExists())
            {
                AssetDatabase.AddObjectToAsset(mesh, meshAssetPath);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, meshAssetPath);
            }

            GameObject subGo = new GameObject(gameObjectName);
            subGo.transform.SetParent(parentGameObject.transform);
            subGo.isStatic = true;

            MeshFilter mf = subGo.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = subGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;

            return subGo;
        }

        private static void UnpackLocalTextures(GridProxy grid, FileMap mapFile)
        {
            grid.mapMaterials = MaterialRolodex.CreateInstance<MaterialRolodex>();
            grid.mapMaterials.texturesRolodex = grid.level.levelTextures;
            AssetDatabase.CreateAsset(grid.mapMaterials, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mats.asset", true));

            if (mapFile.GetTextureFileCount() == 0)
            {
                Debug.LogError("Map " + grid.name + " has no texture file");
            }
            else
            {
                grid.level.levelTextures.AddTextures(grid.fullName + "_tex_", mapFile.GetMainTextureFile());
            }
            if(mapFile.GetTextureFileCount() > 1)
            {
                Debug.LogError("Map " + grid.name + " has more than one texture file");
            }

            if (mapFile.GetGeometryFileCount() == 0)
            {
                Debug.LogError("Map " + grid.name + " has no geometry file");
            }
            else
            {
                grid.mapMaterials.AddMaterials(mapFile.GetMainGeometryFile());
            }
            if (mapFile.GetGeometryFileCount() > 1)
            {
                Debug.LogError("Map " + grid.name + " has more than one geometry file");
            }
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

        private static GameObject UnpackShadows(FileCollisions collisionFile)
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