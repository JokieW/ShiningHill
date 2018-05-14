using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ShiningHill
{
    public class Region : MonoBehaviour
    {
        [SerializeField]
        public SH3_ExeData.RegionData regionData;

        public void ResetRegion(SH3_ExeData.RegionData newData)
        {
            regionData = newData;
            if(regionData.markers != null)
            for(int i = 0; i != regionData.markers.Count; i++)
            {
                SH3_ExeData.EventMarker marker = regionData.markers[i];
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = marker.offset + " " + marker.type;
                go.transform.parent = transform;
                go.transform.localScale = new Vector3(250, 250, 250);
                go.transform.localPosition = marker.GetCenterPosition();
            }
        }

        public void OnDrawGizmosSelected()
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            for (int i = 0; i != regionData.markers.Count; i++)
            {
                SH3_ExeData.EventMarker marker = regionData.markers[i];
                Vector3 a, b, c, d;
                marker.GetBounds(out a, out b);
                c = new Vector3(b.x, a.y, b.z);
                d = new Vector3(a.x, b.y, a.z);
                Handles.matrix = transform.localToWorldMatrix;
                Handles.DrawSolidRectangleWithOutline(new Vector3[] { a, c, b, d }, new Color(1.0f, 0.92f, 0.016f, 0.10f), new Color(1.0f, 0.92f, 0.016f, 0.50f));
            }
            Gizmos.matrix = old;
        }
    }
}