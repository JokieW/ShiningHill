using ShiningHill;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SH3ArcImportProxy))]
[CanEditMultipleObjects]
public class SH3ArcImportProxyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //Unpack
        if (GUILayout.Button("Unpack"))
        {
            try
            {
                ExplorerUtil.StartAssetEditing();
                for (int i = 0; i < targets.Length; i++)
                {
                    SH3ArcImportProxy proxy = (SH3ArcImportProxy)targets[i];
                    proxy.arcarc.GetRoot(out ArcFileSystem.Root root);
                    root.GetFolder(proxy.arcName, out ArcFileSystem.Root.Folder folder);
                    proxy.Unpack(in folder, true);
                }
            }
            finally
            {
                ExplorerUtil.StopAssetEditing();
            }
        }

        //Pack
        if (GUILayout.Button("Pack"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                SH3ArcImportProxy proxy = (SH3ArcImportProxy)targets[i];
                UnpackPath basePath = proxy.GetDatalessPath();
                ArcFileSystem.MakeArcArcInfo(basePath, proxy.GetMap(), out ArcFileSystem.Root.Folder folder);
                proxy.Pack(in folder);
            }
        }
    }
}

[CreateAssetMenu(fileName = "SH3ArcImportProxy", menuName = "Import Proxies/SH3 Arc")]
[CanEditMultipleObjects]
public class SH3ArcImportProxy : BaseImportProxy
{
    public SH3ArcArcImportProxy arcarc;
    public UnityEngine.Object arc;
    public string arcName;
    public bool unpackRecursive = true;

    public SH3LevelProxy level;
    public UnityEngine.Object[] files;

    public UnpackPath GetDatalessPath()
    {
        return UnpackPath.GetDirectory(arc).WithPath("");
    }

    public (string, string[]) GetMap()
    {
        string[] filesMap = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            filesMap[i] = UnpackPath.GetPath(files[i]).GetRelativePath();
        }
        return (arcName, filesMap);
    }

    public void ImportAssets(in ArcFileSystem.Root.Folder extractedFolder)
    {
        UnpackPath rootPath = UnpackPath.GetDirectory(arc).WithPath("");
        files = new UnityEngine.Object[extractedFolder.files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = AssetDatabase.LoadMainAssetAtPath(rootPath.WithMixedPath(extractedFolder.files[i].entry.name));
        }

        if (arcName.Length == 4 && arcName.Substring(0, 2) == "bg")
        {
            level = CreateInstance<SH3LevelProxy>();
            level.levelName = arcName.Substring(2);
            level.parentArc = this;
            UnpackPath proxyTo = UnpackPath.GetDirectory(arc).AddToPath(arcName + "/").WithDirectoryAndName(UnpackDirectory.Proxy, arcName.Substring(2) + ".asset", true);
            AssetDatabase.CreateAsset(level, proxyTo);
            if(unpackRecursive)
            {
                level.Unpack();
            }
        }
    }

    public void Unpack(in ArcFileSystem.Root.Folder extractedFolder, bool doAssetImport)
    {
        UnityEngine.Profiling.Profiler.BeginSample("UnpackArc");
        UnpackPath rootPath = UnpackPath.GetDirectory(arc).WithPath("");
        ArcFileSystem.UnpackArc(in extractedFolder, UnpackPath.GetPath(arc), rootPath);
        if (doAssetImport)
        {
            ImportAssets(in extractedFolder);
        }
        EditorUtility.SetDirty(this);
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public void Pack(in ArcFileSystem.Root.Folder folder)
    {
        ArcFileSystem.PackArc(in folder, GetDatalessPath(), UnpackPath.GetPath(arc));
    }
}
