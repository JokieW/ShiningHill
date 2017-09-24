using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SH3RunCamera : MonoBehaviour
{
    private SHPtr v3_camPos = 0x0711A660;
    private SHPtr v3_camTarget = 0x0711A784;

    void Update()
    {
        transform.position = Scribe.ReadVector3(StateChecker.instance.memHandle, v3_camPos);
        transform.LookAt(Scribe.ReadVector3(StateChecker.instance.memHandle, new System.IntPtr(0x0711A69C)));
        //transform.rotation = Scribe.ReadQuaternion(StateChecker.instance.memHandle, 
    }

    void OnDrawGizmos()
    {
        if (StateChecker.instance != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 v3 = Scribe.ReadVector3(StateChecker.instance.memHandle, new System.IntPtr(0x0711A784));
            Gizmos.matrix = StateChecker.instance.transform.worldToLocalMatrix;
            Gizmos.DrawSphere(v3, 1);
        }
    }
}
