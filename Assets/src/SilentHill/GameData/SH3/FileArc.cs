using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

using UnityEditor;

using SH.Core;

namespace SH.GameData.SH3
{
    public class FileArc
    {
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
                        if(entry.type == 1) //Is Root
                        {
                            root = new Root();
                            root.entry = entry;
                            root.folders = new Root.Folder[entry.indexOrIndices];
                        }
                        else if(entry.type == 2) //Is Folder
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

        public static void UnpackArc(in Root.Folder folder, string arcPath, string to)
        {
            try
            {
                float filesExtracted = 0.0f;
                using (FileStream inputFile = new FileStream(arcPath, FileMode.Open, FileAccess.ReadWrite))
                using (BinaryReader reader = new BinaryReader(inputFile))
                {
                    reader.BaseStream.Position = 0L;
                    ArcHeader header = reader.ReadStruct<ArcHeader>();

                    for (int j = 0; j < folder.files.Length; j++)
                    {
                        Root.Folder.File file = folder.files[j];
                        if (EditorUtility.DisplayCancelableProgressBar("Extracting " + folder.entry.name + "...", file.entry.name, filesExtracted / folder.files.Length))
                        {
                            return;
                        }

                        string fullFilePath = to + file.entry.name;
                        {
                            string fullFilePathName = Path.GetDirectoryName(fullFilePath).Replace('\\', '/');
                            if (!Directory.Exists(fullFilePathName)) Directory.CreateDirectory(fullFilePathName);
                        }

                        reader.BaseStream.Position = Marshal.SizeOf<ArcHeader>() + (file.entry.indexOrIndices * Marshal.SizeOf<ArcEntry>());
                        ArcEntry entry = reader.ReadStruct<ArcEntry>();
                        reader.BaseStream.Position = entry.offset;

                        File.WriteAllBytes(fullFilePath, reader.ReadBytes((int)entry.length));
                        filesExtracted++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void PackArc(in Root.Folder folder, string rootFrom, string arcPath)
        {
            try
            {
                int topSize = Marshal.SizeOf<ArcHeader>() +
                    (Marshal.SizeOf<ArcEntry>() * folder.files.Length);

                using (FileStream stream = new FileStream(arcPath, FileMode.Create, FileAccess.ReadWrite))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    int paddingLength = 0;
                    for (int j = 0; j < folder.files.Length; j++)
                    {
                        ref readonly Root.Folder.File file = ref folder.files[j];
                        if (file.filesize > 0)
                        {
                            paddingLength += (((file.filesize - 1) / 0xFFFF) + 1) * 4;
                        }
                    }

                    ArcHeader header = new ArcHeader((uint)folder.files.Length, (uint)paddingLength);
                    writer.WriteStruct(header);

                    int currentOffset = topSize + paddingLength;
                    int currentPaddingOffset = topSize;
                    for (int j = 0; j < folder.files.Length; j++)
                    {
                        ref readonly Root.Folder.File file = ref folder.files[j];
                        ArcEntry entry = new ArcEntry((uint)currentOffset, (uint)currentPaddingOffset, (uint)file.filesize);
                        writer.WriteStruct(entry);

                        if (file.filesize > 0)
                        {
                            currentPaddingOffset += (((file.filesize - 1) / 0xFFFF) + 1) * 4;
                            currentOffset += file.filesize;
                        }
                    }

                    writer.BaseStream.Position += paddingLength;
                    float filesCompressed = 0.0f;
                    for (int j = 0; j < folder.files.Length; j++)
                    {
                        ref readonly Root.Folder.File file = ref folder.files[j];
                        if (EditorUtility.DisplayCancelableProgressBar("Compressing Files...", file.entry.name, filesCompressed / (float)folder.files.Length)) return;
                        byte[] fileBytes = null;
                        using (FileStream fileStream = new FileStream(rootFrom + file.entry.name, FileMode.Open, FileAccess.ReadWrite))
                        using (BinaryReader reader = new BinaryReader(fileStream))
                        {
                            fileBytes = reader.ReadBytes((int)reader.BaseStream.Length);
                        }
                        writer.Write(fileBytes);
                        filesCompressed++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
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
                for(int i = 0; i < folders.Length; i++)
                {
                    ref readonly Folder f = ref folders[i];
                    if(f.entry.name == name)
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
        public struct ArcHeader
        {
            public uint magicbytes;
            public uint fileCount;
            public uint paddingSize;
            public uint field_0C;

            public ArcHeader(uint fileCount, uint paddingSize)
            {
                this.magicbytes = 0x20030507;
                this.fileCount = fileCount;
                this.paddingSize = paddingSize;
                this.field_0C = 0x0;
            }
        }

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
        public struct ArcEntry
        {
            public uint offset;
            public uint paddingOffset; //use unknown
            public uint length;
            public uint length2; //Not quite sure why there is two length

            public ArcEntry(uint offset, uint paddingOffset, uint length)
            {
                this.offset = offset;
                this.paddingOffset = paddingOffset;
                this.length = length;
                this.length2 = length;
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
    }
}
