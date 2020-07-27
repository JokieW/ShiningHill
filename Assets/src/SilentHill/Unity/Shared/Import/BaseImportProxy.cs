using UnityEditor;
using UnityEngine;

namespace SH.Unity.Shared
{
    [CanEditMultipleObjects]
    public class BaseImportProxyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BaseImportProxy proxy = (BaseImportProxy)target;
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

    public abstract class BaseImportProxy : ScriptableObject
    {
        public bool unpackRecursive = true;

        public abstract void Unpack();
        public abstract void Pack();
    }

    public abstract class PackingOptions
    {

    }

    public class NoPackingOptions : PackingOptions
    {

    }
}
