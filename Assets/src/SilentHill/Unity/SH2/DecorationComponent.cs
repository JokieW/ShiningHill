using UnityEditor;
using UnityEngine;

using SH.Core;
using SH.GameData.SH2;

namespace SH.Unity.SH2
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DecorationComponent))]
    public class DecorationComponentEditor : Editor
    {
        void OnSceneGUI()
        {
            Matrix4x4 prevMatrix = Handles.matrix;
            DecorationComponent t = target as DecorationComponent;
            if (t.decoration != null)
            {
                Handles.matrix = t.transform.localToWorldMatrix;
                HandlesUtil.DrawBoundingCube(t.decoration.header.boundingBoxA, t.decoration.header.boundingBoxB);
            }
            Handles.matrix = prevMatrix;
        }
    }

    public class DecorationComponent : MonoBehaviour
    {
        public FileGeometry.Geometry.MapDecorations.Decoration decoration;
    }
}
