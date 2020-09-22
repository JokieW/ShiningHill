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
                SubFileGeometry fg = t.mapFile.GetMainGeometryFile();
                if (fg != null)
                {
                    //HandlesUtil.DrawBoundingCube(fg.geometries.mapMesh.header.boundingBoxA, fg.geometries.mapMesh.header.boundingBoxB);
                }
            }
            Handles.matrix = prevMatrix;
        }
    }

    public class MapFileComponent : MonoBehaviour
    {
        public FileMap mapFile;
        public List<SubFileTex> textures;
        public List<SubFileGeometry> geometries;

        public void SetMapFile(FileMap mapFile)
        {
            this.mapFile = mapFile;
            textures = new List<SubFileTex>();
            mapFile.GetTextureFiles(textures);
            geometries = new List<SubFileGeometry>();
            mapFile.GetGeometryFiles(geometries);
        }
    }
}
