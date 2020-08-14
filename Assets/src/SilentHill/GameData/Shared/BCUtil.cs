using UnityEngine;

namespace SH.GameData.Shared
{
    public static class BCUtil
    {
        public unsafe struct ColorBlock
        {
            public Color* a;
            public Color* e;
            public Color* i;
            public Color* m;
        }
    }
}
