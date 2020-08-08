using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestLineDrawer : MonoBehaviour
{
    public GameObject otherPoint;
    public Color color = Color.white;

    private void OnDrawGizmos()
    {
        if(otherPoint != null)
        {
            Color oldColor = Gizmos.color;
            Color oldhColor = Handles.color;

            Gizmos.color = color;
            Handles.color = color;
            Gizmos.DrawLine(transform.position, otherPoint.transform.position);
            float distance = Vector3.Distance(otherPoint.transform.position, transform.position);
            Handles.Label(transform.position, distance.ToString());

            Gizmos.color = oldColor;
            Handles.color = oldhColor;
        }
    }
}
