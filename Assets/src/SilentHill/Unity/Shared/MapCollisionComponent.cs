using UnityEngine;
using UnityEditor;

using SH.GameData.Shared;
using UnityEngine.UI;
using System;

namespace SH.Unity.Shared
{
    [CustomEditor(typeof(MapCollisionComponent))]
    public class MapCollisionComponentEditor : Editor
    {
        private static Vector3[] rectBuffer = new Vector3[4];

        private bool showGroup0 = true;
        private bool showGroup1 = true;
        private bool showGroup2 = true;
        private bool showGroup3 = true;
        private bool showGroup4 = true;
        private bool showLabels = true;

        private static Vector3 GetFaceCenter(in CollisionFace face)
        {
            if(face.isTriangle)
            {
                return new Vector3(
                    (face.vertex0.x + face.vertex1.x + face.vertex2.x) / 3,
                    (face.vertex0.y + face.vertex1.y + face.vertex2.y) / 3,
                    (face.vertex0.z + face.vertex1.z + face.vertex2.z) / 3
                );
            }
            else if(face.isQuad)
            {
                return face.vertex0 + ((face.vertex2 - face.vertex0) / 2);
            }
            throw new InvalidOperationException();
        }

        void OnSceneGUI()
        {
            MapCollisionComponent t = target as MapCollisionComponent;
            if (t.collisions != null)
            {
                Matrix4x4 prevMatrix = Handles.matrix;
                Handles.matrix = t.transform.localToWorldMatrix;
                for (int arrayIndex = 0; arrayIndex < 4; arrayIndex++)
                {
                    if (ShowGroupIndex(arrayIndex))
                    {
                        CollisionFace[] faces = t.collisions.IndexToFaceArray(arrayIndex);
                        if (faces != null)
                        {
                            for (int i = 0; i < faces.Length; i++)
                            {
                                ref readonly CollisionFace face = ref faces[i];
                                rectBuffer[0] = face.vertex0;
                                rectBuffer[1] = face.vertex1;
                                rectBuffer[2] = face.vertex2;
                                rectBuffer[3] = face.isQuad ? face.vertex3 : face.vertex2;
                                GroupIndexToColors(arrayIndex, out Color faceColor, out Color outlineColor);
                                Handles.DrawSolidRectangleWithOutline(rectBuffer, faceColor, outlineColor);
                            }
                        }
                    }
                }

                if (showLabels)
                {
                    for (int arrayIndex = 0; arrayIndex < 4; arrayIndex++)
                    {
                        if (ShowGroupIndex(arrayIndex))
                        {
                            CollisionFace[] faces = t.collisions.IndexToFaceArray(arrayIndex);
                            if (faces != null)
                            {
                                for (int i = 0; i < faces.Length; i++)
                                {
                                    ref readonly CollisionFace face = ref faces[i];
                                    GUI.contentColor = Color.black;
                                    Handles.Label(GetFaceCenter(face), face.GetLabel());
                                }
                            }
                        }
                    }
                }

                if (ShowGroupIndex(4))
                {
                    CollisionCylinder[] cylinders = t.collisions.group4Cylinders;
                    if (cylinders != null)
                    {
                        for (int i = 0; i < cylinders.Length; i++)
                        {
                            ref readonly CollisionCylinder cylinder = ref cylinders[i];
                            GroupIndexToColors(4, out Color faceColor, out Color outlineColor);
                            DrawWireCapsule(cylinder.position, Quaternion.identity, cylinder.radius, cylinder.height.y, outlineColor);
                            if (showLabels)
                            {
                                Handles.Label(cylinder.position, cylinder.GetLabel());
                            }
                        }
                    }
                }
                Handles.matrix = prevMatrix;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            showGroup0 = EditorGUILayout.Toggle("Show group 0", showGroup0);
            showGroup1 = EditorGUILayout.Toggle("Show group 1", showGroup1);
            showGroup2 = EditorGUILayout.Toggle("Show group 2", showGroup2);
            showGroup3 = EditorGUILayout.Toggle("Show group 3", showGroup3);
            showGroup4 = EditorGUILayout.Toggle("Show group 4", showGroup4);
            showLabels = EditorGUILayout.Toggle("Show labels", showLabels);

        }

        //https://answers.unity.com/questions/56063/draw-capsule-gizmo.html
        public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color))
        {
            if (_color != default(Color))
                Handles.color = _color;
            Matrix4x4 angleMatrix = Handles.matrix * Matrix4x4.TRS(_pos, _rot, Vector3.one);
            Vector3 up = angleMatrix.MultiplyVector(Vector3.up);
            Vector3 down = angleMatrix.MultiplyVector(Vector3.down);
            Vector3 left = angleMatrix.MultiplyVector(Vector3.left);
            Vector3 back = angleMatrix.MultiplyVector(Vector3.back);
            using (new Handles.DrawingScope(angleMatrix))
            {
                var pointOffset = (_height - (_radius * 2)) / 2;

                //draw sideways
                Handles.DrawWireArc(up * pointOffset, left, back, -180, _radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
                Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
                Handles.DrawWireArc(down * pointOffset, left, back, 180, _radius);
                //draw frontways
                Handles.DrawWireArc(up * pointOffset, back, left, 180, _radius);
                Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
                Handles.DrawWireArc(down * pointOffset, back, left, -180, _radius);
                //draw center
                Handles.DrawWireDisc(up * pointOffset, up, _radius);
                Handles.DrawWireDisc(down * pointOffset, up, _radius);

            }
        }

        private bool ShowGroupIndex(int index)
        {
            if (index == 0) return showGroup0;
            if (index == 1) return showGroup1;
            if (index == 2) return showGroup2;
            if (index == 3) return showGroup3;
            if (index == 4) return showGroup4;
            return true;
        }

        private void GroupIndexToColors(int index, out Color faceColor, out Color outlineColor)
        {
            if(index == 0)
            {
                faceColor = new Color(0.00000f, 0.73333f, 0.62745f, 0.50000f);
                outlineColor = new Color(0.00000f, 0.35686f, 0.30588f, 1.00000f);
                return;
            }
            if (index == 1)
            {
                faceColor = new Color(1.00000f, 0.35294f, 0.29411f, 0.50000f);
                outlineColor = new Color(0.36862f, 0.02745f, 0.00000f, 1.00000f);
                return;
            }
            if (index == 2)
            {
                faceColor = new Color(0.98431f, 0.73333f, 0.00000f, 0.50000f);
                outlineColor = new Color(0.35682f, 0.27058f, 0.00000f, 1.00000f);
                return;
            }
            if (index == 3)
            {
                faceColor = new Color(0.57647f, 0.69803f, 0.75686f, 0.50000f);
                outlineColor = new Color(0.12941f, 0.19607f, 0.22745f, 1.00000f);
                return;
            }
            if (index == 4)
            {
                faceColor = new Color(0.46274f, 0.21568f, 0.60392f, 0.50000f);
                outlineColor = new Color(0.16078f, 0.07450f, 0.21568f, 1.00000f);
                return;
            }

            faceColor = Color.white;
            outlineColor = Color.black;
        }
    }

    public class MapCollisionComponent : MonoBehaviour
    {
        public FileCollisions collisions;
    }
}
