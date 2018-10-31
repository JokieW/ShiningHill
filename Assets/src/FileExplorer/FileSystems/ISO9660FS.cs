using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ShiningHill
{
    //Thanks to https://wiki.osdev.org/ISO_9660
    public unsafe class ISO9660FS : FileSystemBase
    {
        XStream _stream;
        PrimaryVolumeDescriptor _pvd;
        DirectoryEntry _uniformRoot;
        DirectoryRecord _root;

        static string GetNameOfDirectoryRecord(DirectoryRecord* dir)
        {
            int nameLength = dir->fileIdentifierLength;
            sbyte* nameBuffer = stackalloc sbyte[nameLength];
            for (int i = 0; i != nameLength; i++)
            {
                nameBuffer[i] = (sbyte)*(&dir->fileIdentifier + i);
            }

            string name = *nameBuffer == 0 || *nameBuffer == 1 ? "" : new string(nameBuffer, 0, nameLength);
            return name.Replace(";1", "");
        }

        public ISO9660FS()
        {
        }

        public ISO9660FS(XStream stream)
        {
            _stream = stream;
            
            PrimaryVolumeDescriptor p;
            stream.Position = 0x8000;
            stream.Read((byte*)&p, 0, 2048);
            _root = *(DirectoryRecord*)&p.rootDirectory;
            _pvd = p;
        }

        public override FileSystemBase Instantiate(XStream stream)
        {
            return new ISO9660FS(stream);
        }

        public void Close()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
            base.Dispose();
        }

        public override XStream OpenFile(DirectoryEntry root, string path)
        {
            string[] paths = path.Split('/');
            int i = 1;
            FORE:
            foreach (DirectoryEntry de in root)
            {
                if (de.name == paths[i])
                {
                    i++;
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
        }

        public override void SetUniformDirectories(SourceBase source, ref DirectoryEntry self)
        {
            if (_uniformRoot == null)
            {
                DirectoryRecord root = _root;
                SetUniformDirectories(ref self, &root);
                self.specialFS = GetIdForType<ISO9660FS>();
                _uniformRoot = self;
            }
            self = _uniformRoot;
        }

        public void SetUniformDirectories(ref DirectoryEntry self, DirectoryRecord* recDir)
        {
            DirectoryEntry uniDir = self ?? new DirectoryEntry();
            if(uniDir.name == null) uniDir.name = GetNameOfDirectoryRecord(recDir);

            if ((recDir->fileFlags & FileFlags.IsSubdirectory) != 0)
            {
                _stream.Position = 0x800 * recDir->extentLocation;
                byte* extent = stackalloc byte[(int)recDir->extentLength];
                _stream.Read(extent, 0, (int)recDir->extentLength);

                List<DirectoryEntry> entries = new List<DirectoryEntry>();
                while (true)
                {
                    byte len = *extent;
                    if (len == 0) break;

                    DirectoryRecord* subdir = (DirectoryRecord*)extent;
                    if (subdir->fileIdentifier != 0 && subdir->fileIdentifier != 1)
                    {
                        DirectoryEntry subDir = null;
                        SetUniformDirectories(ref subDir, subdir);
                        subDir.parent = uniDir;
                        entries.Add(subDir);
                    }
                    extent += len;
                }
                uniDir.subentries = entries.ToArray();
            }
            else
            {
                uniDir.flags |= DirectoryEntry.DirFlags.IsFile;
                uniDir.fileAddress = 0x800 * recDir->extentLocation;
                uniDir.fileLength = recDir->extentLength;
            }
            self = uniDir;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 17)]
        public struct VolDateTime
        {
            public fixed byte year[4];                      //[strD] Year from 1 to 9999.
            public fixed byte month[2];                     //[strD] Month from 1 to 12. 
            public fixed byte day[2];                       //[strD] Day from 1 to 31. 
            public fixed byte hour[2];                      //[strD] Hour from 0 to 23. 
            public fixed byte minute[2];                    //[strD] Minute from 0 to 59.
            public fixed byte second[2];                    //[strD] Second from 0 to 59. 
            public fixed byte centisecond[2];               //[strD] Hundredths of a second from 0 to 99. 
            public byte gmt;                                //[strD] Time zone offset from GMT in 15 minute intervals, starting at interval -48 (west) and running up to interval 52 (east). So value 0 indicates interval -48 which equals GMT-12 hours, and value 100 indicates interval 52 which equals GMT+13 hours. 
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 7)]
        public struct DirDateTime
        {
            public byte year;                               //Number of years since 1900. 
            public byte month;                              //Month of the year from 1 to 12. 
            public byte day;                                //Day of the month from 1 to 31. 
            public byte hour;                               //Hour of the day from 0 to 23. 
            public byte minute;                             //Minute of the hour from 0 to 59. 
            public byte second;                             //Second of the minute from 0 to 59. 
            public byte gmt;                                //Offset from GMT in 15 minute intervals from -48 (West) to +52 (East). 
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct PathTableEntry
        {
            public byte length;                             //Length of Directory Identifier 
            public byte xaLength;                           //Extended Attribute Record Length 
            public uint extentLocation;                     //Location of Extent (LBA). This is in a different format depending on whether this is the L-Table or M-Table (see explanation above). 
            public ushort directoryNumberOfParents;         //Directory number of parent directory (an index in to the path table). This is the field that limits the table to 65536 records. 
            private byte directoryIdentifier;               //[strD] Directory Identifier (name). 
                                                            //Padding Field - contains a zero if the Length of Directory Identifier field is odd, not present otherwise. This means that each table entry will always start on an even byte number. 
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct DirectoryRecord
        {
            public byte length;                             //Length of Directory Record. 
            public byte xaLength;                           //Extended Attribute Record length. 

            public uint extentLocation;                     //Location of extent (LBA) in both-endian format. 
            private uint extentLocation_msb;

            public uint extentLength;                       //Data length (size of extent) in both-endian format. 
            private uint extentLength_msb;

            public DirDateTime dateTime;                    //Recording date and time.
            public FileFlags fileFlags;                     //File flags

            public byte fileUnitSize;                       //File unit size for files recorded in interleaved mode, zero otherwise. 
            public byte interleaveGapSize;                  //Interleave gap size for files recorded in interleaved mode, zero otherwise. 
            public ushort volumeSequenceNumber;             //Volume sequence number - the volume that this extent is recorded on, in 16 bit both-endian format. 
            private ushort volumeSequenceNumber_msb;
            public byte fileIdentifierLength;               //Length of file identifier (file name). This terminates with a ';' character followed by the file ID number in ASCII coded decimal ('1'). 
            public byte fileIdentifier;                     //[strD] File identifier.
                                                            //Padding field - zero if length of file identifier is even, otherwise, this field is not present. This means that a directory entry will always start on an even byte number. 
                                                            //System Use - The remaining bytes up to the maximum record size of 255 may be used for extensions of ISO 9660. The most common one is the System Use Share Protocol (SUSP) and its application, the Rock Ridge Interchange Protocol (RRIP). 
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 2048)]
        public struct PrimaryVolumeDescriptor
        {
            public VolumeDescriptorType typeCode;           //Always 0x01 for a Primary Volume Descriptor. 
            public fixed byte standardIdentifier[5];        //[strA] Always 'CD001'. 
            public byte version;                            //Always 0x01. 
            private byte unused1;                           //Always 0x00. 

            public fixed byte systemIdentifier[32];         //[strA] The name of the system that can act upon sectors 0x00-0x0F for the volume. 
            public fixed byte volumeIdentifier[32];         //[strD] Identification of this volume. 

            private ulong unused2;                          //All zeroes. 

            public uint volumeSpaceSize;                    //Number of Logical Blocks in which the volume is recorded. 
            private uint volumeSpaceSize_msb;

            private fixed byte unused3[32];                 //All zeroes. 

            public ushort volumeSetSize;                    //The size of the set in this logical volume (number of disks). 
            private ushort volumeSetSize_msb;
            public ushort volumeSequenceNumber;             //The number of this disk in the Volume Set. 
            private ushort volumeSequenceNumber_msb;
            public ushort logicalBlockSize;                 //The size in bytes of a logical block. NB: This means that a logical block on a CD could be something other than 2 KiB! 
            private ushort logicalBlockSize_msb;

            public uint pathTableSize;                      //The size in bytes of the path table. 
            private uint pathTableSize_msb;
            public uint locOfTypeLPathTable;                //LBA location of the path table. The path table pointed to contains only little-endian values. 
            public uint locOfOptTypeLPathTable;             //LBA location of the optional path table. The path table pointed to contains only little-endian values. Zero means that no optional path table exists. 
            private uint locOfTypeMPathTable;               //LBA location of the path table. The path table pointed to contains only big-endian values. 
            private uint locOfOptTypeMPathTable;            //LBA location of the optional path table. The path table pointed to contains only big-endian values. Zero means that no optional path table exists. 

            public byte rootDirectory;                      //Note that this is not an LBA address, it is the actual Directory Record, which contains a single byte Directory Identifier (0x00), hence the fixed 34 byte size. 
            private fixed byte rootDirectory_rest[33];

            public fixed byte volumeSetIdentifier[128];     //[strD] Identifier of the volume set of which this volume is a member.
            public fixed byte publisherIdentifier[128];     //[strA] The volume publisher. For extended publisher information, the first byte should be 0x5F, followed by the filename of a file in the root directory. If not specified, all bytes should be 0x20. 
            public fixed byte dataPreparerIdentifier[128];  //[strA] The identifier of the person(s) who prepared the data for this volume. For extended preparation information, the first byte should be 0x5F, followed by the filename of a file in the root directory. If not specified, all bytes should be 0x20. 
            public fixed byte applicationIdentifier[128];   //[strA] Identifies how the data are recorded on this volume. For extended information, the first byte should be 0x5F, followed by the filename of a file in the root directory. If not specified, all bytes should be 0x20. 

            public fixed byte copyrightFileIdentifier[38];  //[strD] Filename of a file in the root directory that contains copyright information for this volume set. If not specified, all bytes should be 0x20. 
            public fixed byte abstractFileIdentifier[36];   //[strD] Filename of a file in the root directory that contains abstract information for this volume set. If not specified, all bytes should be 0x20. 
            public fixed byte biblioFileIdentifier[37];     //[strD] Filename of a file in the root directory that contains bibliographic information for this volume set. If not specified, all bytes should be 0x20. 

            public VolDateTime volumeCreationTime;          //The date and time of when the volume was created. 
            public VolDateTime volumeModificationTime;      //The date and time of when the volume was modified. 
            public VolDateTime volumeExpirationTime;        //The date and time after which this volume is considered to be obsolete. If not specified, then the volume is never considered to be obsolete. 
            public VolDateTime volumeEffectiveTime;         //The date and time after which the volume may be used. If not specified, the volume may be used immediately. 

            public byte fileStructureVersion;               //The directory records and path table version (always 0x01). 

            private byte unused4;                           //Always 0x00.
            private fixed byte applicationUsed[512];        //Contents not defined by ISO 9660. 
            private fixed byte reserved[653];               //Reserved by ISO. 
        }

        public enum VolumeDescriptorType : byte
        {
            BootRecord = 0x00,                              //Boot Record 
            PrimaryVolumeDescriptor = 0x01,                 //Primary Volume Descriptor 
            SupplementaryVolumeDescriptor = 0x02,           //Supplementary Volume Descriptor
            VolumePartitionDescriptor = 0x03,               //Volume Partition Descriptor 
                                                            //4-254 Reserved 
            VolumeDescriptorSetTerminator = 0xFF            //Volume Descriptor Set Terminator
        }

        [Flags]
        public enum FileFlags : byte
        {
            IsHidden = 0x01,                                //If set, the existence of this file need not be made known to the user (basically a 'hidden' flag. 
            IsSubdirectory = 0x02,                          //If set, this record describes a directory (in other words, it is a subdirectory extent). 
            IsAssociated = 0x04,                            //If set, this file is an "Associated File". 
            HasAttributeRecords = 0x08,                     //If set, the extended attribute record contains information about the format of this file. 
            HasPermissions = 0x10,                          //If set, owner and group permissions are set in the extended attribute record. 
            Reserved1 = 0x20,                               //Reserved
            Reserved2 = 0x40,                               //Reserved
            NotFinalDirRecord = 0x80                        //If set, this is not the final directory record for this file (for files spanning several extents, for example files over 4GiB long.
        }
    }
}
