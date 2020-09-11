using UnityEditor;
using UnityEngine;

using SH.Core;
using SH.GameData.SH2;

namespace SH.Unity.SH2
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DecalComponent))]
    public class DecalComponentEditor : Editor
    {
        void OnSceneGUI()
        {
            Matrix4x4 prevMatrix = Handles.matrix;
            DecalComponent t = target as DecalComponent;
            if (t.decal != null)
            {
                Handles.matrix = t.transform.localToWorldMatrix;
                HandlesUtil.DrawBoundingCube(t.decal.header.boundingBoxA, t.decal.header.boundingBoxB);
            }
            Handles.matrix = prevMatrix;
        }
    }

    public class DecalComponent : MonoBehaviour
    {
        public FileGeometry.Geometry.MapDecals.Decal decal;
    }
}
