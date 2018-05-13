using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ShiningHill
{
    public class ArcExporter : EditorWindow
    {
        //static EditorWindow _currentWindow;

        [MenuItem("ShiningHill/ArcExporter")]
        public static void ShowWindow()
        {
            /*_currentWindow = */EditorWindow.GetWindow(typeof(ArcExporter));
        }

        void OnGUI()
        {
            if (GUILayout.Button("Open"))
            {
                new ArcFileSystem();
            }
            if (GUILayout.Button("Load exe"))
            {
                SH3exeExtractor.ExtractRegionEventData();
            }
        }
    }
}