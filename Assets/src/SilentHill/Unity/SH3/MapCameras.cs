using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;
using SH.Core;

namespace SH.Unity.SH3
{
    public class MapCameras : MonoBehaviour 
	{
        [MenuItem("Assets/holla/amigo")]
        public static void ImportMap()
        {

        }

        public List<Camera> cameras = new List<Camera>();
        public static MapCameras ReadCameras(/*MapCamerasAssetPaths paths*/)
        {
            GameObject subGO = null;//Scene.BeginEditingPrefab(paths.GetPrefabPath(), "Cameras");

            try
            {
                MapCameras cams = subGO.AddComponent<MapCameras>();

                BinaryReader reader = null;//new BinaryReader(new FileStream(paths.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));
                
                while (true)
                {
                    Camera cam = Camera.TryMakeCamera(reader);
                    if (cam == null)
                    {
                        break;
                    }
                    else
                    {
                        cams.cameras.Add(cam);
                    }
                }

                reader.Close();

                //Scene.FinishEditingPrefab(paths.GetPrefabPath(), subGO);

                return cams;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        Matrix4x4 adjustedMat = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.002f, -0.002f, 0.002f));
        void OnDrawGizmosSelected()
        {
            
            foreach (Camera cam in cameras)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix = adjustedMat;
                Gizmos.DrawWireCube(cam.activeArea.center, cam.activeArea.size);

                Gizmos.color = Color.magenta;
                if (cam.constraintsArea.size == Vector3.zero)
                {
                    Gizmos.matrix = Matrix4x4.TRS(cam.constraintsArea.center, Quaternion.LookRotation(Vector3.Normalize(cam.activeArea.center - cam.constraintsArea.center)), new Vector3(0.002f, -0.002f, 0.002f));
                    Gizmos.DrawFrustum(cam.constraintsArea.center, 60.0f, 1.0f, 0.1f, 1.33f);
                    Gizmos.matrix = adjustedMat;
                }
                else
                {
                    Gizmos.DrawWireCube(cam.constraintsArea.center, cam.constraintsArea.size);
                }
            }
        }

        [Serializable]
        public class Camera
        {
            public Bounds activeArea;
            public Bounds constraintsArea;
            public int type;
            public int Unknown1;
            public Vector4 Unknown2;
            public Vector4 Unknown3;
            public int Unknown4;
            public Vector2 Unknown5;
            public Vector2 Unknown6;

            public static Camera TryMakeCamera(BinaryReader reader)
            {
                Vector2 zoneA = reader.ReadVector2();
                reader.SkipBytes(8);
                Vector2 zoneB = reader.ReadVector2();
                Vector2 zoneHeights = reader.ReadVector2();

                Vector2 constraintA = reader.ReadVector2();
                reader.SkipBytes(8);
                Vector2 constraintB = reader.ReadVector2();
                Vector2 constraintHeights = reader.ReadVector2();

                reader.SkipInt32(0);
                int type = reader.ReadInt32();
                reader.SkipInt32(0);
                int Unknown1 = reader.ReadInt32();

                Vector4 Unknown2 = reader.ReadVector4();
                Vector2 Unknown3 = reader.ReadVector2();
                reader.SkipInt32(0);
                int Unknown4 = reader.ReadInt32();
                Vector2 Unknown5 = reader.ReadVector2();
                Vector2 Unknown6 = reader.ReadVector2();

                if (type != 1)
                {
                    Camera cam = new Camera();

                    Vector3 activeMin = new Vector3(zoneA.x, zoneHeights.x, zoneA.y);
                    Vector3 activeMax = new Vector3(zoneB.x, zoneHeights.y, zoneB.y);
                    cam.activeArea = new Bounds();
                    cam.activeArea.SetMinMax(activeMin, activeMax);

                    Vector3 constraintMin = new Vector3(constraintA.x, constraintHeights.x, constraintA.y);
                    Vector3 constraintMax = new Vector3(constraintB.x, constraintHeights.y, constraintB.y);
                    cam.constraintsArea = new Bounds();
                    cam.constraintsArea.SetMinMax(constraintMin, constraintMax);

                    cam.type = type;
                    cam.Unknown1 = Unknown1;
                    cam.Unknown2 = Unknown2;
                    cam.Unknown3 = Unknown3;
                    cam.Unknown4 = Unknown4;
                    cam.Unknown5 = Unknown5;
                    cam.Unknown6 = Unknown6;

                    return cam;
                }
                return null;
            }
        }
	}
}