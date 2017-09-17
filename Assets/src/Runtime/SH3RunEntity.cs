using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SH3RunEntity : MonoBehaviour
{
    public static Func<IntPtr> GetMemHandle;
    
	void Update ()
    {
        IntPtr handle = GetMemHandle();
        if (handle != IntPtr.Zero)
        {
            transform.position = Scribe.ReadVector3(handle, new IntPtr(0x008984E0)) * 0.002f;
            transform.rotation = Quaternion.Euler(Scribe.ReadVector3(handle, new IntPtr(0x008984F0)) * Mathf.Rad2Deg);
        }
    }
}
