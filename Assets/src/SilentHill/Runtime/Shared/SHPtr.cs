using System;

using UnityEngine;

namespace SH.Runtime.Shared
{
    [Serializable]
    public struct SHPtr
    {
        [SerializeField]
        private long _ptr;

        public SHPtr(long address)
        {
            _ptr = address;
        }

        public static implicit operator SHPtr(int thpr)
        {
            return new SHPtr(thpr);
        }

        public static implicit operator SHPtr(long thpr)
        {
            return new SHPtr(thpr);
        }

        public static implicit operator SHPtr(IntPtr thpr)
        {
            return new SHPtr(thpr.ToInt64());
        }

        public static implicit operator IntPtr(SHPtr thpr)
        {
            return new IntPtr(thpr._ptr);
        }
    }
}
