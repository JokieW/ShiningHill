using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace ShiningHill
{
    public class SourceBase
    {
        static SourceHandler[] _handlers = new SourceHandler[]
        {
            new SourceHandler(0),
            new SourceHandler<SH1_BinCueSource>(1)
        };

        public static SourceHandler GetHandlerForPath(string path)
        {
            for (int i = 0; i != _handlers.Length; i++)
            {
                SourceHandler sh = _handlers[i];
                if (sh.DetectCompatibility(path))
                {
                    return sh;
                }
            }
            return null;
        }

        public static SourceHandler GetHandlerForID(byte id)
        {
            return _handlers[id];
        }

        public void PostProcessDirectories(DirectoryEntry entries)
        {
            IdentifierBase.RunArchiveIdentifiers(entries);
            ExpandArchives(entries);

            //run ids
        }

        public void ExpandArchives(DirectoryEntry entries)
        {
            for (int i = 0; i != entries.subentries.Length; i++)
            {
                DirectoryEntry de = entries.subentries[i];
                if ()
            }
        }

        DirectoryEntry _directories;
        public void SetDirectories(DirectoryEntry entries)
        {
            _directories = entries;
        }

        public BinaryReader OpenFile(string path)
        {
            FileSystemBase.FileSystemHandler fsh = FileSystemBase.GetHandlerForID(_directories.specialFS);
            using (FileSystemBase fs = fsh.Instantiate(GetStream().MakeSubStream()))
            {
                return fs.OpenFile(path);
            }
        }

        protected virtual XStream GetStream() { return null; }
        public virtual string description { get { return null; } }
        public virtual void Init(string path) { }
        public virtual void Close() { }
        public virtual bool DetectCompatibility(string path) { return false; }

        public class SourceHandler
        {
            protected byte _id;
            public byte id { get { return _id; } }
            public SourceHandler(byte id) { _id = id; }
            public virtual string description { get { return "bad handler"; } }
            public virtual bool DetectCompatibility(string path) { return false; }
            public virtual SourceBase Instantiate(string path) { return null; }
        }

        public class SourceHandler<T> : SourceHandler where T : SourceBase, new()
        {
            T dummySource = new T();

            public SourceHandler(byte id) : base(id) { }

            public override string description
            {
                get { return dummySource.description; }
            }

            public override bool DetectCompatibility(string path)
            {
                return dummySource.DetectCompatibility(path);
            }

            public override SourceBase Instantiate(string path)
            {
                T s = new T();
                s.Init(path);
                return s;
            }
        }
    }

    public struct DirectoryBrowser
    {
        DirectoryEntry _entries;
        public DirectoryBrowser(DirectoryEntry entries)
        {
            _entries = entries;
        }

        private bool CompareNameAtLevel(string path, string name, int level)
        {
            int location = 0;
            int lookupLevel = -1;
            while (location < path.Length)
            {
                if (path[location++] == '/')
                {
                    lookupLevel++;
                    if (lookupLevel == level) break;
                }
            }

            int i = 0;
            while (true)
            {
                if (location >= path.Length || i >= name.Length || path[location++] != name[i++]) return false;
                if (i == name.Length) return true;
            }
        }

        public bool Exists(string path)
        {
            DirectoryEntry de;
            return TryGetEntry(path, out de);
        }

        public bool TryGetEntry(string path, out DirectoryEntry entry)
        {
            try
            {
                entry = GetEntry(path);
                return true;
            }
            catch { }
            entry = null;
            return false;
        }

        public DirectoryEntry GetEntry(string path)
        {
            DirectoryEntry current = _entries;
            int currentLevel = 0;
            int currentLength = 1;

            GODEEP:
            foreach (DirectoryEntry de in current)
            {
                if (CompareNameAtLevel(path, de.name, currentLevel))
                {
                    current = de;
                    currentLevel++;
                    currentLength += de.name.Length;
                    if (de.isFolder) currentLength++;
                    if (currentLength == path.Length) return current;
                    goto GODEEP;
                }
            }
            throw new DirectoryNotFoundException(path);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class DirectoryEntry : IListable
    {
        public DirectoryEntry parent;
        public DirectoryEntry[] subentries;
        public string name;
        public long fileAddress;
        public long fileLength;
        public DirFlags flags;
        public byte specialFS;
        public byte fileType;

        public bool isExpanded
        {
            get
            {
                return (flags & DirFlags.IsExpanded) != 0;
            }

            set
            {
                if (value) flags |= DirFlags.IsExpanded;
                else flags &= ~DirFlags.IsExpanded;
            }
        }

        public bool isFolder { get { return (flags & DirFlags.IsFile) == 0 || (flags & DirFlags.IsArchive) != 0; } }
        public bool isFile { get { return (flags & DirFlags.IsFile) != 0; } }

        public bool hasChildren
        {
            get
            {
                return subentries != null && subentries.Length != 0;
            }
        }

        public void Draw()
        {
            EditorGUILayout.LabelField(name);
        }

        public override string ToString()
        {
            return name;
        }

        public IEnumerator<IListable> GetEnumerator()
        {
            if (subentries != null)
            {
                for (int i = 0; i != subentries.Length; i++)
                {
                    yield return subentries[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [Flags]
        public enum DirFlags : byte
        {
            None = 0x00,
            IsFile = 0x01,
            IsArchive = 0x02,

            IsExpanded = 0x80
        }
    }
}
