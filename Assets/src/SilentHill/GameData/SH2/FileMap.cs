using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using UnityEditor;

using SH.Core;

namespace SH.GameData.SH2
{
    [Serializable]
    public class FileMapSubFile
    {
        public virtual FileMap.SubFileHeader GetSubFileHeader()
        {
            return default;
        }
    }

    [Serializable]
    public class FileMap
    {
        public Header header;
        public FileMapSubFile[] subFiles;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            [Hex] public int magicByte; // 0x20010510
            [Hex] public int fileLength;
            [Hex] public int fileCount;
            [Hex] public int field_0C;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct SubFileHeader
        {
            public int subFileType; // 1 = geometry, 2 = texture
            [Hex] public int fileLength;
            [Hex] public int field_08;
            [Hex] public int field_0C;
        }

        public int GetFileCount()
        {
            int count = 0;
            if (subFiles != null)
            {
                count = subFiles.Length;
            }
            return count;
        }

        public int GetTextureFileCount()
        {
            int count = 0;
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    if (subFiles[i].GetSubFileHeader().subFileType == 2)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void GetTextureFiles(List<FileTex> files)
        {
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    if (subFiles[i].GetSubFileHeader().subFileType == 2)
                    {
                        files.Add((FileTex)subFiles[i]);
                    }
                }
            }
        }

        public FileTex GetMainTextureFile()
        {
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    if (subFiles[i].GetSubFileHeader().subFileType == 2)
                    {
                        return (FileTex)subFiles[i];
                    }
                }
            }
            return null;
        }

        public int GetGeometryFileCount()
        {
            int count = 0;
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    if (subFiles[i].GetSubFileHeader().subFileType == 1)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void GetGeometryFiles(List<FileGeometry> files)
        {
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    if (subFiles[i].GetSubFileHeader().subFileType == 1)
                    {
                        files.Add((FileGeometry)subFiles[i]);
                    }
                }
            }
        }

        public FileGeometry GetMainGeometryFile()
        {
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    if (subFiles[i].GetSubFileHeader().subFileType == 1)
                    {
                        return (FileGeometry)subFiles[i];
                    }
                }
            }
            return null;
        }

        public static FileMap ReadMapFile(string path)
        {
            FileMap mapFile = new FileMap();
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {
                mapFile.ReadFile(reader);
            }
            return mapFile;
        }

        public void ReadFile(BinaryReader reader)
        {
            header = reader.ReadStruct<Header>();

            subFiles = new FileMapSubFile[header.fileCount];
            for (int i = 0; i < header.fileCount; i++)
            {
                SubFileHeader subFileHeader = reader.ReadStruct<SubFileHeader>();
                if(subFileHeader.subFileType == 2)
                {
                    FileTex file = new FileTex();
                    file.subFileHeader = subFileHeader;
                    file.ReadFile(reader);
                    subFiles[i] = file;
                }
                else if (subFileHeader.subFileType == 1)
                {
                    FileGeometry file = new FileGeometry();
                    file.subFileHeader = subFileHeader;
                    file.ReadFile(reader);
                    subFiles[i] = file;
                }
            }
        }

        public void WriteFile(BinaryWriter writer)
        {

        }
    }
}
