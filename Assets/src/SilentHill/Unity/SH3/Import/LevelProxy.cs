using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SH.Unity.Shared;
using SH.GameData.SH3;

namespace SH.Unity.SH3
{
    [CustomEditor(typeof(LevelProxy))]
    [CanEditMultipleObjects]
    public class LevelProxyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            //Unpack
            if (GUILayout.Button("Unpack"))
            {
                try
                {
                    //AssetUtil.StartAssetEditing();
                    for (int i = 0; i < targets.Length; i++)
                    {
                        LevelProxy proxy = (LevelProxy)targets[i];
                        proxy.Unpack();
                    }
                }
                finally
                {
                    //AssetUtil.StopAssetEditing();
                }
            }

            //Pack
            if (GUILayout.Button("Pack"))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    LevelProxy proxy = (LevelProxy)targets[i];
                    proxy.Pack();
                }
            }
        }
    }

    public class LevelProxy : BaseImportProxy
    {
        public string levelName;
        public ArcProxy parentArc;
        public SceneAsset scene;
        public GridProxy[] grids;
        public bool unpackRecursive = true;
        public UnityEngine.Object GBtex;
        public UnityEngine.Object GBcam;
        public UnityEngine.Object GBfcl;
        public UnityEngine.Object nwfcl;

        public MaterialRolodex GBTextures;

        public void Unpack()
        {
            UnityEngine.Profiling.Profiler.BeginSample("UnpackLevel");

            bool hasGlobalTR = false;
            UnpackPath globalTR = default;
            Dictionary<string, GridProxy> newGrids = new Dictionary<string, GridProxy>();
            for (int i = 0; i < parentArc.files.Length; i++)
            {
                UnpackPath filepath = UnpackPath.GetPath(parentArc.files[i]);
                if (filepath.nameWithoutExtension.Substring(0, 2) == levelName)
                {
                    if (filepath.name == "cc01TR.tex")
                    {
                        hasGlobalTR = true;
                        globalTR = filepath;
                    }
                    else
                    {

                        string gridName = filepath.name.Substring(2, 2);
                        string extension = filepath.extension;
                        if (gridName == "GB")
                        {
                            if (extension == ".tex") GBtex = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            else if (extension == ".cam") GBcam = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            else if (extension == ".fcl") GBfcl = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                        }
                        else if (gridName == "nw")
                        {
                            if (extension == ".fcl") nwfcl = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                        }
                        else
                        {
                            if (filepath.name.EndsWith("TR.tex") || filepath.nameWithoutExtension.Length == 4)
                            {
                                GridProxy grid;
                                if (!newGrids.TryGetValue(gridName, out grid))
                                {
                                    grid = GridProxy.CreateInstance<GridProxy>();
                                    grid.level = this;
                                    grid.gridName = gridName;
                                    newGrids.Add(gridName, grid);
                                }

                                if (extension == ".map") grid.map = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                                else if (extension == ".cam") grid.cam = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                                else if (extension == ".cld") grid.cld = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                                else if (extension == ".kg2") grid.kg2 = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                                else if (extension == ".ded") grid.ded = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                                else if (extension == ".tex") grid.TRtex = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filepath);
                            }
                        }
                    }
                }
            }

            if(hasGlobalTR)
            {
                foreach(KeyValuePair<string, GridProxy> kvp in newGrids)
                {
                    kvp.Value.TRtex = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(globalTR);
                }
            }

            if (GBtex != null)
            {
                TextureGroup trGroup;
                using (FileStream file = new FileStream(UnpackPath.GetPath(GBtex), FileMode.Open, FileAccess.ReadWrite))
                using (BinaryReader reader = new BinaryReader(file))
                {
                    trGroup = TextureGroup.ReadTextureGroup(reader);
                }
                GBTextures = ScriptableObject.CreateInstance<MaterialRolodex>();
                UnpackPath toPath = UnpackPath.GetDirectory(this).WithDirectoryAndName(UnpackDirectory.Unity, levelName + "GB_mats.asset", true);
                AssetDatabase.CreateAsset(GBTextures, toPath);
                GBTextures.AddTextures(MaterialRolodex.ReadTex32(levelName + "GB_tex", in trGroup));
            }

            bool wasAssetEditing = AssetUtil.IsAssetEditing();
            if (wasAssetEditing)
            {
                AssetUtil.StopAssetEditing();
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

        public void Pack()
        {

        }
    }
}
