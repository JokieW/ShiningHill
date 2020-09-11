using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace SH.GameData.Shared
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    public struct ColorBC2
    {
        public ushort alpha_abcd;
        public ushort alpha_efgh;
        public ushort alpha_ijkl;
        public ushort alpha_mnop;
        public ushort color_0;
        public ushort color_1;
        public byte abcd;
        public byte efgh;
        public byte ijkl;
        public byte mnop;

        public static unsafe void BufferToColorRGBA8888(Color[] pixels, byte[] bcPixels, int width, int height)
        {
            fixed (void* voidColorPtr = pixels)
            fixed (void* voidBlockPtr = bcPixels)
            {
                Color* colorPtr = (Color*)voidColorPtr;
                ColorBC2* blockPtr = (ColorBC2*)voidBlockPtr;

                int blockWidth = width / 4;
                int blockHeight = height / 4;
                BCUtil.ColorBlock block = new BCUtil.ColorBlock();
                for (int blockIndex = 0; blockIndex < blockWidth * blockHeight; blockIndex++)
                {
                    int bx = blockIndex % blockWidth;
                    int by = blockIndex / blockWidth;
                    int px = bx * 4;
                    int py = by * 4;

                    block.a = colorPtr + ((py * width) + px);
                    block.e = colorPtr + (((py + 1) * width) + px);
                    block.i = colorPtr + (((py + 2) * width) + px);
                    block.m = colorPtr + (((py + 3) * width) + px);

                    blockPtr[blockIndex].ToColorRGBA8888(ref block);
                }
            }
        }

        public unsafe void ToColorRGBA8888(ref BCUtil.ColorBlock block)
        {
            for (byte i = 0; i < 4; i++)
            {
                *(block.a + i) = GetColor(abcd, i, alpha_abcd);
                *(block.e + i) = GetColor(efgh, i, alpha_efgh);
                *(block.i + i) = GetColor(ijkl, i, alpha_ijkl);
                *(block.m + i) = GetColor(mnop, i, alpha_mnop);
            }
        }

        private Color32 UnpackColor(ushort color)
        {
            float r = (color & 0xF800) >> 11;
            float g = (color & 0x7E0) >> 5;
            float b = color & 0x1F;
            return new Color(r / 31.0f, g / 63.0f, b / 31.0f, 1.0f);
        }

        private Color GetColor(byte colorByte, byte colorIndex, ushort alphaShort)
        {
            int code;
            if (colorIndex == 0) code = colorByte & 0x03;
            else if (colorIndex == 1) code = (colorByte & 0x0C) >> 2;
            else if (colorIndex == 2) code = (colorByte & 0x30) >> 4;
            else if (colorIndex == 3) code = (colorByte & 0xC0) >> 6;
            else throw new IndexOutOfRangeException();

            float alpha;
            if (colorIndex == 0) alpha = (alphaShort & 0x000F) / 15.0f;
            else if (colorIndex == 1) alpha = ((alphaShort & 0x00F0) >> 4) / 15.0f;
            else if (colorIndex == 2) alpha = ((alphaShort & 0x0F00) >> 8) / 15.0f;
            else if (colorIndex == 3) alpha = ((alphaShort & 0xF000) >> 12) / 15.0f;
            else throw new IndexOutOfRangeException();

            if (code == 0)
            {
                Color newColor = UnpackColor(color_0);
                newColor.a = alpha;
                return newColor;
            }
            else if (code == 1)
            {
                Color newColor = UnpackColor(color_1);
                newColor.a = alpha;
                return newColor;
            }
            else if (code == 2)
            {
                Color color0 = UnpackColor(color_0);
                Color color1 = UnpackColor(color_1);
                Color newColor = new Color
                {
                    r = (2 * color0.r + color1.r) / 3,
                    g = (2 * color0.g + color1.g) / 3,
                    b = (2 * color0.b + color1.b) / 3,
                    a = alpha
                };
                return newColor;
            }
            else if (code == 3)
            {
                Color color0 = UnpackColor(color_0);
                Color color1 = UnpackColor(color_1);
                Color newColor = new Color
                {
                    r = (color0.r + 2 * color1.r) / 3,
                    g = (color0.g + 2 * color1.g) / 3,
                    b = (color0.b + 2 * color1.b) / 3,
                    a = alpha
                };
                return newColor;
            }
            else throw new Exception();

        }
    }
}
