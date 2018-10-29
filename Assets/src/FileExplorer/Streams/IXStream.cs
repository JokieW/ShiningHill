using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public unsafe class XStream : FileStream
{
    public XStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share) { }

    protected XStream _parentStream;
    protected List<XStream> _childStreams = new List<XStream>();

    public virtual XStream MakeSubStream() { return null; }
    public virtual XStream MakeSubStream(long start, long length) { return null; }
    public virtual int Read(byte* array, int offset, int count) { return 0; }

    public override void Close()
    {
        foreach(XStream x in _childStreams)
        {
            x.Close();
        }
        base.Close();
        if(_parentStream != null) _parentStream._childStreams.Remove(this);
        _childStreams = null;
    }
}
