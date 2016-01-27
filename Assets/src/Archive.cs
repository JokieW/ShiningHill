using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class Archive
{
    private Object _archiveFile;

    public Object File
    {
        get
        {
            return _archiveFile;
        }
        set
        {
            _archiveFile = value;
        }
    }

    ArcHeader _header = null;
    Dictionary<string, ArcFile> _files = null;
    public Dictionary<string, ArcFile> AllFiles
    {
        get
        {
            return _files;
        }
    }

    public Archive(Object file)
    {
        _archiveFile = file;
    }

    public bool IsLoaded()
    {
        return _header != null && _files != null;
    }

    public void OpenArchive()
    {
        string assetPath = AssetDatabase.GetAssetPath(_archiveFile);
        BinaryReader reader = null;
        try
        {
            reader = new BinaryReader(new FileStream(assetPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            _header = new ArcHeader(reader);

            _files = new Dictionary<string, ArcFile>((int)_header.fileCount);
            for (int fileI = 0; fileI != (int)_header.fileCount; fileI++)
            {
                ArcFile file = new ArcFile(reader);
                string name = String.Format("File {0} ({1})", fileI, file.FileID);
                _files.Add(name, file);
            }
        }
        catch(Exception e)
        {
            Debug.LogError("Path " + assetPath);
            Debug.LogException(e);
            if (reader != null)
            {
                reader.Close();
            }
        }
    }

    public void UnloadArchive()
    {
        _header = null;
        _files = null;
    }

    public class ArcHeader
    {
        public UInt32 firstUint32;
        public UInt32 fileCount;
        public UInt32 paddingLength;
        public UInt32 fourthUint32;

        public ArcHeader(BinaryReader reader)
        {
            reader.BaseStream.Position = 0L;
            firstUint32 = reader.ReadUInt32();
            fileCount = reader.ReadUInt32();
            paddingLength = reader.ReadUInt32();
            fourthUint32 = reader.ReadUInt32();
        }
    }

    public class ArcFile
    {
        public UInt32 Offset;
        public UInt32 FileID; //Unknown
        public UInt32 Length;
        public UInt32 Lenght2; //Not quite sure why there is two length
        public byte[] data;

        public ArcFile(BinaryReader reader)
        {
            Offset = reader.ReadUInt32();
            FileID = reader.ReadUInt32();
            Length = reader.ReadUInt32();
            Lenght2 = reader.ReadUInt32();

            long currentSeek = reader.BaseStream.Position;
            reader.BaseStream.Position = (long)Offset;
            data = reader.ReadBytes((int)Length);
            reader.BaseStream.Position = currentSeek;
        }
    }
}
