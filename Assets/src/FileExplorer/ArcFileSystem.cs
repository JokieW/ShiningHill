using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using System.Runtime.InteropServices;
using System.Text;

namespace ShiningHill
{
    public class ArcFileSystem
    {
        public static void DecompressArcs(string from = "C:/Silent Hill 3/", string to = "C:/Silent Hill 3/")
        {
            Root root = default;
            float totalFiles = 0.0f;
            using (MemoryStream stream = new MemoryStream())
            {
                using (FileStream file = new FileStream(from + "data/" + "arc.arc", FileMode.Open, FileAccess.Read))
                using (GZipStream gzip = new GZipStream(file, CompressionMode.Decompress))
                {
                    gzip.CopyTo(stream);
                }

                /*using (FileStream file = new FileStream(to + "arcuncompressed", FileMode.Create, FileAccess.Write))
                {
                    stream.Position = 0L;
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    file.Write(buffer, 0, buffer.Length);
                }*/

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    reader.BaseStream.Position = 0L;
                    ArcArcHeader header = reader.ReadStruct<ArcArcHeader>();
                    int folderCount = 0;

                    StringBuilder sb = new StringBuilder();
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        ArcArcEntry entry = new ArcArcEntry(reader);
                        switch (entry.type)
                        {
                            case 1:
                                root = new Root();
                                root.entry = entry;
                                root.folders = new Root.Folder[entry.indexOrIndices];
                                sb.AppendLine("- Root");
                                break;

                            case 2:
                                Root.Folder folder = new Root.Folder();
                                folder.entry = entry;
                                folder.files = new ArcArcEntry[entry.indexOrIndices];
                                root.folders[folderCount++] = folder;
                                sb.AppendLine("  - "+ folder.entry.name);
                                break;

                            case 3:
                                root.folders[entry.indexOfParent].files[entry.indexOrIndices] = entry;
                                totalFiles++;
                                sb.AppendLine("    - " + entry.name);
                                break;
                        }
                    }
                    File.WriteAllText(to + "extractedfiles.txt", sb.ToString());
                }
            }

            string toArc = to + "arc/";
            if (Directory.Exists(toArc)) Directory.Delete(toArc, true);

