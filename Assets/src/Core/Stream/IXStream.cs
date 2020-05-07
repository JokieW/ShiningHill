using System.IO;

namespace SH.Core.Stream
{
    public unsafe class XStream : FileStream
    {
        public XStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share) { }

        public virtual XStream MakeSubStream() { return null; }
        public virtual XStream MakeSubStream(long start, long length) { return null; }
        public virtual int Read(byte* array, int offset, int count) { return 0; }
    }
}
