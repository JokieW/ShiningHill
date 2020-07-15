using UnityEngine;

using SH.GameData.SH3;

namespace SH.Unity.SH3
{
	public class ObjectTransformComponent : MonoBehaviour 
	{
        public FileMap.ObjectTransform header;
        public Vector4[] boundingBox;
    }
}
