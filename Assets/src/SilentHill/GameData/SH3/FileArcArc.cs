using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

using SH.Core;

namespace SH.GameData.SH3
{
    public class FileArcArc
    {
        [Serializable]
        public struct ArcArcHeader
        {
            public uint magicbytes;
            public uint field_04;
            public uint field_08;
            public uint field_0C;

            public static ArcArcHeader Make()
            {
                return new ArcArcHeader()
                {
                    magicbytes = 0x20030417,
                    field_04 = 0,
                    field_08 = 0,
                    field_0C = 0
                };
            }
        }

        [Serializable]
        public struct Root
        {
            public ArcArcEntry entry;
            public Folder[] folders;

            public int GetUncompressedSize()
            {
                int arcArcUncompressedSize = Marshal.SizeOf<ArcArcHeader>();
                arcArcUncompressedSize += entry.entryLength;

                for (short i = 0; i < folders.Length; i++)
                {
                    ref readonly Root.Folder folder = ref folders[i];
                    arcArcUncompressedSize += folder.entry.entryLength;

                    for (short j = 0; j < folder.files.Length; j++)
                    {
                        arcArcUncompressedSize += folder.files[j].entry.entryLength;
                    }
                }
                return arcArcUncompressedSize;
            }

            public bool GetFolder(string name, out Folder folder)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    ref readonly Folder f = ref folders[i];
                    if (f.entry.name == name)
                    {
                        folder = f;
                        return true;
                    }
                }
                folder = default;
                return false;
            }

            [Serializable]
            public struct Folder
            {
                public ArcArcEntry entry;
                public File[] files;

                [Serializable]
                public struct File
                {
                    public ArcArcEntry entry;
                    public int filesize;
                }
            }
        }

        [Serializable]
        public struct ArcArcEntry
        {
            public short type; //1 arcarc, 2 arc, 3 file
            public short entryLength;
            public short indexOrIndices; //index in parent if file, indices count if folder
            public short indexOfParent;
            public string name;

            public ArcArcEntry(short type, short indexOrIndices, short indexOfParent, string name)
            {
                this.type = type;
                this.indexOrIndices = indexOrIndices;
                this.indexOfParent = indexOfParent;
                this.name = name;
                this.entryLength = (short)(0x09 + name.Length);
                if (name.Length % 2 == 0)
                {
                    this.entryLength++;
                }
            }

            public ArcArcEntry(BinaryReader reader)
            {
                type = reader.ReadInt16();
                entryLength = reader.ReadInt16();
                indexOrIndices = reader.ReadInt16();
                indexOfParent = reader.ReadInt16();
                name = reader.ReadNullTerminatedString();
                if (name.Length % 2 == 0)
                {
                    reader.SkipByte();
                }
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(type);
                writer.Write(entryLength);
                writer.Write(indexOrIndices);
                writer.Write(indexOfParent);
                writer.WriteNullTerminatedString(name);
                if (name.Length % 2 == 0)
                {
                    writer.Write((byte)0x00);
                }
            }
        }

        public static void UnpackArcArc(string arcarcPath, out Root root)
        {
            root = default;
            using (MemoryStream stream = new MemoryStream())
            {
                using (FileStream file = new FileStream(arcarcPath, FileMode.Open, FileAccess.ReadWrite))
                using (GZipStream gzip = new GZipStream(file, CompressionMode.Decompress))
                {
                    gzip.CopyTo(stream);
                }

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    reader.BaseStream.Position = 0L;
                    ArcArcHeader header = reader.ReadStruct<ArcArcHeader>();
                    int folderCount = 0;

                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        ArcArcEntry entry = new ArcArcEntry(reader);
                        if (entry.type == 1) //Is Root
                        {
                            root = new Root();
                            root.entry = entry;
                            root.folders = new Root.Folder[entry.indexOrIndices];
                        }
                        else if (entry.type == 2) //Is Folder
                        {
                            Root.Folder folder = new Root.Folder();
                            folder.entry = entry;
                            folder.files = new Root.Folder.File[entry.indexOrIndices];
                            root.folders[folderCount++] = folder;
                        }
                        else if (entry.type == 3) //Is File
                        {
                            Root.Folder.File file = new Root.Folder.File();
                            file.entry = entry;
                            file.filesize = 0;
                            ref readonly Root.Folder folder = ref root.folders[entry.indexOfParent];
                            folder.files[entry.indexOrIndices] = file;
                        }
                    }
                }
            }
        }

        public static void PackArcArc(string pathToArcArc, in Root root)
        {
            byte[] arcArcUncompressed = new byte[root.GetUncompressedSize()];
            using (MemoryStream memory = new MemoryStream(arcArcUncompressed))
            using (BinaryWriter writer = new BinaryWriter(memory))
            {
                writer.BaseStream.Position = 0L;
                writer.WriteStruct(ArcArcHeader.Make());
                root.entry.Write(writer);
                for (short i = 0; i < root.folders.Length; i++)
                {
                    ref readonly Root.Folder folder = ref root.folders[i];
                    folder.entry.Write(writer);
                    for (short j = 0; j < folder.files.Length; j++)
                    {
                        ref readonly Root.Folder.File file = ref folder.files[j];
                        file.entry.Write(writer);
                    }
                }
            }

            using (FileStream file = new FileStream(pathToArcArc, FileMode.Create, FileAccess.ReadWrite))
            using (GZipStream gzip = new GZipStream(file, CompressionMode.Compress))
            {
                gzip.Write(arcArcUncompressed, 0, arcArcUncompressed.Length);
            }
        }


        static readonly string rootName = new string(new char[] { (char)0xEC, (char)0x0A });
        public static void MakeArcArcInfo(string rootPath, (string, string[]) map, out Root.Folder folder)
        {
            Root root;
            MakeArcArcInfo(rootPath, new (string, string[])[1] { map }, out root);
            folder = root.folders[0];
        }

        public static void MakeArcArcInfo(string rootPath, (string, string[])[] map, out Root root)
        {
            root = new Root();
            {
                root.entry = new ArcArcEntry(1, (short)map.Length, 0, rootName);
                root.folders = new Root.Folder[map.Length];

                for (short i = 0; i < map.Length; i++)
                {
                    (string, string[]) folderMap = map[i];
                    Root.Folder folder = new Root.Folder();
                    folder.entry = new ArcArcEntry(2, (short)folderMap.Item2.Length, 0, folderMap.Item1);
                    folder.files = new Root.Folder.File[folderMap.Item2.Length];
                    root.folders[i] = folder;

                    for (short j = 0; j < folderMap.Item2.Length; j++)
                    {
                        string fileName = folderMap.Item2[j];
                        ArcArcEntry entry = new ArcArcEntry(3, j, i, fileName);
                        int size = (int)new FileInfo(rootPath + fileName).Length;
                        folder.files[j] = new Root.Folder.File() { entry = entry, filesize = size };
                    }
                }
            }
        }
    }
}
