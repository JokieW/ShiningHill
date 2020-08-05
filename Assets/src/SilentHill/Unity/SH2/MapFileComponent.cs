using UnityEditor;
using UnityEngine;

using SH.Core;
using SH.GameData.SH2;

namespace SH.Unity.SH2
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MapFileComponent))]
    public class MapFileComponentEditor : Editor
    {
        void OnSceneGUI()
        {
            Matrix4x4 prevMatrix = Handles.matrix;
            MapFileComponent t = target as MapFileComponent;
            if (t.mapFile != null)
            {
                Handles.matrix = t.transform.localToWorldMatrix;
                HandlesUtil.DrawBoundingCube(t.mapFile.GetMainGeometryFile().geometry.mapMesh.header.boundingBoxA, t.mapFile.GetMainGeometryFile().geometry.mapMesh.header.boundingBoxB);
            }
            Handles.matrix = prevMatrix;
        }
    }

    public class MapFileComponent : MonoBehaviour
    {
       public FileMap mapFile;
    }
}
