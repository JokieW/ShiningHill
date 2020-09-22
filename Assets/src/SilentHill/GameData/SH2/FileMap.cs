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
                    FileMapSubFile subFile = subFiles[i];
                    if (subFile != null && subFile.GetSubFileHeader().subFileType == 2)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void GetTextureFiles(List<SubFileTex> files)
        {
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    FileMapSubFile subFile = subFiles[i];
                    if (subFile != null && subFile.GetSubFileHeader().subFileType == 2)
                    {
                        files.Add((SubFileTex)subFiles[i]);
                    }
                }
            }
        }

        public SubFileTex GetMainTextureFile()
        {
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    FileMapSubFile subFile = subFiles[i];
                    if (subFile != null && subFile.GetSubFileHeader().subFileType == 2)
                    {
                        return (SubFileTex)subFiles[i];
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
                    FileMapSubFile subFile = subFiles[i];
                    if (subFile != null && subFile.GetSubFileHeader().subFileType == 1)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void GetGeometryFiles(List<SubFileGeometry> files)
        {
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    FileMapSubFile subFile = subFiles[i];
                    if (subFile != null && subFile.GetSubFileHeader().subFileType == 1)
                    {
                        files.Add((SubFileGeometry)subFiles[i]);
                    }
                }
            }
        }

        public SubFileGeometry GetMainGeometryFile()
        {
            if (subFiles != null)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
                    FileMapSubFile subFile = subFiles[i];
                    if (subFile != null && subFile.GetSubFileHeader().subFileType == 1)
                    {
                        return (SubFileGeometry)subFiles[i];
                    }
                }
            }
            return null;
        }

        public static FileMap ReadMapFile(string path, bool readTextureFiles = true, bool readGeometryFiles = true)
        {
            FileMap mapFile = new FileMap();
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryReader reader = new BinaryReader(file))
            {
                mapFile.ReadFile(reader, readTextureFiles, readGeometryFiles);
            }
            return mapFile;
        }

        public void ReadFile(BinaryReader reader, bool readTextureFiles = true, bool readGeometryFiles = true)
        {
            header = reader.ReadStruct<Header>();

            subFiles = new FileMapSubFile[header.fileCount];
            for (int i = 0; i < header.fileCount; i++)
            {
                SubFileHeader subFileHeader = reader.ReadStruct<SubFileHeader>();
                if(subFileHeader.subFileType == 2 && readTextureFiles)
                {
                    SubFileTex file = new SubFileTex();
                    file.subFileHeader = subFileHeader;
                    file.ReadFile(reader);
                    subFiles[i] = file;
                }
                else if (subFileHeader.subFileType == 1 && readGeometryFiles)
                {
                    SubFileGeometry file = new SubFileGeometry();
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
