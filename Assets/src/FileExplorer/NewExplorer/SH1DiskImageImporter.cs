using ShiningHill;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SH1DiskImageImporter))]
public class SH1DiskImageImporterEditor : Editor
{
    static readonly string[] FILE_FILTERS = new string[] { "Bin/Cue", "cue" };

    public override void OnInspectorGUI()
    {
        SH1DiskImageImporter handler = (SH1DiskImageImporter)target;
        DrawDefaultInspector();

        //Get path to disk
        if (GUILayout.Button("Locate clean disk image..."))
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Select SH1 Disk Image", "", FILE_FILTERS);
            if(!String.IsNullOrEmpty(path))
            {
                handler.cleanDiskImage = path;
            }
        }

        //Unpack 
        GUI.enabled = !String.IsNullOrEmpty(handler.sourceName);
        if (!GUI.enabled)
        {
            EditorGUILayout.LabelField("Cannot unpack without a name for the import.");
        }
        if (GUILayout.Button("Unpack"))
        {
            handler.ImportSource();
        }
        GUI.enabled = true;
    }
}

[CreateAssetMenu(fileName = "SH1DiskImageImporter", menuName = "Importers/SH1 Disk Image")]
public class SH1DiskImageImporter : SourceHandler
{
    public string sourceName;
    public string cleanDiskImage;
    public string DiskImagePath;
    public bool unpackFromCleanDiskImage = true;

    public override void ImportSource()
    {
        try
        {
            string pathToISO = unpackFromCleanDiskImage ? cleanDiskImage : DiskImagePath;
            UnpackPath workDirectory = UnpackPath.GetWorkspaceDirectory(sourceName, true);
            string tempDirectory = Path.GetTempPath() + "/";

            //Unpack cue
            {
                string from = pathToISO;
                UnpackPath to = workDirectory;
                if (EditorUtility.DisplayCancelableProgressBar("Extracting disk image...", from, 0.5f)) return;

                using (CDROMStream cd = CDROMStream.MakeFromCue(from))
                using (ISO9660FS fs = new ISO9660FS(cd))
                {
                    fs.ExtractFiles(to);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    public override void ExportSource()
    {
        throw new System.NotImplementedException();
    }
}
