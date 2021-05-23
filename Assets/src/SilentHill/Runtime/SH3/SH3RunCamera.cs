using UnityEngine;

using SH.Runtime.Shared;

namespace SH.Runtime.SH3
{
    public class SH3RunCamera : MonoBehaviour
    {
        private SHPtr v3_camPos = 0x0711A660;
        private SHPtr v3_camTarget = 0x0711A650;

        void Update()
        {
            transform.localPosition = Scribe.ReadVector3(StateChecker.instance.memHandle, v3_camPos);
            v3_camTarget = 0x0711A69c;
            transform.LookAt(StateChecker.instance.transform.localToWorldMatrix.MultiplyPoint(Scribe.ReadVector3(StateChecker.instance.memHandle, v3_camTarget)));
            //transform.rotation = Scribe.ReadQuaternion(StateChecker.instance.memHandle, 
        }

        void OnDrawGizmos()
        {
            if (StateChecker.instance != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 v3 = Scribe.ReadVector3(StateChecker.instance.memHandle, v3_camTarget);
                Debug.Log(v3);
                Gizmos.matrix = StateChecker.instance.transform.localToWorldMatrix;
                Gizmos.DrawSphere(v3, 1f / 0.002f);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}
