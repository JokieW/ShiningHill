using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace SH.GameData.Shared
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct ColorARGB
    {
        public byte alpha;
        public byte red;
        public byte green;
        public byte blue;

        public ColorARGB(byte a, byte r, byte g, byte b)
        {
            alpha = a;
            red = r;
            green = g;
            blue = b;
        }

        public static implicit operator Color32(ColorARGB color)
        {
            return new Color32(color.red, color.green, color.blue, color.alpha);
        }

        public static implicit operator ColorARGB(Color32 color)
        {
            return new ColorARGB(color.a, color.r, color.g, color.b);
        }
    }
}
