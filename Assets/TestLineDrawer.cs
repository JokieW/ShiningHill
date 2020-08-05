using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestLineDrawer : MonoBehaviour
{
    public GameObject otherPoint;

    private void OnDrawGizmos()
    {
        if(otherPoint != null)
        {
            Gizmos.DrawLine(transform.position, otherPoint.transform.position);
            float distance = Vector3.Distance(otherPoint.transform.position, transform.position);
            Handles.Label(transform.position, distance.ToString());
            Handles.Label(transform.position, distance.ToString());
        }
    }
}
