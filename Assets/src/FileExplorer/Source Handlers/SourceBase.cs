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

        protected DirectoryEntry _directories;

        public DirectoryEntry GetDirectories()
        {
            return _directories;
        }

        public DirectoryBrowser GetBrowser()
        {
            return new DirectoryBrowser(_directories);
        }

        public void PostProcessDirectories(DirectoryEntry entries)
        {
            IdentifierBase.RunIdentifiers(entries);
            ExpandArchives(entries);
        }

        public void ExpandArchives(DirectoryEntry entries)
        {
            if (entries.subentries != null)
            {
                XStream stream = GetStream();
                for (int i = 0; i != entries.subentries.Length; i++)
                {
                    DirectoryEntry de = entries.subentries[i];
                    if (de.specialFS != 0)
                    {
                        FileSystemBase.FileSystemHandler fhs = FileSystemBase.GetHandlerForID(de.specialFS);
                        using (FileSystemBase fs = fhs.Instantiate(stream.MakeSubStream(de.fileAddress, de.fileLength)))
                        {
                            fs.SetUniformDirectories(this, ref de);
                            PostProcessDirectories(de);
                        }
                    }
                    else
                    {
                        ExpandArchives(de);
                    }
                }
            }
        }
        
        public XStream OpenFile(string path)
        {
            FileSystemBase.FileSystemHandler fsh = FileSystemBase.GetHandlerForID(_directories.specialFS);
            using (FileSystemBase fs = fsh.Instantiate(GetStream().MakeSubStream()))
            {
                return fs.OpenFile(_directories, path);
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

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            DirectoryEntry de = GetEntry(path);
            if (de.subentries != null)
            {
                for (int i = 0; i != de.subentries.Length; i++)
                {
                    if (de.subentries[i].isFolder)
                    {
                        yield return de.subentries[i].GetFullPath();
                    }
                }
            }
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            DirectoryEntry de = GetEntry(path);
            if (de.subentries != null)
            {
                for (int i = 0; i != de.subentries.Length; i++)
                {
                    if (de.subentries[i].isFile)
                    {
                        yield return de.subentries[i].GetFullPath();
                    }
                }
            }
        }

        public bool Exists(string path)
        {
            DirectoryEntry de;
            return TryGetEntry(path, out de);
        }

        public string[] GetDirectories(string path)
        {
            DirectoryEntry de = GetEntry(path);
            if (de.subentries != null)
            {
                string[] files = new string[de.subentries.Length];
                for (int i = 0; i != de.subentries.Length; i++)
                {
                    files[i] = de.subentries[i].GetFullPath();
                }
                return files;
            }
            return new string[0];
        }

        public string[] GetFiles(string path)
        {
            DirectoryEntry de = GetEntry(path);
            if (de.subentries != null)
            {
                string[] files = new string[de.subentries.Length];
                for (int i = 0; i != de.subentries.Length; i++)
                {
                    files[i] = de.subentries[i].GetFullPath();
                }
                return files;
            }
            return new string[0];
        }

        public string GetParent(string path)
        {
            DirectoryEntry de = GetEntry(path);
            if (de.parent == null) return null;
            return de.parent.GetFullPath();
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
            DirectoryEntry current = null;
            string[] names = path.Split('/');
            bool ignoreLast = names[names.Length - 1] == "";

            for (int i = 0; i != names.Length + (ignoreLast ? -1 : 0); i++)
            {
                if(current == null)
                {
                    if(_entries.name == names[i])
                    {
                        current = _entries;
                    }
                    else
                    {
                        throw new DirectoryNotFoundException(path);
                    }
                }
                else if (current.subentries != null)
                {
                    for (int j = 0; j != current.subentries.Length; j++)
                    {
                        if(current.subentries[j].name == names[i])
                        {
                            current = current.subentries[j];
                            break;
                        }
                        if(j + 1 == current.subentries.Length)
                        {
                            throw new DirectoryNotFoundException(path);
                        }
                    }
                }
            }
            return current;
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

        public DirectoryEntry GetRoot()
        {
            DirectoryEntry de = this;
            while (de.parent != null) de = de.parent;
            return de;
        }

        public string GetFullPath()
        {
            string path = "";
            if(parent != null)
            {
                path += parent.GetFullPath();
            }
            path += name;
            if (isFolder) path += "/";
            return path;
        }

        public string GetDirectoryPath()
        {
            string path = "";
            if (parent != null)
            {
                path += parent.GetFullPath();
            }
            if (!isFile) path += name + "/";
            return path;
        }

        public string GetExtension()
        {
            return Path.GetExtension(name);
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
