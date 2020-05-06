using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShiningHill
{
    public class FileSystemBase : System.IDisposable
    {
        static FileSystemHandler[] _handlers = new FileSystemHandler[]
        {
            new FileSystemHandler(0),
            new FileSystemHandler<ISO9660FS>(1),
            new FileSystemHandler<SH1FileSystem>(2)
        };

        public static FileSystemHandler GetHandlerForID(byte id)
        {
            for (int i = 0; i != _handlers.Length; i++)
            {
                if (_handlers[i].id == id) return _handlers[i];
            }
            return null;
        }

        public static byte GetIdForType<T>()
        {
            return GetIdForType(typeof(T));
        }

        public static byte GetIdForType(Type type)
        {
            for (int i = 0; i != _handlers.Length; i++)
            {
                if (_handlers[i].GetFSType() == type) return _handlers[i].id;
            }
            return 0;
        }

        public virtual XStream OpenFile(DirectoryEntry root, string path) { return null; }
        public virtual FileSystemBase Instantiate(XStream stream) { return null; }
        public virtual void SetUniformDirectories(SourceBase source, ref DirectoryEntry self) { }
        public virtual void Dispose() { }

        public class FileSystemHandler
        {
            protected byte _id;
            public byte id { get { return _id; } }
            public FileSystemHandler(byte id) { _id = id; }
            public virtual Type GetFSType() { return null; }
            public virtual FileSystemBase Instantiate(XStream stream) { return null; }
        }

        public class FileSystemHandler<T> : FileSystemHandler where T : FileSystemBase, new()
        {
            T dummyFS = new T();

            public FileSystemHandler(byte id) : base(id) { }

            public override Type GetFSType()
            {
                return typeof(T);
            }

            public override FileSystemBase Instantiate(XStream stream)
            {
                return dummyFS.Instantiate(stream);
            }
        }
    }
}
