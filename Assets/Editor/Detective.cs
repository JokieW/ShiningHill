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
    public class Detective : EditorWindow
    {
        //static EditorWindow _currentWindow;

        [MenuItem("ShiningHill/Detective")]
        public static void ShowWindow()
        {
            /*_currentWindow = */EditorWindow.GetWindow(typeof(Detective));
        }

        bool _initialised = false;
        static ArcFileSystem _currentArchive;
        Object _currentFile;
        byte[] _binaryFile;

        int _selectedFile;
        string[] _fileNames;
        Vector2 _scrollPosition;

        //Hex display
        Vector2 _hexScroll;

        void Init()
        {
            
            _initialised = true;
        }

        void LoadFile()
        {
            string assetPath = AssetDatabase.GetAssetPath(_currentFile);
            BinaryReader reader = new BinaryReader(new FileStream(assetPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            _binaryFile = new byte[reader.BaseStream.Length];
            reader.Read(_binaryFile, 0, (int)reader.BaseStream.Length);

        }

        void OnGUI()
        {
            if (!_initialised)
            {
                Init();
            }

            EditorGUI.BeginChangeCheck();
            _currentFile = EditorGUILayout.ObjectField(_currentFile, typeof(Object), true);
            if(EditorGUI.EndChangeCheck())
            {
                LoadFile();
            }

            if (_binaryFile != null)
            {
                _hexScroll = HexDisplay.Display(_hexScroll, _binaryFile, null);
            }


            /*if (GUILayout.Button("Open"))
            {
                _currentArchive = new ArcFileSystem();
                /*_currentArchive.File = _currentFile;
                _currentArchive.OpenArchive();
                _selectedFile = 0;
                //_fileNames = _currentArchive.AllFiles.Select(x => x.Key).ToArray();
                _scrollPosition = Vector2.zero;
            }*/

            /*if (_currentArchive != null)
            {
                GUILayout.BeginHorizontal();

                //File selection
                GUILayout.BeginVertical();
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

                EditorGUI.BeginChangeCheck();
                _selectedFile = GUILayout.SelectionGrid(_selectedFile, _currentArchive.stuff, 1);
                if (EditorGUI.EndChangeCheck())
                {
                    _hexScroll = Vector2.zero;
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                // File info
                GUILayout.BeginVertical();

                _hexScroll = HexDisplay.Display(_hexScroll, _currentArchive.data, null);

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }*/
            /*if (_currentArchive.AllFiles != null)
            {
                GUILayout.BeginHorizontal();

                //File selection
                GUILayout.BeginVertical(GUILayout.Width(150.0f));
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

                EditorGUI.BeginChangeCheck();
                _selectedFile = GUILayout.SelectionGrid(_selectedFile, _fileNames, 1);
                if (EditorGUI.EndChangeCheck())
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
                GUILayout.Label("Length " + file.Length + " (" + file.Lenght2 + ")");
                GUILayout.Label("Type UNKNOWN");
                if (GUILayout.Button("Try to recover as Scene"))
                {
                    Scene.AttemptRecovery(file);
                }

                if (GUILayout.Button("Try recover as textures"))
                {
                    RecoverTexture(file);
                }

                _hexScroll = HexDisplay.Display(_hexScroll, file.data, null);

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            
            }*/

        }
    }
}