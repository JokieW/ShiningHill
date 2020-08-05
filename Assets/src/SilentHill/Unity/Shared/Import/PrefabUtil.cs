using SH.Core;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace SH.Unity.Shared
{
    public static class PrefabUtil
    {
        public static void MakePrefab(ref GameObject prefab, UnpackPath dest, IReadOnlyList<GameObject> gameObjects, bool deleteGameObjectsOnError = false)
        {
            try
            {
                GameObject prefabinstance;
                if (dest.FileExists())
                {
                    prefabinstance = PrefabUtility.LoadPrefabContents(dest);
                    if (gameObjects != null)
                    {
                        CollectionPool.Request(out List<GameObject> toDelete);
                        foreach (Transform child in prefabinstance.transform)
                        {
                            for (int i = 0; i < gameObjects.Count; i++)
                            {
                                GameObject go = gameObjects[i];
                                if (child != null && child.name == go.name)
                                {
                                    toDelete.Add(child.gameObject);
                                    break;
                                }
                            }
                        }

                        for(int i = 0; i < toDelete.Count; i++)
                        {
                            GameObject.DestroyImmediate(toDelete[i]);
                        }
                        CollectionPool.Return(ref toDelete);
                    }
                }
                else
                {
                    prefabinstance = new GameObject(dest.nameWithoutExtension);
                    prefabinstance.isStatic = true;
                }

                if (gameObjects != null)
                {
                    for (int i = 0; i < gameObjects.Count; i++)
                    {
                        GameObject go = gameObjects[i];
                        if (go != null)
                        {
                            go.transform.SetParent(prefabinstance.transform, true);
                        }
                    }
                }

                if (dest.FileExists())
                {
                    prefab = PrefabUtility.SaveAsPrefabAsset(prefabinstance, dest);
                    PrefabUtility.UnloadPrefabContents(prefabinstance);
                }
                else
                {
                    prefab = PrefabUtility.SaveAsPrefabAsset(prefabinstance, dest);
                    GameObject.DestroyImmediate(prefabinstance);
                }
            }
            catch
            {
                if (deleteGameObjectsOnError && gameObjects != null)
                {
                    for(int i = 0; i < gameObjects.Count; i++)
                    {
                        GameObject go = gameObjects[i];
                        if (go != null)
                        {
                            GameObject.DestroyImmediate(gameObjects[i]);
                        }
                    }
                }
                throw;
            }
        }
    }
}
