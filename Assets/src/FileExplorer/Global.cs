using UnityEngine;
using System.Collections;

namespace ShiningHill
{
    public class Global : MonoBehaviour
    {
        [ContextMenu("Fill from EXE")]
        void FillFromExe()
        {
            SH3_ExeData.RegionData[] regionPointers = SH3exeExtractor.ExtractRegionEventData();
            for(int i = 0; i != regionPointers.Length; i++)
            {
                SH3_ExeData.RegionData data = regionPointers[i];
                GameObject go = new GameObject("Region " + i + ": " + data.name);
                go.AddComponent<SH3_Region>().ResetRegion(data);
                go.transform.parent = transform;
                go.transform.localScale = Vector3.one;
            }
            SH3exeExtractor.UpdateAssetsFromRegions(regionPointers);
        }
    }
}