using ShiningHill;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SH3ArcArcImportProxy))]
public class SH3ArcArcImportProxyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SH3ArcArcImportProxy proxy = (SH3ArcArcImportProxy)target;
        DrawDefaultInspector();

        //Unpack
        if (GUILayout.Button("Unpack"))
        {
            try
            {
                ExplorerUtil.StartAssetEditing();
                proxy.Unpack();
            }
            finally
            {
                ExplorerUtil.StopAssetEditing();
            }
        }

        //Pack
        if (GUILayout.Button("Pack"))
        {
            proxy.Pack();
        }
    }
}

[CreateAssetMenu(fileName = "SH3ArcArcImportProxy", menuName = "Import Proxies/SH3 ArcArc")]
public class SH3ArcArcImportProxy : BaseImportProxy
{
    public UnityEngine.Object arcArc;
    public List<SH3ArcImportProxy> arcs;
    public bool unpackRecursive = true;

    public void GetRoot(out ArcFileSystem.Root root)
    {
        ArcFileSystem.UnpackArcArc(UnpackPath.GetPath(arcArc), out root);
    }

    public void Unpack()
    {
        GetRoot(out ArcFileSystem.Root root);

        arcs = new List<SH3ArcImportProxy>();
        UnpackPath basepath = UnpackPath.GetDirectory(arcArc);
        try
        {
            for (int i = 0; i < root.folders.Length; i++)
            {
                ref readonly ArcFileSystem.Root.Folder folder = ref root.folders[i];
                if (EditorUtility.DisplayCancelableProgressBar("Unpacking Arc.Arc...", folder.entry.name, (float)i / (float)root.folders.Length)) return;

                UnpackPath arcpath = basepath.WithName(folder.entry.name + ".arc");
                SH3ArcImportProxy arc = CreateInstance<SH3ArcImportProxy>();
                arc.arc = AssetDatabase.LoadMainAssetAtPath(arcpath);
                arc.arcarc = this;
                arc.arcName = folder.entry.name;
                AssetDatabase.CreateAsset(arc, arcpath.WithDirectoryAndName(UnpackDirectory.Proxy, arcpath.name + ".asset", true));
                arcs.Add(arc);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        if (unpackRecursive)
        {
            for (int i = 0; i < arcs.Count; i++)
            {
                SH3ArcImportProxy arc = arcs[i];
                root.GetFolder(arc.arcName, out ArcFileSystem.Root.Folder folder);
                arcs[i].Unpack(folder, false);
            }

            AssetDatabase.Refresh();
            for (int i = 0; i < arcs.Count; i++)
            {
                SH3ArcImportProxy arc = arcs[i];
                root.GetFolder(arc.arcName, out ArcFileSystem.Root.Folder folder);
                arcs[i].ImportAssets(folder);
            }
        }
        EditorUtility.SetDirty(this);
    }

    public void Pack()
    {
        (string, string[])[] map = new (string, string[])[arcs.Count];
        for (int i = 0; i < arcs.Count; i++)
        {
            map[i] = arcs[i].GetMap();
        }

        ArcFileSystem.MakeArcArcInfo(UnpackPath.GetDirectory(arcArc).WithPath(""), map, out ArcFileSystem.Root root);
        for (int i = 0; i < arcs.Count; i++)
        {
            SH3ArcImportProxy arc = arcs[i];
            root.GetFolder(arc.arcName, out ArcFileSystem.Root.Folder folder);
            arc.Pack(in folder);
        }
        ArcFileSystem.PackArcArc(UnpackPath.GetPath(arcArc), in root);
    }
}
