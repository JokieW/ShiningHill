using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
	public class Scene : MonoBehaviour 
	{
        public static GameObject BeginEditingPrefab(string path, string childName)
        {
            string directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            GameObject prefab = (GameObject)AssetDatabase.LoadMainAssetAtPath(path);
            GameObject prefabGo = null;
            if (prefab == null)
            {
                prefabGo = new GameObject("Sector");
                prefabGo.isStatic = true;
            }
            else
            {
                prefabGo = Instantiate((GameObject)prefab);
                PrefabUtility.DisconnectPrefabInstance(prefabGo);
                Transform existingMap = prefabGo.transform.Find(childName);
                if (existingMap != null)
                {
                    DestroyImmediate(existingMap.gameObject);
                }
            }

            GameObject subGO = new GameObject(childName);
            subGO.transform.SetParent(prefabGo.transform);
            subGO.isStatic = true;
            AssetDatabase.StartAssetEditing();

            return subGO;
        }

        public static void FinishEditingPrefab(string path, GameObject subGO)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject prefabGO = subGO.transform.parent.gameObject;
            if (prefab != null)
            {
                PrefabUtility.ReplacePrefab(prefabGO, prefab);
            }
            else
            {
                PrefabUtility.CreatePrefab(path, prefabGO);
            }
            AssetDatabase.StopAssetEditing();
            EditorUtility.SetDirty(prefabGO);

            AssetDatabase.SaveAssets();
            Debug.Log("Does it contain " + path + " " + AssetDatabase.Contains(subGO));

            DestroyImmediate(prefabGO, false);
            Debug.Log("Does it still contain " + path + " " + AssetDatabase.Contains(subGO));
        }
	}
}