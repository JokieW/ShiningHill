using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkipJumpDrawer : MonoBehaviour
{
    [Serializable]
    public struct Pair
    {
        public string name;
        public Vector3 position;
    }

    public Pair[] pairs;
    public Color color = Color.white;

    private void OnDrawGizmos()
    {
        Matrix4x4 gmat = Gizmos.matrix;
        Matrix4x4 hmat = Handles.matrix;
        Color oldColor = Gizmos.color;
        Color oldhColor = Handles.color;

        Gizmos.matrix = transform.parent.localToWorldMatrix;
        Handles.matrix = transform.parent.localToWorldMatrix;
        Gizmos.color = color;
        Handles.color = color;

        for (int i = 0; i < pairs.Length; i++)
        {
            Gizmos.DrawLine(transform.parent.worldToLocalMatrix.MultiplyPoint(transform.position), pairs[i].position);
            Handles.Label(pairs[i].position, pairs[i].name);
        }

        Gizmos.matrix = gmat;
        Handles.matrix = hmat;
        Gizmos.color = oldColor;
        Handles.color = oldhColor;
    }
}
