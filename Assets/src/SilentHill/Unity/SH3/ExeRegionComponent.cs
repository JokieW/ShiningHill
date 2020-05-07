using UnityEngine;
using UnityEditor;

using SH.GameData.SH3;

namespace SH.Unity.SH3
{
    public class ExeRegionComponent : MonoBehaviour
    {
        [SerializeField]
        public ExeData.RegionData regionData;
        [SerializeField]
        [HideInInspector]
        public Mesh markerMesh;
        [SerializeField]
        [HideInInspector]
        public Vector3[] markerLines;

        public void ResetRegion(ExeData.RegionData newData)
        {
            regionData = newData;
        }

        private void OnDrawGizmos()
        {
            if (markerLines != null)
            {
                Matrix4x4 oldmat = Handles.matrix;
                Handles.matrix = transform.localToWorldMatrix;
                Handles.color = Color.yellow;
                Handles.DrawLines(markerLines);
                Handles.matrix = oldmat;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (markerLines != null)
            {
                Matrix4x4 oldmat = Handles.matrix;
                Handles.matrix = transform.localToWorldMatrix;
                if (regionData != null && regionData.events != null)
                {
                    for (int i = 0; i != regionData.events.Count; i++)
                    {
                        ExeData.EventInfo ev = regionData.events[i];
                        short off = ev.GetLocationOffset();
                        for (int j = 0; j != regionData.markers.Count; j++)
                        {
                            ExeData.EventMarker marker = regionData.markers[j];
                            if (off == marker.offset && marker.type == 2)
                            {
                                Handles.Label(marker.GetCenter() * 0.002f, ev.eventNumber.ToString("X2"));
                            }
                        }

                    }
                }
                Handles.matrix = oldmat;
            }
        }
    }
}
