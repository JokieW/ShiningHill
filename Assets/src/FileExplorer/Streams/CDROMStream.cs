using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public unsafe class CDROMStream: System.IDisposable
{
    const long DEFAULT_CACHE_SIZE = 445;
    const long DATA_SIZE = 0x800;

    readonly long _sectorSize;
    readonly long _totalFileSectors;

    FileStream _fs;
    byte[] _cache;
    long _cacheSectorCount;
    long _lastReadSector;
    long _firstCachedSector;
    long _dataPosition;
    long _cachePosition;

    public long length
    {
        get { return _totalFileSectors * DATA_SIZE; }
    }

    public long position
    {
        get { return _dataPosition; }
        set { _dataPosition = value; }
    }

    public CDROMStream(string cuePath)
    {
        if (!File.Exists(cuePath)) throw new FileNotFoundException("CDROM cue file not found", cuePath);
        if (Path.GetExtension(cuePath).ToLower() != ".cue") throw new FileNotFoundException("File is not a .cue", cuePath);

        string binPath;
        byte mode;
        if (!ReadCueSheet(cuePath, out binPath, out mode, out _sectorSize)) throw new InvalidOperationException("Invalid cue file " + cuePath);
        if (!File.Exists(binPath)) throw new FileNotFoundException("CDROM bin file not found", binPath);
        
        _totalFileSectors = new FileInfo(binPath).Length / _sectorSize;

        ResetCacheSize();
        _fs = new FileStream(binPath, FileMode.Open, FileAccess.Read, FileShare.Read, _cache.Length, FileOptions.None);

        _lastReadSector = -1;
        _dataPosition = 0;
        CheckCache(true);
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_fs != null)
        {
            _fs.Close();
            _fs = null;
        }
    }

    private bool ReadCueSheet(string path, out string bpath, out byte bmode, out long bsize)
    {
        // Garbage, works for now
        string[] lines = File.ReadAllLines(path);
        string binpath = null;
        int binmode = -1;
        long binsize = -1;

        foreach(string line in lines)
        {
            string l = line.Trim();
            if (l.StartsWith("FILE ") && l.EndsWith(" BINARY"))
            {
                binpath = l.Substring(5, l.Length - 5 - 7).Replace("\"", "");
                if(!File.Exists(binpath))
                {
                    string testPath = Path.GetDirectoryName(path).Replace("\\", "/") + "/" + binpath;
                    if (File.Exists(testPath))
                    {
                        binpath = testPath;
                    }
                }

                binmode = -1;
                binsize = -1;
            }

            if(l.StartsWith("TRACK 01 "))
            {
                string[] ls = l.Substring(9).Split('/');
                if (ls[0] == "MODE1") binmode = 1;
                if (ls[0] == "MODE2") binmode = 2;
                binsize = Convert.ToInt64(ls[1]);
            }

            if(binpath != null && binmode != -1 && binsize != -1)
            {
                bpath = binpath;
                bmode = (byte)binmode;
                bsize = binsize;
                return true;
            }
        }
        bpath = null;
        bmode = 0;
        bsize = 0;
        return false;
    }
    
    private void CheckCache(bool forceRecache = false)
    {
        long curSector = _dataPosition / DATA_SIZE;
        if (curSector != _lastReadSector)
        { 
            if (forceRecache || curSector < _firstCachedSector || curSector >= _firstCachedSector + _cacheSectorCount)
            {
                long newCacheStart = curSector;
                if(newCacheStart + _cacheSectorCount > _totalFileSectors)
                {
                    newCacheStart = _totalFileSectors - _cacheSectorCount;
                }
                _fs.Position = newCacheStart * _sectorSize;
                _fs.Read(_cache, 0, _cache.Length);
                _firstCachedSector = newCacheStart;
            }
            
            fixed (byte* b = _cache)
            {
                long init = (long)b;
                CDSector* cds = ((CDSector*)b) + (curSector - _firstCachedSector);
                _cachePosition = ((long)cds - init) + cds->dataOffset + (_dataPosition % DATA_SIZE);
            }
            _lastReadSector = curSector;
        }
    }

    private void SeekToSector(long sector)
    {
        _dataPosition = sector * DATA_SIZE;
    }

    public void Read(byte[] buffer, long size)
    {
        fixed(byte* b = buffer)
        {
            Read(b, size);
        }
    }

    public void Read(byte* buffer, long size)
    {
        if (_dataPosition + size > length) throw new EndOfStreamException("Read would go out of stream range");
        long sizeLeft = size;
        while (sizeLeft != 0)
        {
            CheckCache();
            
            long remainingBytes = RemainingBytesInSectorData();
            long lumpSize = remainingBytes >= sizeLeft ? sizeLeft : remainingBytes;
            sizeLeft -= lumpSize;
            remainingBytes -= lumpSize;
            
            fixed (byte* c = _cache)
            {
                byte* cc = c + _cachePosition;
                long longLength = lumpSize >> 3; // Divide by 8
                long remainderLength = lumpSize - (longLength << 3); //Multiply by 8
                for (int j = 0; j != remainderLength; j++)
                {
                    *buffer++ = *cc++;
                }

                ulong* ccc = (ulong*)cc;
                ulong* buf = (ulong*)buffer;
                for (int j = 0; j != longLength; j++)
                {
                    *buf++ = *ccc++;
                }
            }
            _cachePosition += lumpSize;
            _dataPosition += lumpSize;
        }
    }

    public static void LongCopy(byte* src, byte *dst, long length)
    {

    }
    
    private long RemainingBytesInSectorData()
    {
        return DATA_SIZE - (_dataPosition % DATA_SIZE);
    }

    public void SetCacheSize(long newsize)
    {
        _cacheSectorCount = TruncateCacheSize(newsize);
        _cache = new byte[_cacheSectorCount * _sectorSize];
    }

    public void ResetCacheSize()
    {
        _cacheSectorCount = TruncateCacheSize(DEFAULT_CACHE_SIZE);
        _cache = new byte[_cacheSectorCount * _sectorSize];
    }

    private long TruncateCacheSize(long size)
    {
        if(size > _totalFileSectors)
        {
            return _totalFileSectors;
        }
        return size;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1, Size = 2352)]
    public struct CDSector
    {
        const int DATA_OFFSET_MODE1 = 16;
        const int DATA_OFFSET_MODE2 = 24;
        [FieldOffset(0)]
        public fixed byte sync[12];
        [FieldOffset(12)]
        public byte rawHour;
        [FieldOffset(13)]
        public byte rawMinute;
        [FieldOffset(14)]
        public byte rawSector;
        [FieldOffset(15)]
        public byte mode;

        //Mode 1
        [FieldOffset(DATA_OFFSET_MODE1)]
        public byte data_mode1;
        [FieldOffset(2064)]
        public uint errorDetection_mode1;
        [FieldOffset(2068)]
        public byte reserved_mode1;

        //Mode 2
        [FieldOffset(16)]
        public fixed byte subheader_mode2[8];
        [FieldOffset(DATA_OFFSET_MODE2)]
        public byte data_mode2;
        [FieldOffset(2072)]
        public uint errorDetection_mode2;

        [FieldOffset(2076)]
        public fixed byte errorCorrection[276];

        public long linearSector
        {
            get { return (ReadHexAsDecimal(rawHour) * 60 * 75) + ((ReadHexAsDecimal(rawMinute) - 2) * 75) + ReadHexAsDecimal(rawSector); }
        }

        public long dataOffset
        {
            get { return mode == 1 ? DATA_OFFSET_MODE1 : DATA_OFFSET_MODE2; }
        }

        public static byte* GetData(CDSector* sector)
        {
            return (*sector).mode == 1 ? &(*sector).data_mode1 : &(*sector).data_mode2;
        }

        private static byte ReadHexAsDecimal(byte v)
        {
            byte r = 0;
            for (int i = 0, b = 0, e = 1; i != 8; i++, b++)
            {
                if (b == 4) { b = 0; e *= 10; }
                if ((v & 0x01) != 0) { r += (byte)((1 << b) * e); }
                v >>= 1;
            }
            return r;
        }

    }
}
