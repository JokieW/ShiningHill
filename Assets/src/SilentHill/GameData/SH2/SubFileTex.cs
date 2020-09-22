using SH.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SH.GameData.SH2
{
    [Serializable]
    public class SubFileTex : FileMapSubFile
    {
        public FileMap.SubFileHeader subFileHeader;
        public Header header;
        public DXTTexture[] textures;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            public uint magicBytes; //0x19990901
            public int field_04; //always 0
            public int field_08; //always 0
            public int field_0C; //always 1
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public class DXTTexture
        {
            public Header header;
            public Sprite[] sprites; //Last sprite is always the main one

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public int textureId; // max FFFF
                public short width;
                public short height;
                public short width2; // Always the same as width
                public short height2; // Always the same as height
                public int spriteCount;

                [Hex] public short field_10; // 1 2 3 4 5 6 7 8 9 A B C D E F 10 28
                [Hex] public short field_12; // Always field_00
                [Hex] public int field_14; // Always 0
                [Hex] public int field_18; // Always 0
                [Hex] public int field_1C; // Always 0
            }

            [Serializable]
            public class Sprite
            {
                public SpriteHeader header;
                public PixelHeader pixelHeader;
                public byte[] pixels;

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct SpriteHeader
                {
                    [Hex] public int spriteId; // max FFFF
                    public short x; // x position of the sprite in texture
                    public short y; // y position of the sprite in texture
                    public short width; // width of the sprite
                    public short height; // height of the sprite
                    [Hex] public int format; // 100 102 103 104
                }

                [Serializable]
                [StructLayout(LayoutKind.Sequential, Pack = 0)]
                public struct PixelHeader
                {
                    [Hex] public int pixelDataLength;
                    [Hex] public int pixelHeaderAndDataLength;
                    [Hex] public int field_08; // Always 0
                    [Hex] public int field_0C; // Always 99000000
                }
            }
        }

        public void ReadFile(BinaryReader reader)
        {
            header = reader.ReadStruct<Header>();

            CollectionPool.Request(out List<DXTTexture> newTextures);
            while (reader.PeekInt32() != 0)
            {
                DXTTexture newTexture = new DXTTexture();
                newTexture.header = reader.ReadStruct<DXTTexture.Header>();
                newTexture.sprites = new DXTTexture.Sprite[newTexture.header.spriteCount];
                for (int i = 0; i < newTexture.sprites.Length; i++)
                {
                    DXTTexture.Sprite sprite = new DXTTexture.Sprite();
                    sprite.header = reader.ReadStruct<DXTTexture.Sprite.SpriteHeader>();
                    sprite.pixelHeader = reader.ReadStruct<DXTTexture.Sprite.PixelHeader>();
                    sprite.pixels = new byte[sprite.pixelHeader.pixelDataLength];
                    reader.ReadBytes(sprite.pixels);
                    newTexture.sprites[i] = sprite;
                }
                
                newTextures.Add(newTexture);
            }

            reader.SkipBytes(16);

            textures = newTextures.ToArray();
            CollectionPool.Return(ref newTextures);
        }

        public void WriteFile(BinaryWriter writer)
        {

        }

        public override FileMap.SubFileHeader GetSubFileHeader()
        {
            return subFileHeader;
        }
    }

}
