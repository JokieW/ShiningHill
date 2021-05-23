using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SH.Core.Stream
{
    public unsafe class CDROMStream : XStream
    {
        const long DEFAULT_CACHE_SIZE = 445;
        const long DATA_SIZE = 0x800;

        readonly string _path;
        readonly long _sectorSize;
        readonly long _firstFileSectors;
        readonly long _totalFileSectors;

        byte[] _cache;
        byte* _cachePtr;
        GCHandle _cacheHandle;

        long _cacheSectorCount;
        long _lastReadSector;
        long _firstCachedSector;
        long _dataPosition;
        long _cachePosition;

        public override long Length
        {
            get { return _totalFileSectors * DATA_SIZE; }
        }

        public override long Position
        {
            get { return _dataPosition; }
            set { _dataPosition = value; }
        }

        public static CDROMStream MakeFromCue(string cuePath)
        {
            if (!File.Exists(cuePath)) throw new FileNotFoundException("CDROM cue file not found", cuePath);
            if (Path.GetExtension(cuePath).ToLower() != ".cue") throw new FileNotFoundException("File is not a .cue", cuePath);

            string binPath;
            byte mode;
            long sectorSize;
            if (!ReadCueSheet(cuePath, out binPath, out mode, out sectorSize)) throw new InvalidOperationException("Invalid cue file " + cuePath);
            if (!File.Exists(binPath)) throw new FileNotFoundException("CDROM bin file not found", binPath);

            return new CDROMStream(binPath, sectorSize);
        }

        public override XStream MakeSubStream()
        {
            return new CDROMStream(_path, _sectorSize);
        }

        public override XStream MakeSubStream(long start, long length)
        {
            long len = length / DATA_SIZE;
            CDROMStream cds =  new CDROMStream(_path, _sectorSize, start / DATA_SIZE, len == 0 ? 1 : len);
            return cds;
        }

        private CDROMStream(string binPath, long sectorSize) 
            : this(binPath, sectorSize, 0L, new FileInfo(binPath).Length / sectorSize)
        {
        }

        private CDROMStream(string binPath, long sectorSize, long firstSector, long sectorCount) : base(binPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        {
            _path = binPath;
            _sectorSize = sectorSize;
            _firstFileSectors = firstSector;
            _totalFileSectors = sectorCount;

            ResetCacheSize();

            _lastReadSector = -1;
            _dataPosition = 0;
            CheckCache(true);
        }

        ~CDROMStream()
        {
            if (_cacheHandle.IsAllocated)
            {
                _cacheHandle.Free();
            }
        }

        private static bool ReadCueSheet(string path, out string bpath, out byte bmode, out long bsize)
        {
            // Garbage, works for now
            string[] lines = File.ReadAllLines(path);
            string binpath = null;
            int binmode = -1;
            long binsize = -1;

            foreach (string line in lines)
            {
                string l = line.Trim();
                if (l.StartsWith("FILE ") && l.EndsWith(" BINARY"))
                {
                    binpath = l.Substring(5, l.Length - 5 - 7).Replace("\"", "");
                    if (!File.Exists(binpath))
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

                if (l.StartsWith("TRACK 01 "))
                {
                    string[] ls = l.Substring(9).Split('/');
                    if (ls[0] == "MODE1") binmode = 1;
                    if (ls[0] == "MODE2") binmode = 2;
                    binsize = Convert.ToInt64(ls[1]);
                }

                if (binpath != null && binmode != -1 && binsize != -1)
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
            long curSector = _firstFileSectors + (_dataPosition / DATA_SIZE);
            if (curSector != _lastReadSector)
            {
                if (forceRecache || curSector < _firstCachedSector || curSector >= _firstCachedSector + _cacheSectorCount)
                {
                    long newCacheStart = curSector;
                    if (newCacheStart + _cacheSectorCount > (_firstFileSectors + _totalFileSectors))
                    {
                        newCacheStart = (_firstFileSectors + _totalFileSectors) - _cacheSectorCount;
                    }
                    base.Position = newCacheStart * _sectorSize;
                    base.Read(_cache, 0, _cache.Length);
                    _firstCachedSector = newCacheStart;
                }

                CDSector* cds = ((CDSector*)_cachePtr) + (curSector - _firstCachedSector);
                _cachePosition = ((long)cds - (long)_cachePtr) + cds->dataOffset + (_dataPosition % DATA_SIZE);

                _lastReadSector = curSector;
            }
        }

        public override int Read(byte[] array, int offset, int count)
        {
            fixed (byte* b = array)
            {
                return Read(b, offset, count);
            }
        }

        public override int Read(byte* array, int offset, int count)
        {
            if (_dataPosition + count > Length)
            {
                throw new EndOfStreamException("Read would go out of stream range");
            }
            long sizeLeft = count;
            while (sizeLeft != 0)
            {
                CheckCache();

                long remainingBytes = RemainingBytesInSectorData();
                long lumpSize = remainingBytes >= sizeLeft ? sizeLeft : remainingBytes;
                sizeLeft -= lumpSize;
                remainingBytes -= lumpSize;

                long longLength = lumpSize >> 3; // Divide by 8
                ulong* longcache = (ulong*)(_cachePtr + _cachePosition);
                ulong* longbuffer = (ulong*)array;
                for (int j = 0; j != longLength; j++)
                {
                    *longbuffer++ = *longcache++;
                }

                long remainderLength = lumpSize - (longLength << 3); //Multiply by 8
                byte* newbytecache = (byte*)longcache;
                for (int j = 0; j != remainderLength; j++)
                {
                    *array++ = *newbytecache++;
                }

                _cachePosition += lumpSize;
                _dataPosition += lumpSize;
            }
            return count;
        }

        private long RemainingBytesInSectorData()
        {
            return DATA_SIZE - (_dataPosition % DATA_SIZE);
        }

        public void SetCacheSize(long newsize)
        {
            if (_cacheHandle.IsAllocated)
            {
                _cacheHandle.Free();
            }
            _cacheSectorCount = TruncateCacheSize(newsize);
            _cache = new byte[_cacheSectorCount * _sectorSize];

            _cacheHandle = GCHandle.Alloc(_cache, GCHandleType.Pinned);
            _cachePtr = (byte*)_cacheHandle.AddrOfPinnedObject();
        }

        public void ResetCacheSize()
        {
            SetCacheSize(DEFAULT_CACHE_SIZE);
        }

        private long TruncateCacheSize(long size)
        {
            if (size > _totalFileSectors)
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

            private static int ReadHexAsDecimal(int v)
            {
                int r = 0;
                for (int e = 1; v != 0; v >>= 4, e *= 10)
                {
                    r += (v & 0x0F) * e;
                }
                return r;
            }
        }
    }
}
