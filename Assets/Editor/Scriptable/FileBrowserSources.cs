using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;

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

        public void Draw()
        {
            EditorGUILayout.LabelField(name);
        }

        private IEnumerable<IListable> Enumerate()
        {
            for(int i = 0; i != workspaces.Count; i++)
                yield return workspaces[i];
        }

        public IEnumerator<IListable> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
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

        public void Draw()
        {
            EditorGUILayout.LabelField(path);
        }

        private IEnumerable<IListable> Enumerate()
        {
            return null;
        }

        public IEnumerator<IListable> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
