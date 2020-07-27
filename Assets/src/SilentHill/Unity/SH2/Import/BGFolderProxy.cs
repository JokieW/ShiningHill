using UnityEditor;
using UnityEngine;

using SH.Unity.Shared;
using System.IO;

namespace SH.Unity.SH2
{
    [CustomEditor(typeof(BGFolderProxy))]
    public class BGFolderProxyEditor : BaseImportProxyEditor { }

    [CanEditMultipleObjects]
    public class BGFolderProxy : RootFolderProxy
    {
        public LevelProxy[] levels;

        public override void Unpack()
        {
            string[] workFolders = Directory.GetDirectories(workFolderPath);
            levels = new LevelProxy[workFolders.Length];
            for (int i = 0; i < workFolders.Length; i++)
            {
                string workFolderPath = workFolders[i];
                LevelProxy level = CreateInstance<LevelProxy>();
                level.levelName = new DirectoryInfo(workFolderPath).Name;
                level.levelPath = new UnpackPath(workFolderPath);
                level.parentBGFolder = this;
                UnpackPath proxyTo = UnpackPath.GetDirectory(this).AddToPath(rootFolderName + "/" + level.levelName + "/").WithName(level.levelName + ".asset", true);
                AssetDatabase.CreateAsset(level, proxyTo);
                levels[i] = level;
                if (unpackRecursive)
                {
                    level.Unpack();
                }
            }

            EditorUtility.SetDirty(this);
        }

        public override void Pack()
        {
        }
    }
}