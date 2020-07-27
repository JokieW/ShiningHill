using System.IO;

using UnityEditor;
using UnityEngine;

using SH.Unity.Shared;

namespace SH.Unity.SH2
{
    [CustomEditor(typeof(GenericFolderProxy))]
    public class GenericFolderProxyEditor : BaseImportProxyEditor { }

    [CanEditMultipleObjects]
    public class GenericFolderProxy : RootFolderProxy
    {
        public UnityEngine.Object[] unityFiles;

        public override void Unpack()
        {
            string[] workFiles = Directory.GetFiles(workFolderPath, "*.*", SearchOption.AllDirectories);
            unityFiles = new UnityEngine.Object[workFiles.Length];
            for (int i = 0; i < workFiles.Length; i++)
            {
                unityFiles[i] = AssetDatabase.LoadMainAssetAtPath(workFiles[i]);
            }

            EditorUtility.SetDirty(this);
        }

        public override void Pack()
        {
        }
    }
}