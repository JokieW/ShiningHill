using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOutlines : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Color prevColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;

        float idist = -320000.0f;
        for (int i = 0; i < 0x10; i++)
        {
            if (i == 0x10 / 2)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = new Color(0.75f, 0.90f, 0.90f, 0.40f);
            }
            Gizmos.DrawLine(new Vector3(idist, 0.0f, -320000.0f), new Vector3(idist, 0.0f, 320000.0f));
            idist += 20000.0f;
            //Gizmos.color = new Color(0.90f, 0.75f, 0.90f, 0.25f);
            //Gizmos.DrawLine(new Vector3(idist, 0.0f, -320000.0f), new Vector3(idist, 0.0f, 320000.0f));
            idist += 20000.0f;
        }
        idist = -320000.0f;
        for (int i = 0; i < 0x10; i++)
        {
            if (i == 0x10 / 2)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = new Color(0.75f, 0.90f, 0.90f, 0.40f);
            }
            Gizmos.DrawLine(new Vector3(-320000.0f, 0.0f, idist), new Vector3(320000.0f, 0.0f, idist));
            idist += 20000.0f;
            //Gizmos.color = new Color(0.90f, 0.75f, 0.90f, 0.25f);
            //Gizmos.DrawLine(new Vector3(-320000.0f, 0.0f, idist), new Vector3(320000.0f, 0.0f, idist));
            idist += 20000.0f;
        }

        Gizmos.matrix = prevMatrix;
        Gizmos.color = prevColor;
    }
}
