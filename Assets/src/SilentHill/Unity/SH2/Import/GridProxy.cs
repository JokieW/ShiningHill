using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using SH.Unity.Shared;
using SH.GameData.SH2;
using SH.GameData.Shared;
using SH.Core;

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
                UnpackLocalTextures(this, mapFile.GetMainTextureFile());
                GameObject go = UnpackMap(mapFile, this);
                go.AddComponent<MapFileComponent>().SetMapFile(mapFile);
                gameObjects.Add(go);
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
            GameObject topGo = new GameObject("Geometry") { isStatic = true };

            UnpackPath meshAssetPath = UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mesh.asset", true);
            if (meshAssetPath.FileExists())
            {
                AssetDatabase.DeleteAsset(meshAssetPath);
            }

            try
            {
                FileGeometry meshFile = mapFile.GetMainGeometryFile();

                if (meshFile != null)
                {
                    for (int i = 0; i < meshFile.geometries.Length; i++)
                    {
                        GameObject geometryGo = new GameObject("Geometry " + i.ToString("X")) { isStatic = true };
                        geometryGo.transform.SetParent(topGo.transform);

                        FileGeometry.Geometry geo = meshFile.geometries[i];
                        for (int j = 0; j < 2; j++)
                        {
                            GameObject meshGroupGo = new GameObject("MeshGroup " + j.ToString("X")) { isStatic = true };
                            meshGroupGo.transform.SetParent(geometryGo.transform);

                            FileGeometry.Geometry.MeshGroup meshGroup = j == 0 ? geo.meshGroup0 : geo.meshGroup1;
                            if (meshGroup != null)
                            {
                                for (int k = 0; k < meshGroup.subMeshGroups.Length; k++)
                                {
                                    GameObject subMeshGroupGo = new GameObject("SubMeshGroup " + k.ToString("X")) { isStatic = true };
                                    subMeshGroupGo.transform.SetParent(meshGroupGo.transform);

                                    int indicesIndex = 0;
                                    FileGeometry.Geometry.MeshGroup.SubMeshGroup subMeshGroup = meshGroup.subMeshGroups[k];
                                    for (int l = 0; l < subMeshGroup.subSubMeshGroups.Length; l++)
                                    {
                                        GameObject subSubMeshGroupGo = new GameObject("SubSubMeshGroup " + l.ToString("X")) { isStatic = true };
                                        subSubMeshGroupGo.transform.SetParent(subMeshGroupGo.transform);

                                        FileGeometry.Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup subSubMeshGroup = subMeshGroup.subSubMeshGroups[l];
                                        int sectionId = subSubMeshGroup.header.sectionId;
                                        int vertexSize = subMeshGroup.vertexSections[sectionId].vertexSize;
                                        for (int m = 0; m < subSubMeshGroup.meshParts.Length; m++)
                                        {
                                            FileGeometry.Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup.MeshPart meshPart = subSubMeshGroup.meshParts[m];
                                            Mesh mesh = MakeSubMeshFromIndices(
                                                vertexSize,
                                                meshPart,
                                                subMeshGroup.vertices[sectionId],
                                                grid.fullName + "_meshpart_" + i + "_" + j + "_" + k + "_" + l + "_" + m + "_" + subSubMeshGroup.header.materialIndex,
                                                ref indicesIndex,
                                                subMeshGroup.indices);
                                            Material material = GetMaterial(grid.localTextures, grid.level.levelMaterials, meshFile.materials[subSubMeshGroup.header.materialIndex], MaterialRolodex.MaterialType.Diffuse);

                                            GameObject meshPartGo = CreateMeshAssetAndSubGameObject("MeshPart " + m.ToString("X"), meshAssetPath, subSubMeshGroupGo, mesh, material);
                                            meshPartGo.AddComponent<MapSubMeshComponent>().subMesh = subSubMeshGroup;
                                        }
                                    }
                                }
                            }
                        }

                        if (geo.mapDecorations != null)
                        {
                            FileGeometry.Geometry.MapDecorations.Decoration[] decorations = geo.mapDecorations.decorations;
                            for (int k = 0; k < decorations.Length; k++)
                            {
                                FileGeometry.Geometry.MapDecorations.Decoration decoration = decorations[k];

                                GameObject decGo = new GameObject("Decoration") { isStatic = true };
                                decGo.transform.SetParent(geometryGo.transform);
                                decGo.AddComponent<DecorationComponent>().decoration = decoration;

                                for (int l = 0, indicesIndex = 0; l < decoration.subDecorations.Length; l++)
                                {
                                    FileGeometry.Geometry.MapDecorations.Decoration.SubDecoration subDecoration = decoration.subDecorations[l];
                                    int vertexSize = decoration.vertexSections[subDecoration.sectionId].vertexSize;
                                    Mesh mesh = MakeSubMeshFromIndices(vertexSize, decoration.vertices[subDecoration.sectionId], grid.fullName + "_subdecoration_" + k, subDecoration.stripLength, subDecoration.stripCount, ref indicesIndex, decoration.indices);
                                    Material material = GetMaterial(grid.localTextures, grid.level.levelMaterials, meshFile.materials[subDecoration.materialIndex]);
                                    GameObject subGo = CreateMeshAssetAndSubGameObject("SubDecoration", meshAssetPath, decGo, mesh, material);
                                    subGo.AddComponent<SubDecorationComponent>().subDecoration = subDecoration;
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

        private static Material GetMaterial(MaterialRolodex local, MaterialRolodex level, FileGeometry.MeshMaterial meshMaterial)
        {
            Material material = null;
            if(local != null)
            {
                material = local.GetOrCreateMaterial(meshMaterial.textureId);
            }
            if (level != null && material == null)
            {
                material = level.GetOrCreateMaterial(meshMaterial.textureId);
            }
            return material;
        }

        private static Material GetMaterial(MaterialRolodex local, MaterialRolodex level, FileGeometry.MeshMaterial meshMaterial, MaterialRolodex.MaterialType materialType)
        {
            Material material = null;
            if (local != null)
            {
                material = local.GetOrCreateMaterial(meshMaterial.textureId, materialType);
            }
            if (level != null && material == null)
            {
                material = level.GetOrCreateMaterial(meshMaterial.textureId, materialType);
            }
            return material;
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

        private static Mesh MakeSubMeshFromIndices(int vertexSize, FileGeometry.Geometry.MeshGroup.SubMeshGroup.SubSubMeshGroup.MeshPart subSubMesh, byte[] vertices, string meshName, ref int indicesIndex, ushort[] indices)
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
                FileGeometry.Geometry.UnpackVertices(vertexSize, vertices, verts, norms, uvs, colors);
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

        private static void UnpackLocalTextures(GridProxy grid, in FileTex localTextures)
        {
            grid.localTextures = MaterialRolodex.CreateInstance<MaterialRolodex>();
            AssetDatabase.CreateAsset(grid.localTextures, UnpackPath.GetDirectory(grid).WithDirectoryAndName(UnpackDirectory.Unity, grid.fullName + "_mats.asset", true));
            grid.localTextures.ReadAndAddTexDXT1(grid.fullName + "_tex_", localTextures);
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