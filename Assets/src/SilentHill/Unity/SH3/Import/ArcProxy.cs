using UnityEditor;
using UnityEngine;

using SH.GameData.SH3;
using SH.Unity.Shared;

namespace SH.Unity.SH3
{
    [CustomEditor(typeof(ArcProxy))]
    [CanEditMultipleObjects]
    public class ArcProxyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            //Unpack
            if (GUILayout.Button("Unpack"))
            {
                try
                {
                    AssetUtil.StartAssetEditing();
                    for (int i = 0; i < targets.Length; i++)
                    {
                        ArcProxy proxy = (ArcProxy)targets[i];
                        proxy.arcarc.GetRoot(out FileArcArc.Root root);
                        root.GetFolder(proxy.arcName, out FileArcArc.Root.Folder folder);
                        proxy.Unpack(in folder, true);
                    }
                }
                finally
                {
                    AssetUtil.StopAssetEditing();
                }
            }

            //Pack
            if (GUILayout.Button("Pack"))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    ArcProxy proxy = (ArcProxy)targets[i];
                    UnpackPath basePath = proxy.GetDatalessPath();
                    FileArcArc.MakeArcArcInfo(basePath, proxy.GetMap(), out FileArcArc.Root.Folder folder);
                    proxy.Pack(in folder);
                }
            }
        }
    }

    [CreateAssetMenu(fileName = "SH3ArcImportProxy", menuName = "Import Proxies/SH3 Arc")]
    [CanEditMultipleObjects]
    public class ArcProxy : BaseImportProxy
    {
        public ArcArcProxy arcarc;
        public UnityEngine.Object arc;
        public string arcName;
        public bool unpackRecursive = true;

        public LevelProxy level;
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

        public void ImportAssets(in FileArcArc.Root.Folder extractedFolder)
        {
            UnpackPath rootPath = UnpackPath.GetDirectory(arc).WithPath("");
            files = new UnityEngine.Object[extractedFolder.files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = AssetDatabase.LoadMainAssetAtPath(rootPath.WithMixedPath(extractedFolder.files[i].entry.name));
            }

            if (arcName.Length == 4 && arcName.Substring(0, 2) == "bg")
            {
                level = CreateInstance<LevelProxy>();
                level.levelName = arcName.Substring(2);
                level.parentArc = this;
                UnpackPath proxyTo = UnpackPath.GetDirectory(arc).AddToPath(arcName + "/").WithDirectoryAndName(UnpackDirectory.Proxy, arcName.Substring(2) + ".asset", true);
                AssetDatabase.CreateAsset(level, proxyTo);
                if (unpackRecursive)
                {
                    level.Unpack();
                }
            }
        }

        public void Unpack(in FileArcArc.Root.Folder extractedFolder, bool doAssetImport)
        {
            UnityEngine.Profiling.Profiler.BeginSample("UnpackArc");
            UnpackPath rootPath = UnpackPath.GetDirectory(arc).WithPath("");
            FileArc.UnpackArc(in extractedFolder, UnpackPath.GetPath(arc), rootPath);
            if (doAssetImport)
            {
                ImportAssets(in extractedFolder);
            }
            EditorUtility.SetDirty(this);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void Pack(in FileArcArc.Root.Folder folder)
        {
            FileArc.PackArc(in folder, GetDatalessPath(), UnpackPath.GetPath(arc));
        }
    }
}
