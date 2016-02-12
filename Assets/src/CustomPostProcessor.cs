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

            for (int i = 0; i != textures.Length; i++)
            {

                Material mat = new Material(defaultDiffuseMat);
                mat.name = Path.GetFileName(path)+".mat_"+i;
                textures[i].name = Path.GetFileName(path) + ".tex_" + i;

                mat.mainTexture = textures[i];

                if (i == 0)
                {
                    AssetDatabase.CreateAsset(textures[i], path.Replace(".tex", ".asset"));
                }
                else
                {
                    AssetDatabase.AddObjectToAsset(textures[i], path.Replace(".tex", ".asset"));
                }
                AssetDatabase.AddObjectToAsset(mat, path.Replace(".tex", ".asset"));
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.SaveAssets();
        }
    }
}