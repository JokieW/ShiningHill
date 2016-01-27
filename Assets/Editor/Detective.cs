using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class Detective : EditorWindow
{
    static EditorWindow _currentWindow;

    [MenuItem("SilentParty/Detective")]
    public static void ShowWindow()
    {
        _currentWindow = EditorWindow.GetWindow(typeof(Detective));
    }

    bool _initialised = false;
    static Archive _currentArchive;
    Object _currentFile;

    void Init()
    {
        _currentArchive = new Archive(null);
        _initialised = true;
    }

    void OnGUI()
    {
        if (!_initialised || _currentArchive == null)
        {
            Init();
        }

        _currentFile = EditorGUILayout.ObjectField("Archive", _currentFile, typeof(Object), false);

        if (GUILayout.Button("Open"))
        {
            _currentArchive.File = _currentFile;
            _currentArchive.OpenArchive();
        }
        if (_currentArchive.AllFiles != null)
        {
            foreach (KeyValuePair<string, Archive.ArcFile> kvp in _currentArchive.AllFiles)
            {
                GUILayout.Label("__________________________");
                GUILayout.Label(kvp.Key);
                GUILayout.Label("Offset " + kvp.Value.Offset);
                GUILayout.Label("FileID " + kvp.Value.FileID);
                GUILayout.Label("Length " + kvp.Value.Length);
                GUILayout.Label("Length2 " + kvp.Value.Lenght2);
            }
        }

    }
}