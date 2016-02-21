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
        public static readonly float GLOBAL_SCALE = 0.002f;

        public Matrix4x4 GetSH3ToUnityMatrix()
        {
            Map map = GetComponent<Map>();
            if (map != null)
            {
                return Matrix4x4.TRS(map.transform.localPosition, map.transform.localRotation, new Vector3(GLOBAL_SCALE, GLOBAL_SCALE, GLOBAL_SCALE));
            }
            return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(GLOBAL_SCALE, GLOBAL_SCALE, GLOBAL_SCALE));
        }

        public static GameObject BeginEditingPrefab(string path, string childName)
        {
            string prefabPath = path.Replace(Path.GetExtension(path), ".prefab");

            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            GameObject prefabGo = null;
            GameObject subGO = null;

            if (prefab == null)
            {
                prefabGo = new GameObject("Area");
                prefabGo.AddComponent<Scene>();
                prefabGo.isStatic = true;
            }
            else
            {
                prefabGo = (GameObject)GameObject.Instantiate(prefab);
                PrefabUtility.DisconnectPrefabInstance(prefabGo);
                Transform existingMap = prefabGo.transform.FindChild(childName);
                if (existingMap != null)
                {
                    DestroyImmediate(existingMap.gameObject);
                }
            }

            prefabGo.transform.localScale = Vector3.one;
            subGO = new GameObject(childName);
            subGO.transform.SetParent(prefabGo.transform);
            subGO.isStatic = true;

            return subGO;
        }

        public static void FinishEditingPrefab(string path, GameObject subGO)
        {
            string prefabPath = path.Replace(Path.GetExtension(path), ".prefab");
            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            GameObject prefabGO = subGO.transform.parent.gameObject;
            prefabGO.transform.localScale = Vector3.one;//new Vector3(GLOBAL_SCALE, GLOBAL_SCALE, GLOBAL_SCALE);

            if (prefab != null)
            {
                PrefabUtility.ReplacePrefab(prefabGO, prefab);
            }
            else
            {
                PrefabUtility.CreatePrefab(prefabPath, prefabGO);
            }

            AssetDatabase.SaveAssets();

            DestroyImmediate(prefabGO, false);
        }
	}
}