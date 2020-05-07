using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace SH.DataFormat.Shared
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct ColorRGBA5551
    {
        public byte one;
        public byte two;

        public static implicit operator Color32(ColorRGBA5551 color)
        {
            //Thanks de_lof
            int r = (color.two & 0x7c) << 1;
            int g = ((color.two & 0x03) << 6) | ((color.one & 0xe0) >> 2);
            int b = (color.one & 0x1f) << 3;
            int a = (color.two & 0x80) != 0 ? 255 : 0;
            r |= r >> 5;
            g |= g >> 5;
            b |= b >> 5;
            return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }
}
