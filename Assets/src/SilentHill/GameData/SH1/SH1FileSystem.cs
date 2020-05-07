using System;
using System.IO;
using System.Collections.Generic;
using SH.Core.Stream;

namespace SH1.DataFormat.SH1
{
    public class SH1FileSystem : IDisposable
    {
        XStream _stream;
        //DirectoryEntry _uniformRoot;
        string exeName;

        public SH1FileSystem() { }
        public SH1FileSystem(XStream stream)
        {
            _stream = stream;
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        /*public override void SetUniformDirectories(SourceBase source, ref DirectoryEntry self)
        {
            if (_uniformRoot == null)
            {
                //Get EXE name
                string filePath = self.GetFullPath();
                string cnfPath = null;

                DirectoryBrowser browser = source.GetBrowser();
                string parentPath = browser.GetParent(filePath);
                foreach (string p in browser.EnumerateFiles(parentPath))
                {
                    if(Path.GetFileName(p) == "SYSTEM.CNF")
                    {
                        cnfPath = p;
                        break;
                    }
                }
                if (cnfPath == null) throw new InvalidOperationException();

                using (StreamReader sr = new StreamReader(source.OpenFile(cnfPath)))
                {
                    string exeline = sr.ReadLine();
                    exeName = exeline.Substring(exeline.Length - 13, 11);
                }
                
                //Add folders
                VersionInfo versionInfo = VersionInfo.GetInfoForVersion(exeName);
                Dictionary<string, DirectoryEntry> _nameToFolders = new Dictionary<string, DirectoryEntry>(16) { { "", self } };
                Dictionary<DirectoryEntry, List<DirectoryEntry>> _folders = new Dictionary<DirectoryEntry, List<DirectoryEntry>>(16) { { self, new List<DirectoryEntry>() } };
                for(int i = 0; i != versionInfo.directories.Length; i++)
                {
                    string dirname = versionInfo.directories[i];
                    if (dirname != "")
                    {
                        DirectoryEntry de = new DirectoryEntry();
                        de.name = versionInfo.directories[i];
                        de.parent = self;
                        _folders.Add(de, new List<DirectoryEntry>());
                        _nameToFolders.Add(de.name, de);
                    }
                }

                //Add Files
                using (BinaryReader exeReader = new BinaryReader(source.OpenFile(parentPath + exeName)))
                {
                    exeReader.BaseStream.Position = versionInfo.fileInfoOffset;
                    for (int i = 0; i != versionInfo.fileCount; i++)
                    {
                        FileRecord fr = versionInfo.altRecordFormat ?
                            (FileRecord)new FileRecordAlt(exeReader.ReadUInt32(), exeReader.ReadUInt32(), exeReader.ReadUInt32()) :
                            (FileRecord)new FileRecordStd(exeReader.ReadUInt32(), exeReader.ReadUInt32(), exeReader.ReadUInt32());

                        DirectoryEntry de = new DirectoryEntry();
                        de.parent = _nameToFolders[fr.GetDirectory(versionInfo)];
                        _folders[de.parent].Add(de);
                        de.flags |= DirectoryEntry.DirFlags.IsFile;
                        de.fileAddress = (fr.startSector + versionInfo.baseLba) * 0x800;
                        de.fileLength = fr.GetSize();
                        de.name = fr.GetName() + fr.GetExtension(versionInfo);
                    }
                }

                //Append folders to self subentries
                {
                    List<DirectoryEntry> entries = new List<DirectoryEntry>();
                    int i = 0;
                    foreach (KeyValuePair<DirectoryEntry, List<DirectoryEntry>> kvp in _folders)
                    {
                        if(kvp.Key == self)
                        {
                            foreach(DirectoryEntry de in kvp.Value)
                            {
                                entries.Add(de);
                            }
                        }
                        else
                        {
                            kvp.Key.subentries = kvp.Value.ToArray();
                            entries.Add(kvp.Key);
                        }
                    }
                    if (self.subentries != null)
                    {
                        foreach (DirectoryEntry de in self.subentries)
                        {
                            entries.Add(de);
                        }
                    }
                    self.subentries = entries.ToArray();
                }

                self.specialFS = GetIdForType<SH1FileSystem>();
                _uniformRoot = self;
            }
            self = _uniformRoot;
        }

        public override XStream OpenFile(DirectoryEntry root, string path)
        {
            string[] paths = path.Split('/');
            int i = 0;
            FORE:
            foreach (DirectoryEntry de in root)
            {
                if (de.name == paths[i++])
                {
                    if (i == paths.Length)
                    {
                        return _stream.MakeSubStream(de.fileAddress, de.fileLength);
                    }
                    else if (de.specialFS != 0)
                    {
                        FileSystemHandler fsh = GetHandlerForID(de.specialFS);
                        using (FileSystemBase fs = fsh.Instantiate(_stream.MakeSubStream(de.fileAddress, de.fileLength)))
                        {
                            string substring = "";
                            for (int j = i; j != paths.Length; j++)
                            {
                                substring += "/" + path[j];
                            }
                            return fs.OpenFile(de, substring);
                        }
                    }
                    else
                    {
                        root = de;
                        goto FORE;
                    }
                }
            }
            throw new FileNotFoundException(path);
        }*/
        
        //Thanks to horrorx sources
        abstract class FileRecord
        {
            public abstract uint startSector { get; }
            public abstract uint chunkCount { get; }
            public abstract uint directoryIndex { get; }
            public abstract uint name0 { get; }
            public abstract uint name1 { get; }
            public abstract uint extensionIndex { get; }

            public abstract int GetSize();
            public abstract string GetExtension(VersionInfo info);
            public abstract string GetDirectory(VersionInfo info);
            public abstract string GetName();
        }

        class FileRecordAlt : FileRecord
        {
            uint a, b, c;
            public FileRecordAlt(uint a, uint b, uint c)
            {
                this.a = a; this.b = b; this.c = c;
            }

            public override uint startSector => a & 0x7FFFF;
            public override uint chunkCount => (a & 0x7FF80000) >> 19;
            public override uint directoryIndex => ((a & 0x80000000) >> 31) | ((b & 0x7) << 1);
            public override uint name0 => (b & 0xFFFFFFF8) >> 3;
            public override uint name1 => c & 0x7FFFF;
            public override uint extensionIndex => (c & 0x7F80000) >> 19;

            public override int GetSize() { return (int)chunkCount * 0x100; }
            public override string GetExtension(VersionInfo info) { return info.extensions[extensionIndex]; }
            public override string GetDirectory(VersionInfo info) { return info.directories[extensionIndex]; }
            public override string GetName()
            {
                ulong v = (name0 | (name1 << 29)) & 0xFFFFFFFFFFFFUL;
                List<char> name = new List<char>(8);
                for (int i = 0; i < 8 && v != 0; i++)
                {
                    name.Add((char)((v & 0x3F) + 0x20));
                    v >>= 6;
                }
                return new string(name.ToArray());
            }
        }

        class FileRecordStd : FileRecord
        {
            uint a, b, c;
            public FileRecordStd(uint a, uint b, uint c)
            {
                this.a = a; this.b = b; this.c = c;
            }

            public override uint startSector => a & 0x7FFFF;
            public override uint chunkCount => (a & 0xFFF80000) >> 19;
            public override uint directoryIndex => b & 0xF;
            public override uint name0 => (b & 0xFFFFFF0) >> 4;
            public override uint name1 => c & 0xFFFFFF;
            public override uint extensionIndex => (c & 0xFF000000) >> 24;

            public override int GetSize() { return (int)chunkCount * 0x100; }
            public override string GetExtension(VersionInfo info) { return info.extensions[extensionIndex]; }
            public override string GetDirectory(VersionInfo info) { return info.directories[extensionIndex]; }
            public override string GetName()
            {
                uint[] values = new uint[2] { name0, name1 };
                List<char> name = new List<char>(8);
                for (int j = 0; j < 2; j++)
                {
                    int v = (int)values[j];
                    for (int i = 0; i < 4 && v != 0; i++)
                    {
                        name.Add((char)((v & 0x3F) + 0x20));
                        v >>= 6;
                    }
                }
                return new string(name.ToArray());
            }
        }

        class VersionInfo
        {
            static readonly string[] c_defaultExtensions = new string[16] { ".TIM", ".VAB", ".BIN", ".DMS", ".ANM", ".PLM", ".IPD", ".ILM", ".TMD", ".DAT", ".KDT", ".CMP", ".TXT", "", "", "" };
            static readonly string[] c_demoExtensions = new string[16] { ".TIM", ".VAB", ".BIN", ".ANM", ".DMS", ".PLM", ".IPD", ".ILM", ".TMD", ".DAT", ".KDT", ".CMP", ".TXT", "", "", "" };
            static readonly string[] c_opm16Extensions = new string[16] { ".TIM", ".VAB", ".BIN", ".ANM", ".DMS", ".PLM", ".IPD", ".ILM", ".TMD", ".KDT", ".CMP", "", "", "", "", "" };

            static readonly string[] c_defaultDirectoryStruct = new string[16] { "1ST", "ANIM", "BG", "CHARA", "ITEM", "MISC", "SND", "TEST", "TIM", "VIN", "XA", "", "", "", "", "" };
            static readonly string[] c_demoDirectoryStruct = new string[16] { "1ST", "ANIM", "BG", "CHARA", "ITEM", "MISC", "SND", "TIM", "VIN", "XA", "", "", "", "", "", "" };
            static readonly string[] c_opm16DirectoryStruct = new string[16] { "1ST", "ANIM", "BG", "CHARA", "ITEM", "SND", "TEST", "TIM", "VIN", "XA", "", "", "", "", "", "" };
            static readonly string[] c_extendedDirectoryStruct = new string[16] { "1ST", "ANIM", "BG", "CHARA", "ITEM", "MISC", "SND", "TEST", "TIM", "VIN", "VIN2", "VIN3", "VIN4", "VIN5", "XA", "" };

            static readonly VersionInfo[] c_versionInfo = new VersionInfo[]
            {
                new VersionInfo("SLES_015.14", c_extendedDirectoryStruct, c_defaultExtensions, 0xB8FC, 2310, false, 0),
                new VersionInfo("SLUS_007.07", c_defaultDirectoryStruct, c_defaultExtensions, 0xB91C, 2074, false, 0),
                new VersionInfo("SLPM_861.92", c_defaultDirectoryStruct, c_defaultExtensions, 0xB91C, 2074, false, 0),
                new VersionInfo("SLED_017.35", c_demoDirectoryStruct, c_demoExtensions, 0xB648, 850, false, 0),
                new VersionInfo("SLED_021.86", c_extendedDirectoryStruct, c_defaultExtensions, 0xB8FC, 1015, false, 0),
                new VersionInfo("SLUS_900.50", c_demoDirectoryStruct, c_demoExtensions, 0xB648, 849, false, 0),
                new VersionInfo("SLPM_803.63", c_demoDirectoryStruct, c_demoExtensions, 0xB780, 843, true, 0),
            };

            public static VersionInfo GetInfoForVersion(string version)
            {
                for(int i = 0; i != c_versionInfo.Length; i++)
                {
                    if (c_versionInfo[i].exeName == version) return c_versionInfo[i];
                }
                return null;
            }

            public readonly string exeName;
            public readonly string[] directories;
            public readonly string[] extensions;
            public readonly uint fileInfoOffset;
            public readonly int fileCount;
            public readonly bool altRecordFormat;
            public readonly uint baseLba;

            public VersionInfo(string exe, string[] dirs, string[] exts, uint fileOffset, int fileNum, bool isAlt, uint startlba)
            {
                exeName = exe;
                directories = dirs;
                extensions = exts;
                fileInfoOffset = fileOffset;
                fileCount = fileNum;
                altRecordFormat = isAlt;
                baseLba = startlba;
            }
        }
    }
}