using SH.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SH.GameData.SH2
{
    [Serializable]
    public class FileTex : IFileMapSubFile
    {
        public FileMap.SubFileHeader subFileHeader;
        public Header header;
        public DXTTexture[] textures;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            public uint magicBytes; //0x19990901
            public int field_04;
            public int field_08;
            public int field_0C;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public class DXTTexture
        {
            public Header header;
            public Subgroup[] subgroups;
            public PixelHeader pixelHeader;
            public byte[] pixels;

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Header
            {
                [Hex] public short textureId;
                [Hex] public short field_02;
                public short width;
                public short height;
                [Hex] public short field_08;
                [Hex] public short field_0A;
                public int subgroupCount;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct Subgroup
            {
                [Hex] public short field_00;
                [Hex] public short field_02;
                [Hex] public int field_04;
                [Hex] public int field_08;
                [Hex] public int  field_0C;

                [Hex] public short subId;
                [Hex] public short field_12;
                [Hex] public int field_14;
                [Hex] public short field_18;
                [Hex] public short field_1A;
                [Hex] public short field_1C;
                [Hex] public short field_1E;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, Pack = 0)]
            public struct PixelHeader
            {
                [Hex] public int pixelLength;
                [Hex] public int headerAndPixelLength__;
                [Hex] public int field_08;
                [Hex] public int field_0C;
            }
        }

        public void ReadFile(BinaryReader reader)
        {
            header = reader.ReadStruct<Header>();

            CollectionPool.Request(out List<DXTTexture> newTextures);
            while (true)
            {
                DXTTexture.Header textureHeader = reader.ReadStruct<DXTTexture.Header>();
                if(textureHeader.textureId == 0)
                {
                    break;
                }
                
                DXTTexture newTexture = new DXTTexture();
                newTexture.header = textureHeader;
                newTexture.subgroups = new DXTTexture.Subgroup[newTexture.header.subgroupCount];
                for (int i = 0; i < newTexture.subgroups.Length; i++)
                {
                    newTexture.subgroups[i] = reader.ReadStruct<DXTTexture.Subgroup>();
                }
                newTexture.pixelHeader = reader.ReadStruct<DXTTexture.PixelHeader>();
                newTexture.pixels = new byte[newTexture.pixelHeader.pixelLength];
                reader.ReadBytes(newTexture.pixels);
                newTextures.Add(newTexture);
            }

            textures = newTextures.ToArray();
            CollectionPool.Return(ref newTextures);
        }

        public void WriteFile(BinaryWriter writer)
        {

        }

        public FileMap.SubFileHeader GetSubFileHeader()
        {
            return subFileHeader;
        }
    }

}
