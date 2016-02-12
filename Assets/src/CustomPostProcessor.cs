using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{
    public class CustomPostprocessor : AssetPostprocessor
    {
        public static bool DoImports = true;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            if (DoImports)
            {
                foreach (string asset in importedAssets)
                {
                    string extension = Path.GetExtension(asset);

                    if (extension == ".tex") { ProcessTEX(asset); continue; }
                    if (extension == ".map") { Scene.ReadMap(asset); continue; }
                }
            }
        }

        static void ProcessTEX(string path)
        {
            Texture2D[] textures = TextureReaders.ReadTex32(new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)));

            Material defaultDiffuseMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DefaultDiffuseMaterial.mat");

            string prefabPath = path.Replace(".tex", ".prefab");

            UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            for (int i = textures.Length-1; i != -1; i--)
            {
                Material mat = new Material(defaultDiffuseMat);
                mat.name = Path.GetFileName(path).Replace(".tex", "_mat_"+i);
                textures[i].name = Path.GetFileName(path).Replace(".tex", "_tex_" + i);

                mat.mainTexture = textures[i];

                if (i == 0)
                {
                    go.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }

                AssetDatabase.AddObjectToAsset(textures[i], prefabPath);
                AssetDatabase.AddObjectToAsset(mat, prefabPath);
            }

            PrefabUtility.ReplacePrefab(go, prefab);
            GameObject.DestroyImmediate(go);

            AssetDatabase.SaveAssets();

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(prefabPath))
            {
                Debug.Log(obj);
            }
        }
    }
}