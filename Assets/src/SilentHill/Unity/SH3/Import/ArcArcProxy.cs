using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using SH.GameData.SH3;
using SH.Unity.Shared;

namespace SH.Unity.SH3
{
    [CustomEditor(typeof(ArcArcProxy))]
    public class ArcArcProxyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ArcArcProxy proxy = (ArcArcProxy)target;
            DrawDefaultInspector();

            //Unpack
            if (GUILayout.Button("Unpack"))
            {
                try
                {
                    AssetUtil.StartAssetEditing();
                    proxy.Unpack();
                }
                finally
                {
                    AssetUtil.StopAssetEditing();
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
    public class ArcArcProxy : BaseImportProxy
    {
        public UnityEngine.Object arcArc;
        public List<ArcProxy> arcs;

        public void GetRoot(out FileArcArc.Root root)
        {
            FileArcArc.UnpackArcArc(UnpackPath.GetPath(arcArc), out root);
        }

        public override void Unpack()
        {
            GetRoot(out FileArcArc.Root root);

            arcs = new List<ArcProxy>();
            UnpackPath basepath = UnpackPath.GetDirectory(arcArc);
            try
            {
                for (int i = 0; i < root.folders.Length; i++)
                {
                    ref readonly FileArcArc.Root.Folder folder = ref root.folders[i];
                    if (EditorUtility.DisplayCancelableProgressBar("Unpacking Arc.Arc...", folder.entry.name, (float)i / (float)root.folders.Length)) return;

                    UnpackPath arcpath = basepath.WithName(folder.entry.name + ".arc");
                    ArcProxy arc = CreateInstance<ArcProxy>();
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
                    ArcProxy arc = arcs[i];
                    root.GetFolder(arc.arcName, out FileArcArc.Root.Folder folder);
                    arcs[i].Unpack(folder, false);
                }

                AssetDatabase.Refresh();
                for (int i = 0; i < arcs.Count; i++)
                {
                    ArcProxy arc = arcs[i];
                    root.GetFolder(arc.arcName, out FileArcArc.Root.Folder folder);
                    arcs[i].ImportAssets(folder);
                }
            }
            EditorUtility.SetDirty(this);
        }

        public override void Pack()
        {
            (string, string[])[] map = new (string, string[])[arcs.Count];
            for (int i = 0; i < arcs.Count; i++)
            {
                map[i] = arcs[i].GetMap();
            }

            FileArcArc.MakeArcArcInfo(UnpackPath.GetDirectory(arcArc).WithPath(""), map, out FileArcArc.Root root);
            for (int i = 0; i < arcs.Count; i++)
            {
                ArcProxy arc = arcs[i];
                root.GetFolder(arc.arcName, out FileArcArc.Root.Folder folder);
                arc.Pack(in folder);
            }
            FileArcArc.PackArcArc(UnpackPath.GetPath(arcArc), in root);
        }
    }
}
