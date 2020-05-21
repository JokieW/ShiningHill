using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace SH.GameData.Shared
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct ColorRGBA5551
    {
        public byte one;
        public byte two;

        public void Unpack(out byte r, out byte g, out byte b, out byte a)
        {
            //Thanks de_lof
            int _r = (two & 0x7c) << 1;
            int _g = ((two & 0x03) << 6) | ((one & 0xe0) >> 2);
            int _b = (one & 0x1f) << 3;
            int _a = (two & 0x80) != 0 ? 255 : 0;
            _r |= _r >> 5;
            _g |= _g >> 5;
            _b |= _b >> 5;
            r = (byte)_r;
            g = (byte)_g;
            b = (byte)_b;
            a = (byte)_a;
        }

        public static implicit operator Color32(ColorRGBA5551 color)
        {
            color.Unpack(out byte r, out byte g, out byte b, out byte a);
            return new Color32(r, g, b, a);
        }
    }
}