            try
            {
                float filesExtracted = 0.0f;
                for (int i = 0; i < root.folders.Length; i++)
                {
                    Root.Folder folder = root.folders[i];
                    string arcPath = from + "data/" + folder.entry.name + ".arc";
                    using (FileStream inputFile = new FileStream(arcPath, FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(inputFile))
                    {
                        reader.BaseStream.Position = 0L;
                        ArcHeader header = reader.ReadStruct<ArcHeader>();

                        for (int j = 0; j < folder.files.Length; j++)
                        {
                            ArcArcEntry file = folder.files[j];
                            if (EditorUtility.DisplayCancelableProgressBar("Extracting Files...", file.name, filesExtracted / totalFiles))
                            {
                                return;
                            }

                            string fullFilePath = toArc + folder.entry.name + "/" + file.name;
                            {
                                string fullFilePathName = Path.GetDirectoryName(fullFilePath).Replace('\\', '/');
                                if (!Directory.Exists(fullFilePathName)) Directory.CreateDirectory(fullFilePathName);
                            }

                            reader.BaseStream.Position = Marshal.SizeOf<ArcHeader>() + (file.indexOrIndices * Marshal.SizeOf<ArcEntry>());
                            ArcEntry entry = reader.ReadStruct<ArcEntry>();
                            reader.BaseStream.Position = entry.offset;

                            File.WriteAllBytes(fullFilePath, reader.ReadBytes((int)entry.length));
                            filesExtracted++;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static readonly string rootName = new string(new char[] { (char)0xEC, (char)0x0A });

        public static void CompressArcs(string from = "C:/Silent Hill 3/", string to = "C:/Silent Hill 3/")
        {
            int arcArcUncompressedSize = Marshal.SizeOf<ArcArcHeader>();
            Root root = new Root();
            float totalFiles = 0.0f;
            Dictionary<string, int> totalFileSizes = new Dictionary<string, int>();
            {
                string rootPath = from + "arc/";

                string[] rootFolders = Directory.GetDirectories(rootPath);
                root.entry = new ArcArcEntry(1, (short)rootFolders.Length, 0, rootName);
                root.folders = new Root.Folder[rootFolders.Length];
                arcArcUncompressedSize += root.entry.entryLength;

                for (short i = 0; i < rootFolders.Length; i++)
                {
                    string currentFolderPath = rootFolders[i].Replace('\\', '/');
                    string folderShortName = currentFolderPath.Substring(currentFolderPath.LastIndexOf("/") + 1);
                    currentFolderPath += '/';
                    string[] files = Directory.GetFiles(currentFolderPath, "*", SearchOption.AllDirectories);

                    Root.Folder folder = new Root.Folder();
                    folder.entry = new ArcArcEntry(2, (short)files.Length, 0, folderShortName);
                    folder.files = new ArcArcEntry[files.Length];
                    arcArcUncompressedSize += folder.entry.entryLength;
                    root.folders[i] = folder;

                    for (short j = 0; j < files.Length; j++)
                    {
                        string filePath = files[j].Replace('\\', '/');
                        string fileName = filePath.Substring(currentFolderPath.Length);
                        ArcArcEntry file = new ArcArcEntry(3, j, i, fileName);
                        int size = (int)new FileInfo(filePath).Length;
                        totalFileSizes.Add(file.name, size);
                        folder.files[j] = file;
                        arcArcUncompressedSize += file.entryLength;
                        totalFiles++;
                    }
                }
            }

            byte[] arcArcUncompressed = new byte[arcArcUncompressedSize];
            StringBuilder sb = new StringBuilder();
            using (MemoryStream memory = new MemoryStream(arcArcUncompressed))
            using (BinaryWriter writer = new BinaryWriter(memory))
            {
                writer.BaseStream.Position = 0L;
                writer.WriteStruct(ArcArcHeader.Make());
                root.entry.Write(writer);
                sb.AppendLine("- Root");
                for (short i = 0; i < root.folders.Length; i++)
                {
                    Root.Folder folder = root.folders[i];
                    folder.entry.Write(writer);
                    sb.AppendLine("  - " + folder.entry.name);
                    for (short j = 0; j < folder.files.Length; j++)
                    {
                        ArcArcEntry entry = folder.files[j];
                        entry.Write(writer);
                        sb.AppendLine("    - " + entry.name);
                    }
                }
            }
            File.WriteAllText(to + "compressedfiles.txt", sb.ToString());


            string toData = to + "data/";
            if (!Directory.Exists(toData)) Directory.CreateDirectory(toData);
            using (FileStream file = new FileStream(toData + "arc.arc", FileMode.Create, FileAccess.Write))
            using (GZipStream gzip = new GZipStream(file, CompressionMode.Compress))
            {
                gzip.Write(arcArcUncompressed, 0, arcArcUncompressed.Length);
            }
            /*using (FileStream file = new FileStream(toData + "arcuncompressed", FileMode.Create, FileAccess.Write))
            {
                file.Write(arcArcUncompressed, 0, arcArcUncompressed.Length);
            }*/

            try
            {
                float filesCompressed = 0.0f;
                for (int i = 0; i < root.folders.Length; i++)
                {
                    Root.Folder folder = root.folders[i];
                    int topSize = Marshal.SizeOf<ArcHeader>() +
                        (Marshal.SizeOf<ArcEntry>() * folder.files.Length);

                    using (FileStream stream = new FileStream(toData + folder.entry.name + ".arc", FileMode.Create, FileAccess.Write))
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        int paddingLength = 0;
                        for (int j = 0; j < folder.files.Length; j++)
                        {
                            ArcArcEntry file = folder.files[j];
                            int fileSize = totalFileSizes[file.name];
                            if (fileSize > 0)
                            {
                                paddingLength += (((fileSize - 1) / 0xFFFF) + 1) * 4;
                            }
                        }

                        ArcHeader header = new ArcHeader((uint)folder.files.Length, (uint)paddingLength);
                        writer.WriteStruct(header);

                        int currentOffset = topSize + paddingLength;
                        int currentPaddingOffset = topSize;
                        for (int j = 0; j < folder.files.Length; j++)
                        {
                            ArcArcEntry file = folder.files[j];
                            int fileSize = totalFileSizes[file.name];
                            ArcEntry entry = new ArcEntry((uint)currentOffset, (uint)currentPaddingOffset, (uint)fileSize);
                            writer.WriteStruct(entry);

                            if (fileSize > 0)
                            {
                                currentPaddingOffset += (((fileSize - 1) / 0xFFFF) + 1) * 4;
                                currentOffset += fileSize;
                            }
                        }

                        writer.BaseStream.Position += paddingLength;
                        string folderFromPath = from + "arc/" + folder.entry.name + "/";
                        for (int j = 0; j < folder.files.Length; j++)
                        {
                            ArcArcEntry file = folder.files[j];
                            if (EditorUtility.DisplayCancelableProgressBar("Compressing Files...", folder.entry.name + "/" + file.name, filesCompressed / totalFiles))
                            {
                                return;
                            }
                            byte[] fileBytes = null;
                            using (FileStream fileStream = new FileStream(folderFromPath + file.name, FileMode.Open, FileAccess.Read))
                            using (BinaryReader reader = new BinaryReader(fileStream))
                            {
                                fileBytes = reader.ReadBytes((int)reader.BaseStream.Length);
                            }
                            writer.Write(fileBytes);
                            filesCompressed++;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }



        public struct Root
        {
            public ArcArcEntry entry;
            public Folder[] folders;

            public struct Folder
            {
                public ArcArcEntry entry;
                public ArcArcEntry[] files;
            }
        }

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