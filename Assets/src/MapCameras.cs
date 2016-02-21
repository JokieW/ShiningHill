using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
	public class MapCameras : MonoBehaviour 
	{
        public List<Camera> cameras = new List<Camera>();
        public static MapCameras ReadCameras(string path)
        {
            /*string assetPath = path.Replace(".cam", ".asset");*/
            GameObject subGO = Scene.BeginEditingPrefab(path, "Cameras");

            try
            {
                MapCameras cams = subGO.AddComponent<MapCameras>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

                Matrix4x4 transMat = cams.GetComponentInParent<Scene>().GetSH3ToUnityMatrix();
                while (true)
                {
                    Camera cam = Camera.TryMakeCamera(reader, transMat);
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

                Scene.FinishEditingPrefab(path, subGO);

                return cams;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        void OnDrawGizmosSelected()
        {
            
            foreach (Camera cam in cameras)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(cam.activeArea.center, cam.activeArea.size);

                Gizmos.color = Color.magenta;
                if (cam.constraintsArea.size == Vector3.zero)
                {
                    Gizmos.matrix = Matrix4x4.TRS(cam.constraintsArea.center, Quaternion.LookRotation(Vector3.Normalize(cam.activeArea.center - cam.constraintsArea.center)), Vector3.one);
                    Gizmos.DrawFrustum(cam.constraintsArea.center, 60.0f, 1.0f, 0.1f, 1.33f);
                    Gizmos.matrix = Matrix4x4.identity;
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

            public static Camera TryMakeCamera(BinaryReader reader, Matrix4x4 transMat)
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

                    Vector3 activeMin = new Vector3(zoneA.x, -zoneHeights.x, zoneA.y);
                    Vector3 activeMax = new Vector3(zoneB.x, -zoneHeights.y, zoneB.y);
                    cam.activeArea = new Bounds();
                    cam.activeArea.SetMinMax(transMat.MultiplyPoint(activeMin), transMat.MultiplyPoint(activeMax));

                    Vector3 constraintMin = new Vector3(constraintA.x, -constraintHeights.x, constraintA.y);
                    Vector3 constraintMax = new Vector3(constraintB.x, -constraintHeights.y, constraintB.y);
                    cam.constraintsArea = new Bounds();
                    cam.constraintsArea.SetMinMax(transMat.MultiplyPoint(constraintMin), transMat.MultiplyPoint(constraintMax));

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