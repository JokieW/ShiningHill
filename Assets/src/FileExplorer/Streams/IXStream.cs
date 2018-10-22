using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public unsafe interface IXStream : System.IDisposable
{
    long Position { get; set; }
    long Length { get; }

    int Read(byte[] array, int offset, int count);
    int Read(byte* array, int offset, int count);
}
