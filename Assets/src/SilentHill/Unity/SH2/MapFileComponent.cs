using UnityEditor;
using UnityEngine;

using SH.Core;
using SH.GameData.SH2;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

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
                FileGeometry fg = t.mapFile.GetMainGeometryFile();
                if (fg != null)
                {
                    HandlesUtil.DrawBoundingCube(fg.geometry.mapMesh.header.boundingBoxA, fg.geometry.mapMesh.header.boundingBoxB);
                }
            }
            Handles.matrix = prevMatrix;
        }
    }

    public class MapFileComponent : MonoBehaviour
    {
        public FileMap mapFile;
        public List<FileTex> textures;
        public List<FileGeometry> geometries;

        public void SetMapFile(FileMap mapFile)
        {
            this.mapFile = mapFile;
            textures = new List<FileTex>();
            mapFile.GetTextureFiles(textures);
            geometries = new List<FileGeometry>();
            mapFile.GetGeometryFiles(geometries);
        }
    }
}
