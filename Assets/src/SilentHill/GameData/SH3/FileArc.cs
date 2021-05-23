using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEditor;

using SH.Core;

namespace SH.GameData.SH3
{
    public class FileArc
    {
        public static void UnpackArc(in FileArcArc.Root.Folder folder, string arcPath, string to)
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
                        FileArcArc.Root.Folder.File file = folder.files[j];
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

        public static void PackArc(in FileArcArc.Root.Folder folder, string rootFrom, string arcPath)
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
                        ref readonly FileArcArc.Root.Folder.File file = ref folder.files[j];
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
                        ref readonly FileArcArc.Root.Folder.File file = ref folder.files[j];
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
                        ref readonly FileArcArc.Root.Folder.File file = ref folder.files[j];
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
    }
}
