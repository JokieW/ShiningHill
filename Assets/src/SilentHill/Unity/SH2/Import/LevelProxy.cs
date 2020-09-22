using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using SH.Unity.Shared;
using SH.GameData.SH2;
using SH.Core;

namespace SH.Unity.SH2
{

    [CustomEditor(typeof(LevelProxy))]
    public class LevelProxyEditor : BaseImportProxyEditor { }

    public class LevelProxy : BaseImportProxy
    {
        public string levelName;
        public BGFolderProxy parentBGFolder;
        public UnpackPath levelPath;
        public SceneAsset scene;
        public TextureRolodex levelTextures;
        public GridProxy[] grids;
        public UnityEngine.Object map;
        public UnityEngine.Object GBcam;
        public UnityEngine.Object GBmap;
        public UnityEngine.Object GBfcl;
        public UnityEngine.Object nwfcl;
        public UnityEngine.Object parkfcl; //Only for cc

        public override void Unpack()
        {
            UnityEngine.Profiling.Profiler.BeginSample("UnpackLevel");

            Dictionary<string, GridProxy> newGrids = new Dictionary<string, GridProxy>();
            string[] files = Directory.GetFiles(levelPath);
            for (int i = 0; i < files.Length; i++)
            {
                UnpackPath filepath = new UnpackPath(files[i]);
                if (filepath.extension != ".meta")
                {
                    if (filepath.name == "park.fcl")
                    {
                        parkfcl = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                    }
                    else if (filepath.nameWithoutExtension.Substring(0, 2) == levelName)
                    {
                        string fileId = filepath.nameWithoutExtension.Substring(2);
                        string extension = filepath.extension;
                        if (String.IsNullOrEmpty(fileId) && extension == ".map")
                        {
                            map = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                        }
                        else if (fileId == "GB")
                        {
                            if (extension == ".cam") GBcam = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            else if (extension == ".map") GBmap = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            else if (extension == ".fcl") GBfcl = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                        }
                        else if (fileId == "nw")
                        {
                            if (extension == ".fcl") nwfcl = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                        }
                        else
                        {
                            GridProxy grid;
                            if (!newGrids.TryGetValue(fileId, out grid))
                            {
                                grid = GridProxy.CreateInstance<GridProxy>();
                                grid.level = this;
                                grid.gridName = fileId;
                                newGrids.Add(fileId, grid);
                            }

                            if (extension == ".map") grid.map = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            else if (extension == ".cam") grid.cam = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            else if (extension == ".cld") grid.cld = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            else if (extension == ".kg2") grid.kg2 = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            else if (extension == ".dmm") grid.dmm = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                        }
                    }
                }
            }

            bool wasAssetEditing = AssetUtil.IsAssetEditing();
            if (wasAssetEditing)
            {
                AssetUtil.StopAssetEditing();
            }

            if(map != null)
            {
                FileMap mapFile = FileMap.ReadMapFile(UnpackPath.GetPath(map));
                CollectionPool.Request(out List<SubFileTex> textures);
                mapFile.GetTextureFiles(textures);
                UnpackGlobalTextures(this, textures);
                CollectionPool.Return(ref textures);
            }

            grids = new GridProxy[newGrids.Count];
            int j = 0;
            try
            {
                foreach (KeyValuePair<string, GridProxy> kvp in newGrids)
                {
                    grids[j] = kvp.Value;
                    string name = levelName + kvp.Value.gridName;
                    if (EditorUtility.DisplayCancelableProgressBar("Creating grid...", name, (float)j / (float)grids.Length)) return;
                    AssetDatabase.CreateAsset(kvp.Value, UnpackPath.GetDirectory(this).WithName(name + ".asset"));
                    kvp.Value.MakePrefab();
                    j++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            UnpackPath scenePath = UnpackPath.GetDirectory(this).AddToPath("scene/").WithDirectoryAndName(UnpackDirectory.Unity, levelName + "_scene.unity");
            if (scene == null)
            {
                UnityEngine.SceneManagement.Scene sceneInstance = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                sceneInstance.name = levelName;
                GameObject root = new GameObject("Root");
                root.transform.localScale = new Vector3(0.002f, -0.002f, 0.002f);
                EditorSceneManager.MoveGameObjectToScene(root, sceneInstance);
                for (int i = 0; i < grids.Length; i++)
                {
                    PrefabUtility.InstantiatePrefab(grids[i].prefab, root.transform);
                }

                EditorSceneManager.SaveScene(sceneInstance, scenePath);
                scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            }
            else
            {
                UnityEngine.SceneManagement.Scene sceneInstance = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = Color.white;
                GameObject[] gos = sceneInstance.GetRootGameObjects();
                GameObject root = null;
                for (int i = 0; i < gos.Length; i++)
                {
                    GameObject go = gos[i];
                    if (go.name == "Root")
                    {
                        root = go;
                        foreach (Transform child in go.transform)
                        {
                            GameObject.DestroyImmediate(child.gameObject);
                        }
                    }
                }

                for (int i = 0; i < grids.Length; i++)
                {
                    PrefabUtility.InstantiatePrefab(grids[i].prefab, root.transform);
                }

                EditorSceneManager.SaveScene(sceneInstance);
                scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            }

            if (wasAssetEditing)
            {
                AssetUtil.StartAssetEditing();
            }

            if (unpackRecursive)
            {
                for (int i = 0; i < grids.Length; i++)
                {
                    grids[i].Unpack();
                }
            }
            EditorUtility.SetDirty(this);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        private static void UnpackGlobalTextures(LevelProxy level, List<SubFileTex> globalTextures)
        {
            level.levelTextures = TextureRolodex.CreateInstance<TextureRolodex>();
            AssetDatabase.CreateAsset(level.levelTextures, UnpackPath.GetDirectory(level).WithDirectoryAndName(UnpackDirectory.Unity, level.levelName + "_texs.asset", true));
            for (int i = 0; i < globalTextures.Count; i++)
            {
                level.levelTextures.AddTextures(level.levelName + "_tex_" + i, globalTextures[i]);
            }
        }

        public override void Pack()
        {

        }
    }
}