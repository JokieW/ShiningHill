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
                    if (extension == ".cld") { CollisionGroup.ReadCollisions(asset); continue; }
                    if (extension == ".kg2") { ShadowCasters.ReadShadowCasters(asset); continue; }
                }
                AssetDatabase.SaveAssets();
            }
        }

        public static MaterialRolodex ProcessTEX(string path)
        {
            if (File.Exists(path))
            {
                Texture2D[] textures = TextureUtils.ReadTex32(Path.GetFileName(path).Replace(".tex", "_tex"), new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)));

                MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(path.Replace(".tex", ".asset"));
                rolodex.AddTextures(textures);
                return rolodex;
            }
            return null;
        }
    }
}