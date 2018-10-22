using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SourceHandler
{
    static Dictionary<byte, Func<string, SourceHandler>> _handlers = new Dictionary<byte, Func<string, SourceHandler>>()
    {
        { 0, (s) => { return null; } },
        { 1, SH1_BinCueSource.DetectCompatibility }
    };

    /*static Dictionary<byte, ConstructorInfo> _fileSystems = new Dictionary<byte, ConstructorInfo>()
    {
        { 0, null },
        { 1, typeof(ISO9660FS).GetConstructor(_instantiateTypes) }
    };
    static object[] _instantiateBuffer = new object[] { null, 0L, 0L };
    static readonly Type[] _instantiateTypes = new Type[] { typeof(IXStream), typeof(long), typeof(long) };

    public static SourceHandler Instantiate(byte fsid, IXStream stream, long start, long length)
    {
        ConstructorInfo t = _fileSystems[fsid];
        _instantiateBuffer[0] = stream;
        _instantiateBuffer[1] = start;
        _instantiateBuffer[2] = length;
        return (SourceHandler)t.Invoke(_instantiateBuffer);
    }*/

    public static bool GetHandlerForPath(string path, out SourceHandler handler, out byte handlerID)
    {
        foreach(KeyValuePair<byte, Func<string, SourceHandler>> kvp in _handlers)
        {
            SourceHandler sh = kvp.Value(path);
            if(sh != null)
            {
                handler = sh;
                handlerID = kvp.Key;
                return true;
            }
        }
        handler = null;
        handlerID = 0;
        return false;
    }

    public virtual string description
    {
        get { return null; }
    }

    public virtual void Init(string path)
    {
    }

    public virtual void Close()
    {
    }

    public virtual DirectoryEntry GetDirectories()
    {
        return null;
    }
}

public class DirectoryEntry
{
    public DirectoryEntry parent;
    public DirectoryEntry[] subentries;
    public string name;
    public DirFlags flags;
    public byte specialFS;

    [Flags]
    public enum DirFlags : byte
    {
        None = 0x00,
        IsFile = 0x01,
        IsArchive = 0x02
    }
}
