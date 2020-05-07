using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;

using SH.Unity.Shared;

namespace SH.Unity.SH2
{
    [CustomEditor(typeof(SH2PCInstallImporter))]
    public class SH2PCInstallImporterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SH2PCInstallImporter handler = (SH2PCInstallImporter)target;
            DrawDefaultInspector();

            //Get path to disk
            if (GUILayout.Button("Locate clean install directory..."))
            {
                string path = EditorUtility.OpenFolderPanel("Select SH2 install directory", "", "");
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

    [CreateAssetMenu(fileName = "SH2PCInstallImporter", menuName = "Importers/SH2 PC Install")]
    public class SH2PCInstallImporter : SourceHandler
    {
        public string importName;
        public string cleanInstallPath;
        public string installPath;
        public bool unpackFromCleanInstall = true;
        public bool unpackRecursive = true;

        public UnityEngine.Object[] files;

        public override void ImportSource()
        {
            try
            {
                string pathToInstall = unpackFromCleanInstall ? cleanInstallPath : installPath;
                UnpackPath workDirectory = UnpackPath.GetWorkspaceDirectory(importName, true);
                UnpackPath proxyDirectory = UnpackPath.GetProxyDirectory(importName, true);

                //Copy exe
                {
                    string from = pathToInstall + "sh2pc.exe";
                    UnpackPath to = workDirectory.WithName("sh2pc.exe");
                    if (EditorUtility.DisplayCancelableProgressBar("Importing exe...", from, 0.5f)) return;
                    File.Copy(from, to);
                }

                //Copy data
                {
                    string from = pathToInstall + "data/";
                    UnpackPath to = workDirectory.WithPath("data/");
                    if (EditorUtility.DisplayCancelableProgressBar("Importing data...", from, 0.5f)) return;
                    if (AssetUtil.DirectoryCopy(from, to, true, (s, f) => EditorUtility.DisplayCancelableProgressBar("Importing files...", s, f))) return;
                }

                AssetDatabase.Refresh();

                //Make arcarc proxy
                {
                    UnpackPath to = workDirectory.WithPath("data/");
                    List<UnityEngine.Object> fileList = new List<UnityEngine.Object>();
                    foreach (string file in Directory.EnumerateFiles(to, "*.*", SearchOption.AllDirectories))
                    {
                        if (!file.Contains(".mkimage") && Path.GetExtension(file) != ".meta")
                        {
                            fileList.Add(AssetDatabase.LoadMainAssetAtPath(file));
                        }
                    }
                    files = fileList.ToArray();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public override void ExportSource()
        {
            /*try
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
            }*/
        }
    }
}
