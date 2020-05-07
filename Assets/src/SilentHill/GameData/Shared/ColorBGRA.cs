using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace SH.GameData.Shared
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct ColorBGRA
    {
        public byte blue;
        public byte green;
        public byte red;
        public byte alpha;

        public ColorBGRA(byte b, byte g, byte r, byte a)
        {
            blue = b;
            green = g;
            red = r;
            alpha = a;
        }

        public static implicit operator Color32(ColorBGRA color)
        {
            return new Color32(color.red, color.green, color.blue, color.alpha);
        }

        public static implicit operator ColorBGRA(Color32 color)
        {
            return new ColorBGRA(color.b, color.g, color.r, color.a);
        }
    }
}
