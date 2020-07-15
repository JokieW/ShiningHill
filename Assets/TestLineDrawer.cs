using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLineDrawer : MonoBehaviour
{
    public GameObject otherPoint;

    private void OnDrawGizmos()
    {
        if(otherPoint != null)
        {
            Gizmos.DrawLine(transform.position, otherPoint.transform.position);
        }
    }
}
