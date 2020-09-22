using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace SH.GameData.SH2
{
    public class FileLighting
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            [Hex] public short areaCode;
            [Hex] public short field_02;
            [Hex] public int globalLightsCount; //count
            [Hex] public int globalLightsOffset; //offset
            [Hex] public int field_0C; //

            [Hex] public int weirdLightsCount; //
            [Hex] public int weirdLightsOffset; //
            [Hex] public int field_18; //
            [Hex] public int field_1C; //

            [Hex] public int field_20; //
            [Hex] public int lightsCount; //count
            [Hex] public int lightsOffset; //offset
            [Hex] public int field_2C; //

            [Hex] public int field_30; //
            [Hex] public int field_34; //
            [Hex] public int field_38; //
            [Hex] public int field_3C; //

            [Hex] public int field_40; //
            [Hex] public int field_44; //
            [Hex] public int field_48; //
            [Hex] public int field_4C; //

            [Hex] public int field_50; //
            [Hex] public int ambientOffset; //offset
            [Hex] public int field_58; //
            [Hex] public int field_5C; //

            [Hex] public int field_60; //
            [Hex] public int field_64; //
            [Hex] public int field_68; //
            [Hex] public int field_6C; //
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct GlobalLight
        {
            public Quaternion rotation;
            
            public Vector3 field_10;
            public short field_1C;
            public short field_1E;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct LocalLight
        {
            public Color color; //1.0 to 22.40
            
            public float field_10;
            public float range;
            [Hex] public int field_18;
            [Hex] public int field_1C;

            public Vector3 position;
            [Hex] public int Unknown2;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct AmbientLight
        {
            public float field_00;
            public float field_04;
            public float field_08;
            public float field_0C;

            public float field_10;
            public float field_14;
            public float field_18;
            public float field_1C;

            public Color color;

            public float field_30;
            public float field_34;
            public float field_38;
            public float field_3C;

            public float field_40;
            public float field_44;
            public float field_48;
            public float field_4C;

            public float field_50;
            public float field_54;
            public float field_58;
            public float field_5C;

            public float field_60;
            public float field_64;
            public float field_68;
            public float field_6C;

            public float field_70;
            public float field_74;
            public float field_78;
            public float field_7C;
        }


    }
}
