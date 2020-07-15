using UnityEngine;

using SH.GameData.SH3;

namespace SH.Unity.SH3
{
	public class MeshPartComponent : MonoBehaviour 
	{
        public FileMap.MeshPart.Header header;
        public FileMap.MeshPart.ExtraData[] extraData;
    }
}
