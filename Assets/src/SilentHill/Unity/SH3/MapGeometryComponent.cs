using UnityEngine;

using SH.GameData.SH3;

namespace SH.Unity.SH3
{
    public class MapGeometryComponent : MonoBehaviour
    {
        public FileMap geometry;
        public FileMap.Header header;
        public Matrix4x4[] eventMatrices;
    }
}
