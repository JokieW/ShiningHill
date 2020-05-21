using System;
using System.IO;

using UnityEngine;
using UnityEditor;
using SH.Unity.Shared;

namespace SH.Unity.SH3
{
    [CustomEditor(typeof(SH3PCInstallImporter))]
    public class SH3PCInstallImporterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SH3PCInstallImporter handler = (SH3PCInstallImporter)target;
            DrawDefaultInspector();

            //Get path to disk
            if (GUILayout.Button("Locate clean install directory..."))
            {
                string path = EditorUtility.OpenFolderPanel("Select SH3 install directory", "", "");
                if (!String.IsNullOrEmpty(path))
                {
                    handler.cleanInstallPath = path + "/";
                }
            }

            //import
            GUI.enabled = !String.IsNullOrEmpty(handler.importName);
            if (!GUI.enabled)
            {
                EditorGUILayout.LabelField("Cannot import without a name for the import.");
            }
            if (GUILayout.Button("Import Source"))
            {
                handler.ImportSource();
            }
            GUI.enabled = true;

            //export
            GUI.enabled = !String.IsNullOrEmpty(handler.importName);
            if (!GUI.enabled)
            {
                EditorGUILayout.LabelField("Cannot export without an import name or an install path for the export.");
            }
            if (GUILayout.Button("Export Source"))
            {
                handler.ExportSource();
            }
            GUI.enabled = true;
        }
    }

    [CreateAssetMenu(fileName = "SH3PCInstallImporter", menuName = "Importers/SH3 PC Install")]
    public class SH3PCInstallImporter : SourceHandler
    {
        public string importName;
        public string cleanInstallPath;
        public string installPath;
        public bool unpackFromCleanInstall = true;
        public bool unpackRecursive = true;

        public ArcArcProxy arcArc;

        public override void ImportSource()
        {
            try
            {
                string pathToInstall = unpackFromCleanInstall ? cleanInstallPath : installPath;
                UnpackPath workDirectory = UnpackPath.GetWorkspaceDirectory(importName, true);
                UnpackPath proxyDirectory = UnpackPath.GetProxyDirectory(importName, true);

                //Copy exe
                {
                    string from = pathToInstall + "sh3.exe";
                    UnpackPath to = workDirectory.WithName("sh3.exe");
                    if (EditorUtility.DisplayCancelableProgressBar("Importing exe...", from, 0.5f)) return;
                    File.Copy(from, to);
                }

                //Copy data
                {
                    string from = pathToInstall + "data/";
                    UnpackPath to = workDirectory.AddToPath("data/");
                    if (EditorUtility.DisplayCancelableProgressBar("Importing data...", from, 0.5f)) return;
                    if (AssetUtil.DirectoryCopy(from, to, true, (s, f) => EditorUtility.DisplayCancelableProgressBar("Importing files...", s, f))) return;
                }

                AssetDatabase.Refresh();

                //Make arcarc proxy
                {
                    ArcArcProxy arcarc = CreateInstance<ArcArcProxy>();
                    arcarc.arcArc = AssetDatabase.LoadMainAssetAtPath(workDirectory.WithPathAndName("data/", "arc.arc"));
                    AssetDatabase.CreateAsset(arcarc, proxyDirectory.WithPathAndName("data/", "arc.arc.asset", true));
                    if (unpackRecursive)
                    {
                        try
                        {
                            AssetUtil.StartAssetEditing();
                            arcarc.Unpack();
                        }
                        finally
                        {
                            AssetUtil.StopAssetEditing();
                        }
                    }
                    arcArc = arcarc;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public override void ExportSource()
        {
            try
            {
                arcArc.Pack();
                UnpackPath from = UnpackPath.GetWorkspaceDirectory(importName).WithPath("data/");
                string to = installPath + "data/";

                string[] arcs = Directory.GetFiles(from);
                for (int i = 0; i < arcs.Length; i++)
                {
                    string arc = arcs[i];
                    if (Path.GetExtension(arc) == ".arc")
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Exporting data...", arc, (float)i / (float)arcs.Length)) return;
                        File.Copy(arc, to + Path.GetFileName(arc), true);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
