using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace ShiningHill
{
    public class FileBrowserSources : ScriptableObject
    {
        const string SOURCES_PATH = "Assets/Settings/FileBrowserSources.asset";
        static FileBrowserSources _sourcesCache;

        public List<SourceEntry> sources = new List<SourceEntry>();

        public static FileBrowserSources GetSources()
        {
            if (_sourcesCache == null)
            {
                if (File.Exists(SOURCES_PATH))
                {
                    _sourcesCache = AssetDatabase.LoadAssetAtPath<FileBrowserSources>(SOURCES_PATH);
                }
                else
                {
                    string dirPath = Path.GetDirectoryName(SOURCES_PATH);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    _sourcesCache = ScriptableObject.CreateInstance<FileBrowserSources>();
                    AssetDatabase.CreateAsset(_sourcesCache, SOURCES_PATH);
                    AssetDatabase.Refresh();
                }
            }

            return _sourcesCache;
        }

        public static void AddSource(string path, string name, byte handlerID)
        {
            FileBrowserSources s = GetSources();
            s.sources.Add(new SourceEntry() { path = path, name = name, handlerID = handlerID });
            EditorUtility.SetDirty(s);
            AssetDatabase.SaveAssets();
        }

        [Serializable]
        public class SourceEntry : IListable
        {
            public string path;
            public string name;
            public byte handlerID;
            public List<WorkspaceEntry> workspaces = new List<WorkspaceEntry>();

            public bool isExpanded
            {
                get { return true; }
                set { }
            }

            public bool hasChildren
            {
                get
                {
                    return workspaces != null && workspaces.Count > 0;
                }
            }

            public SourceBase MakeSource()
            {
                SourceBase.SourceHandler sh = SourceBase.GetHandlerForID(handlerID);
                return sh.Instantiate(path);
            }

            public void Draw()
            {
                EditorGUILayout.LabelField(name);
            }

            public IEnumerator<IListable> GetEnumerator()
            {
                for (int i = 0; i != workspaces.Count; i++)
                    yield return workspaces[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [Serializable]
        public class WorkspaceEntry : IListable
        {
            public string path;

            public bool isExpanded
            {
                get { return false; }
                set { }
            }

            public bool hasChildren
            {
                get { return false; }
            }

            public void Draw()
            {
                EditorGUILayout.LabelField(path);
            }

            public IEnumerator<IListable> GetEnumerator()
            {
                return null;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
