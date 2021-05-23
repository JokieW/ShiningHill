using UnityEngine;

using SH.GameData.SH3;

namespace SH.Unity.SH3
{
    public class ExeGlobalComponent : MonoBehaviour
    {
        [ContextMenu("Fill from EXE")]
        void FillFromExe()
        {
            ExeData.RegionData[] regionPointers = ExeExtractor.ExtractRegionEventData();
            for(int i = 0; i != regionPointers.Length; i++)
            {
                ExeData.RegionData data = regionPointers[i];
                GameObject go = new GameObject("Region " + i + ": " + data.name);
                go.AddComponent<ExeRegionComponent>().ResetRegion(data);
                go.transform.parent = transform;
                go.transform.localScale = Vector3.one;
            }
            ExeExtractor.UpdateAssetsFromRegions(regionPointers);
        }
    }
}
