using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

using SH.Unity.Shared;
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
                /*MapGeometry mapFile = GetMapFile(this);
                UnpackLocalTextures(this, mapFile.textureGroup);
                mapGO = UnpackMap(this, mapFile);*/
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

        private static GameObject UnpackCollisions(FileCollisions collisionFile)
        {
            GameObject colGo = new GameObject("Collisions");
            colGo.isStatic = true;
            try
            {
                MapCollisionComponent collisionComponent = colGo.AddComponent<MapCollisionComponent>();
                collisionComponent.collisions = collisionFile;
            }
            catch
            {
                GameObject.DestroyImmediate(colGo);
                throw;
            }

            return colGo;
        }

        public override void Pack()
        {

        }
    }
}