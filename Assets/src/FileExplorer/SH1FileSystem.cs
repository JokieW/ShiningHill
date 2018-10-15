using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;

namespace ShiningHill
{
    public class SH1FileSystem 
	{
        static readonly string[] exts = new string[13]
        {
            ".TIM", ".VAB", ".BIN", ".DMS", ".ANM", ".PLM", ".IPD", ".ILM", ".TMD", ".DAT", ".KDT", ".CMP", ".TXT"
        };
        static readonly string[] dirs = new string[11]
        {
            "1ST/", "ANIM/", "BG/", "CHARA/", "ITEM/", "MISC/", "SND/", "TEST/", "TIM/", "VIN/", "XA/"
        };
        static readonly uint baseFileDataPosition = 0xB91C;
        static readonly uint fileCount = 2044; /*2074*/

        public struct FileData
        {
            public uint a;
            public uint b;
            public uint c;
            public uint offset;

            public uint size
            {
                get { return ((a >> 0x13) & 0xFFFF) << 8; }
            }

            public string fullname
            {
                get { return 
                        ((b & 0x0F) >= 11 ? "BAD/" : dirs[b & 0x0F]) +
                        DataUtils.DecodeSH1Name(b, c) +
                        (c >> 24 >= 13 ? "" : exts[c >> 24]); }
            }

            public FileData(uint a, uint b, uint c, ref uint offset, int i)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                offset += (((((a >> 0x19) & 0xFFF) << 8) + 0x7FF) / 0x800) * 0x800;
                this.offset = offset;

                /*if( ((a & 0xE0000000) != 0) ||
                    ((a & 0x1FFF) != 0) ||
                    ((b & 0xF0000000) != 0) ||
                    ((c & 0xF0000000) != 0))
                {
                    Debug.LogError("Unexpected data in FileData " + i + " - " + fullname);
                    Debug.LogError(
                        "a1 = '" + ((a & 0xE0000000) >> 29) +
                        "' a11 = '" + ((a & 0x1FFFE000) >> 5) +
                        "' a2 = '" + (a & 0x1FFF) +
                        "' b = '" + ((b & 0xF0000000) >> 28) +
                        "' c1 = '" + ((c & 0xF0000000) >> 28) +
                        "' c12 = '" + ((c & 0xFF000000) >> 24) + "'");
                }*/
            }
        }

        public static void ImportAll()
        {
            CustomPostprocessor.DoImports = false;
            string hardAssetPath = CustomPostprocessor.GetHardDataPathFor(SHGame.SH1);

            BinaryReader exeReader = new BinaryReader(new FileStream(hardAssetPath + "SLUS_007.07", FileMode.Open, FileAccess.Read, FileShare.Read));
            BinaryReader silentReader = new BinaryReader(new FileStream(hardAssetPath + "data/SILENT", FileMode.Open, FileAccess.Read, FileShare.Read));
            try
            {
                exeReader.BaseStream.Position = baseFileDataPosition;
                uint offset = 0;
                for (int i = 0; i != fileCount; i++)
                {
                    FileData fd = new FileData(exeReader.ReadUInt32(), exeReader.ReadUInt32(), exeReader.ReadUInt32(), ref offset, i);
                    if(fd.size == 0u)
                    {
                        Debug.Log("");
                    }
                    silentReader.BaseStream.Position = fd.offset;
                    string fullpath = hardAssetPath + "Data/" + fd.fullname;
                    string path = Path.GetDirectoryName(fullpath);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    File.WriteAllBytes(fullpath, silentReader.ReadBytes((int)fd.size));
                }
            }
            finally
            {
                exeReader.Close();
                silentReader.Close();
            }

            AssetDatabase.Refresh();
            CustomPostprocessor.DoImports = true;
        }
	}
}