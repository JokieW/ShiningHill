using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class TIMReader
{
    public uint id;
    public uint flag;
    public CLUT clut;
    public Image image;

    public PMODE pmode
    {
        get { return (PMODE)(flag & 0x7); }
    }

    public bool isClut
    {
        get { return (flag & 0x8) != 0; }
    }

    public Texture2D GenerateImage()
    {
        Color32[] pixels = new Color32[image.width * image.height];
        int i = 0;
        switch (pmode)
        {
            case PMODE.CLUT4:
                for(int y = image.height - 1; y != -1; y--)
                {
                    for(int x = 0; x != image.width; x++)
                    {
                        pixels[i++] = clut.entries[image.entries4[(image.width * y) + x]].color32;
                    }
                }
                break;

            case PMODE.CLUT8:
                for (int y = image.height - 1; y != -1; y--)
                {
                    for (int x = 0; x != image.width; x++)
                    {
                        pixels[i++] = clut.entries[image.entries8[(image.width * y) + x]].color32;
                    }
                }
                break;

            case PMODE.DIRECT15:
                for (int y = image.height - 1; y != -1; y--)
                {
                    for (int x = 0; x != image.width; x++)
                    {
                        pixels[i++] = image.entries16[(image.width * y) + x].color32;
                    }
                }
                break;

            case PMODE.DIRECT24:
                for (int y = image.height - 1; y != -1; y--)
                {
                    for (int x = 0; x != image.width; x++)
                    {
                        pixels[i++] = image.entries24[(image.width * y) + x];
                    }
                }
                break;
        }

        Texture2D t = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false, false);
        t.SetPixels32(pixels);
        t.alphaIsTransparency = true;
        t.Apply();
        return t;
    }

    public static TIMReader ReadFile(string path, long offset = 0L)
    {
        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        reader.BaseStream.Position = offset;

        try
        {
            TIMReader tf = new TIMReader();
            tf.id = reader.ReadUInt32();
            tf.flag = reader.ReadUInt32();

            if (tf.isClut)
            {
                uint length = reader.ReadUInt32();
                ushort x = reader.ReadUInt16();
                ushort y = reader.ReadUInt16();
                ushort width = reader.ReadUInt16();
                ushort height = reader.ReadUInt16();
                uint n = (length - 12) / 2;
                Color16[] entries = new Color16[n];
                for (int i = 0; i != n; i++)
                {
                    entries[i] = new Color16() { value = reader.ReadUInt16() };
                }
                tf.clut = new CLUT() { x = x, y = y, width = width, height = height, entries = entries };
            }

            {
                uint length = reader.ReadUInt32();
                ushort x = reader.ReadUInt16();
                ushort y = reader.ReadUInt16();
                ushort width = reader.ReadUInt16();
                ushort height = reader.ReadUInt16();

                switch (tf.pmode)
                {
                    case PMODE.CLUT4:
                        {
                            uint n = (length - 12) / 2;
                            byte[] entries = new byte[n * 4];
                            for (int i = 0, j = 0; i != n; i++)
                            {
                                ushort entry = reader.ReadUInt16();
                                entries[j++] = (byte)((entry & 0x000F));
                                entries[j++] = (byte)((entry & 0x00F0) >> 0x04);
                                entries[j++] = (byte)((entry & 0x0F00) >> 0x08);
                                entries[j++] = (byte)((entry & 0xF000) >> 0x0C);
                            }
                            tf.image = new Image() { x = x, y = y, width = (ushort)(width * 4), height = height, entries4 = entries };
                        }
                        break;

                    case PMODE.CLUT8:
                        {
                            uint n = length - 12;
                            byte[] entries = new byte[n];
                            for (int i = 0; i != n; i++)
                            {
                                entries[i] = reader.ReadByte();
                            }
                            tf.image = new Image() { x = x, y = y, width = (ushort)(width * 2), height = height, entries8 = entries };
                        }
                        break;

                    case PMODE.DIRECT15:
                        {
                            uint n = (length - 12) / 2;
                            Color16[] entries = new Color16[n];
                            for (int i = 0; i != n; i++)
                            {
                                entries[i] = new Color16() { value = reader.ReadUInt16() };
                            }
                            tf.image = new Image() { x = x, y = y, width = width, height = height, entries16 = entries };
                        }
                        break;

                    case PMODE.DIRECT24:
                        {
                            uint n = (length - 12) / 3;
                            Color32[] entries = new Color32[n];
                            for (int i = 0; i != n; i++)
                            {
                                entries[i] = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 255);
                            }
                            tf.image = new Image() { x = x, y = y, width = (ushort)(width * 2), height = height, entries24 = entries };
                        }
                        break;
                }
            }
            return tf;
        }
        finally
        {
            reader.Close();
        }
    }

    public static void ScanForTIMs(string path)
    {
        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        try
        {
            int hits = 0;
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                byte b = reader.ReadByte();
                if (b == 0x10)
                {
                    reader.BaseStream.Position--;
                    long offsetApparent = reader.BaseStream.Position;
                    if (reader.ReadUInt32() != 0x00000010) goto FAILED;
                    uint flags = reader.ReadUInt32();
                    if ((flags & 0xFFFFFFF0) != 0) goto FAILED;
                    if ((flags & 0x7) > 3) goto FAILED;
                    PMODE pmode = (PMODE)(flags & 0x7);

                    if(pmode == PMODE.CLUT4 || pmode == PMODE.CLUT8)
                    {
                        uint length = reader.ReadUInt32();
                        ushort x = reader.ReadUInt16();
                        ushort y = reader.ReadUInt16();
                        ushort w = reader.ReadUInt16();
                        ushort h = reader.ReadUInt16();
                        if (x > 1023 || y > 511 || w == 0 || w != (pmode == PMODE.CLUT4 ? 16 : 256) || h == 0 || h != 1) goto FAILED;
                        if (length < 12) goto FAILED;
                        if (length != (pmode == PMODE.CLUT4 ? 44 : 524)) goto FAILED;
                        reader.BaseStream.Position += length - 12;

                    }

                    {
                        uint length = reader.ReadUInt32();
                        ushort x = reader.ReadUInt16();
                        ushort y = reader.ReadUInt16();
                        ushort w = reader.ReadUInt16();
                        ushort h = reader.ReadUInt16();
                        if (x > 1023 || y > 511 || w == 0 || w > 1023 || h == 0 || h > 511) goto FAILED;
                        if (length < 12) goto FAILED;
                        if (pmode == PMODE.CLUT4 && length != ((2 * w) * h) + 12)  goto FAILED;
                        if (pmode == PMODE.CLUT8 && length != ((2 * w) * h) + 12) goto FAILED;
                        if (pmode == PMODE.DIRECT15 && length != ((2 * w) * h) + 12) goto FAILED;
                        if (pmode == PMODE.DIRECT24 && length != ((6 * w) * h) + 12) goto FAILED;
                    }

                    hits++;
                    try
                    {
                        TIMReader tf = ReadFile(path, offsetApparent);
                        Texture2D t2d = tf.GenerateImage();
                        File.WriteAllBytes("C:/results/" + path + hits.ToString() + ".png", t2d.EncodeToPNG());
                        UnityEngine.Object.DestroyImmediate(t2d);
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e);
                        return;
                    }
                    goto END;

                    FAILED:
                    reader.BaseStream.Position = offsetApparent + 1;

                    END:
                    b = 0;
                }
            }
            Debug.Log("HITS " + hits);
        }
        finally
        {
            reader.Close();
        }
    }

    public enum PMODE
    {
        CLUT4 = 0x0,
        CLUT8 = 0x1,
        DIRECT15 = 0x2,
        DIRECT24 = 0x3,
        MIXED = 0x4
    }

    public class CLUT
    {
        public ushort x;
        public ushort y;
        public ushort width;
        public ushort height;
        public Color16[] entries;
    }

    public struct Color16
    {
        public ushort value;
        
        public byte r { get { return (byte)(value & 0x001F); } }
        public byte g { get { return (byte)((value & 0x03E0) >> 0x5); } }
        public byte b { get { return (byte)((value & 0x7C00) >> 0xA); } }
        public byte stp { get { return (byte)((value & 0x8000) >> 0xF); } }
        public Color32 color32
        {
            get
            {
                byte fr = (byte)(r * 8);
                byte fg = (byte)(g * 8);
                byte fb = (byte)(b * 8);
                byte fa = (byte)(stp * 255);
                if (fr + fg + fb != 0)
                {
                    fa = 255;// fa == 0 ? (byte)255 : (byte)127;
                }
                return new Color32(fr, fg, fb, fa);
            }
        }
        public Color color { get { return color32; } }
    }

    public class Image
    {
        public ushort x;
        public ushort y;
        public ushort width;
        public ushort height;
        public byte[] entries4;
        public byte[] entries8;
        public Color16[] entries16;
        public Color32[] entries24;
    }
}
