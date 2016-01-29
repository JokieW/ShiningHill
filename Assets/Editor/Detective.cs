using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    int _selectedFile;
    string[] _fileNames;
    Vector2 _scrollPosition, _hexScroll;
    HexDisplay.DisplayType _hexDisplayStyle = HexDisplay.DisplayType.ANSI;

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

        if (GUILayout.Button("Test"))
        {
            for (int i = 0; i != 256; i++)
            {
                Debug.Log(i.ToString() + " [" + System.Convert.ToChar(i) + "]");
            }
        }

        if (GUILayout.Button("Open"))
        {
            _currentArchive.File = _currentFile;
            _currentArchive.OpenArchive();
            _selectedFile = 0;
            _fileNames = _currentArchive.AllFiles.Select(x => x.Key).ToArray();
            _scrollPosition = Vector2.zero;
        }
        if (_currentArchive.AllFiles != null)
        {
            GUILayout.BeginHorizontal();

            //File selection
            GUILayout.BeginVertical(GUILayout.Width(150.0f));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

            EditorGUI.BeginChangeCheck();
            _selectedFile = GUILayout.SelectionGrid(_selectedFile, _fileNames, 1);
            if(EditorGUI.EndChangeCheck())
            {
                _hexScroll = Vector2.zero;
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // File info
            GUILayout.BeginVertical();

            Archive.ArcFile file = _currentArchive.AllFiles[_fileNames[_selectedFile]];
            GUILayout.Label("Offset " + file.Offset);
            GUILayout.Label("FileID " + file.FileID);
            GUILayout.Label("Length " + file.Length);
            GUILayout.Label("Length2 " + file.Lenght2);
            GUILayout.Label("Type UNKNOWN");
            _hexDisplayStyle = (HexDisplay.DisplayType)EditorGUILayout.EnumPopup("Preview format", _hexDisplayStyle);

            _hexScroll = HexDisplay.Display(_hexScroll, file.data, 16, 4, _hexDisplayStyle, null);

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        
        }

    }
}