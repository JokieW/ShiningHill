using UnityEngine;
using System.Collections;

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
                go.transform.localPosition = marker.GetFinalPosition();
            }
        }
    }
}