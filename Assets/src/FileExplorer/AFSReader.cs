using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShiningHill
{
    public struct AFSAssetPaths
    {
        string afsname;
        string genericPath;
        SHGame game;

        public AFSAssetPaths(string hardAssetPath, SHGame forgame)
        {
            afsname = Path.GetFileNameWithoutExtension(hardAssetPath);
            genericPath = Path.GetDirectoryName(hardAssetPath).Substring(hardAssetPath.LastIndexOf("/sound/") + 1).Replace("\\", "/") + "/";
            game = forgame;
        }

        public string GetHardAssetPath()
        {
            string path = CustomPostprocessor.GetHardDataPathFor(game);
            return path + genericPath + afsname + ".afs";
        }

        public string GetExtractAssetPath(string filenameWExt)
        {
            string path = CustomPostprocessor.GetExtractDataPathFor(game);
            return path + genericPath + afsname + "/" + filenameWExt;
        }
    }

    public class AFSReader
    {
        private struct FileOffsets
        {
            public int offset;
            public int length;
        }

        private struct FileInfo
        {
            public string filename;
            public short year;
            public short month;
            public short day;
            public short hour;
            public short minute;
            public short second;
            public int something; 
        }

        public static void ReadAFSFiles(AFSAssetPaths paths)
        {
            BinaryReader reader = null;
            try
            {
                reader = new BinaryReader(new FileStream(paths.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));

                /*string afstag = */
                reader.ReadNullTerminatedString();
                int fileCount = reader.ReadInt32();

                //Get all offsets
                ulong assumedFileInfoOffset = 0;
                FileOffsets[] _fileOffsets = new FileOffsets[fileCount];
                for (int i = 0; i != fileCount; i++)
                {
                    _fileOffsets[i] = new FileOffsets() { offset = reader.ReadInt32(), length = reader.ReadInt32() };
                    if (i == fileCount - 1)
                    {
                        assumedFileInfoOffset = (ulong)(_fileOffsets[i].offset + _fileOffsets[i].length);
                        if (assumedFileInfoOffset % 0x800uL != 0)
                        {
                            assumedFileInfoOffset = (assumedFileInfoOffset & 0xFFFFF800uL) + 0x800uL;
                        }
                    }
                }
                int fileInfoOffset = reader.ReadInt32();
                int fileInfolength = reader.ReadInt32();

                //Get all file info
                reader.BaseStream.Position = fileInfoOffset == 0 ? (long)assumedFileInfoOffset : fileInfoOffset;
                FileInfo[] _fileInfos = new FileInfo[fileCount];
                for (int i = 0; i != fileCount; i++)
                {
                    _fileInfos[i] = new FileInfo()
                    {
                        filename = reader.ReadStringBuffer(32),
                        year = reader.ReadInt16(),
                        month = reader.ReadInt16(),
                        day = reader.ReadInt16(),
                        hour = reader.ReadInt16(),
                        minute = reader.ReadInt16(),
                        second = reader.ReadInt16(),
                        something = reader.ReadInt32(),
                    };
                }

                //Create files
                for (int i = 0; i != fileCount; i++)
                {
                    FileStream fs = null;
                    try
                    {
                        FileOffsets offset = _fileOffsets[i];
                        FileInfo info = _fileInfos[i];
                        reader.BaseStream.Position = offset.offset;

                        string folder = paths.GetExtractAssetPath("");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                        string filepath = paths.GetExtractAssetPath(_fileInfos[i].filename);
                        if (File.Exists(filepath)) File.Delete(filepath);

                        fs = File.Create(filepath, _fileOffsets[i].length);
                        byte[] fileData = reader.ReadBytes(offset.length);
                        fs.Write(fileData, 0, fileData.Length);
                        fs.Close();

                        DateTime time = new DateTime(info.year, info.month, info.day, info.hour, info.minute, info.second);
                        File.SetCreationTime(filepath, time);
                        File.SetLastWriteTime(filepath, time);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    finally
                    {
                        if (fs != null) fs.Close();
                    }
                }

                reader.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }
    }
}