using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace ShiningHill
{
    [Serializable]
    public struct VirtualAddress
    {
        [SerializeField]
        long address;

        public VirtualAddress(long virtualaddress)
        {
            this.address = virtualaddress;
        }

        public long raw
        {
            get { return address >= 0x00400000L ? address - 0x00400000L : address; }
        }

        public long @virtual
        {
            get { return address; }
        }

        public static implicit operator VirtualAddress(int thpr)
        {
            return new VirtualAddress(thpr);
        }

        public static implicit operator VirtualAddress(long thpr)
        {
            return new VirtualAddress(thpr);
        }

        public static implicit operator VirtualAddress(IntPtr thpr)
        {
            return new VirtualAddress(thpr.ToInt64());
        }
    }

    public unsafe static class SH3_ExeData
    {
        #region Region
        [Serializable]
        public class RegionData
        {
            public VirtualAddress address;
            public string name;
            public List<EventInfo> events;
            public List<EventMarker> markers;
        }
        #endregion

        #region Events
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct EventInfo
        {
            [SerializeField]
            public int eventNumber;
            [SerializeField]
            public int int0;
            [SerializeField]
            public int int1;
            [SerializeField]
            public int int2;
            [SerializeField]
            public int int3;
            [SerializeField]
            public int int4;


            public EventInfo(BinaryReader reader, int number)
            {
                eventNumber = number;
                int0 = reader.ReadInt32();
                int1 = reader.ReadInt32();
                int2 = reader.ReadInt32();
                int3 = reader.ReadInt32();
                int4 = reader.ReadInt32();
            }

            public bool IsNull()
            {
                return int0 == 0 && int1 == 0 && int2 == 0 && int3 == 0 && int4 == 0;
            }

            public short GetLocationOffset()
            {
                fixed (int* ptr = &int0)
                    return eventLocationMarker.Get(ptr);
            }

            public short GetDestinationOffset()
            {
                fixed(int* ptr = &int0)
                    return eventDestinationMarker.Get(ptr);
            }
        }
        #endregion

        #region Markers
        [Serializable]
        public struct EventMarker
        {
            [SerializeField]
            public short offset;
            [SerializeField]
            public byte type;
            [SerializeField]
            public float x;
            [SerializeField]
            public float y;
            [SerializeField]
            public float z;
            [SerializeField]
            public float offsetA;
            [SerializeField]
            public float offsetB;

            public Vector3 GetCenterPosition()
            {
                float _x = x;
                float _y = y;
                float _z = z;
                switch(type)
                {
                    case 2:
                    case 11:
                    case 17:
                        _z -= offsetA * 0.5f;
                        break;
                    case 3:
                    case 12:
                    case 18:
                        _z += offsetA * 0.5f;
                        break;
                    case 4:
                    case 13:
                    case 19:
                        _x -= offsetA * 0.5f;
                        break;
                    case 5:
                    case 14:
                    case 20:
                        _x += offsetA * 0.5f;
                        break;
                    case 6:
                    case 15:
                    case 21:
                        _x += offsetA * 0.5f;
                        _z += offsetB * 0.5f;
                        break;
                }
                return new Vector3(_x, _y, _z);
            }

            public void GetBounds(out Vector3 lower, out Vector3 upper)
            {
                float _lx = x;
                float _ly = y + 100.0f;
                float _lz = z;
                float _hx = x;
                float _hy = y - 900.0f;
                float _hz = z;
                switch (type)
                {
                    case 2:
                    case 11:
                    case 17:
                        _hz -= offsetA;
                        break;
                    case 3:
                    case 12:
                    case 18:
                        _hz += offsetA;
                        break;
                    case 4:
                    case 13:
                    case 19:
                        _hx -= offsetA;
                        break;
                    case 5:
                    case 14:
                    case 20:
                        _hx += offsetA;
                        break;
                    case 6:
                    case 15:
                    case 21:
                        _hx += offsetA;
                        _hz += offsetB;
                        break;
                }
                lower = new Vector3(_lx, _ly, _lz);
                upper = new Vector3(_hx, _hy, _hz);
            }

        }
        #endregion

        #region EventInfoGetters
        public struct EventInfoGetter
        {
            public byte integerOffset;
            public byte bitShift;
            public short bitMask;
            public EventInfoGetter(byte off, byte shift, short mask)
            {
                integerOffset = off;
                bitShift = shift;
                bitMask = mask;
            }

            public short Get(int* eventInts)
            {
                return (short)((*(eventInts + integerOffset) >> bitShift) & bitMask);
            }
        }

        public static EventInfoGetter f6b0 = new EventInfoGetter(0x00, 0x1E, 0x03); //f6b0
        public static EventInfoGetter f6b4 = new EventInfoGetter(0x00, 0x1B, 0x07); //f6b4
        public static EventInfoGetter f6b8 = new EventInfoGetter(0x00, 0x1A, 0x01); //f6b8
        public static EventInfoGetter f6bc = new EventInfoGetter(0x00, 0x0E, 0xFFF); //f6bc
        public static EventInfoGetter f6c0 = new EventInfoGetter(0x00, 0x0D, 0x01); //f6c0
        public static EventInfoGetter f6c4 = new EventInfoGetter(0x00, 0x01, 0xFFF); //f6c4
        public static EventInfoGetter f6c8 = new EventInfoGetter(0x00, 0x00, 0x01); //f6c8

        public static EventInfoGetter eventTrigger = new EventInfoGetter(0x01, 0x1E, 0x03); //f6cc
        public static EventInfoGetter eventLocationMarker = new EventInfoGetter(0x01, 0x12, 0xFFF); //f6d0
        public static EventInfoGetter f6d4 = new EventInfoGetter(0x01, 0x10, 0x03); //f6d4
        public static EventInfoGetter f6d8 = new EventInfoGetter(0x01, 0x08, 0xFF); //f6d8
        public static EventInfoGetter f6dc = new EventInfoGetter(0x01, 0x07, 0x01); //f6dc
        public static EventInfoGetter actionSoundIndex = new EventInfoGetter(0x01, 0x00, 0x7F); //f6f0

        public static EventInfoGetter f6e0 = new EventInfoGetter(0x02, 0x1F, 0x01); //f6e0
        public static EventInfoGetter f6e4 = new EventInfoGetter(0x02, 0x13, 0xFFF); //f6e4
        public static EventInfoGetter f6e8 = new EventInfoGetter(0x02, 0x12, 0x01); //f6e8
        public static EventInfoGetter f6ec = new EventInfoGetter(0x02, 0x06, 0xFFF); //f6ec
        public static EventInfoGetter f6f8 = new EventInfoGetter(0x02, 0x04, 0x03); //f6f8
        public static EventInfoGetter f6fc = new EventInfoGetter(0x02, 0x02, 0x03); //f6fc
        public static EventInfoGetter screenTransitionColor = new EventInfoGetter(0x02, 0x00, 0x03); //f700

        public static EventInfoGetter eventDestinationMarker = new EventInfoGetter(0x03, 0x14, 0xFFF); //f6f4
        public static EventInfoGetter f704 = new EventInfoGetter(0x03, 0x01, 0x01); //f704
        public static EventInfoGetter f708 = new EventInfoGetter(0x03, 0x00, 0x01); //f708
        public static EventInfoGetter f70c = new EventInfoGetter(0x03, 0x0E, 0x3F); //f70c
        public static EventInfoGetter f710 = new EventInfoGetter(0x03, 0x0B, 0x07); //f710
        public static EventInfoGetter f714 = new EventInfoGetter(0x03, 0x03, 0xFF); //f714
        public static EventInfoGetter f718 = new EventInfoGetter(0x03, 0x03, 0xFF); //f718
        public static EventInfoGetter f71c = new EventInfoGetter(0x03, 0x03, 0xFF); //f71c
        public static EventInfoGetter f720 = new EventInfoGetter(0x03, 0x02, 0x01); //f720

        public static EventInfoGetter f724 = new EventInfoGetter(0x04, 0x18, 0xFF); //f724
        public static EventInfoGetter f728 = new EventInfoGetter(0x04, 0x15, 0x01); //f728
        public static EventInfoGetter f72c = new EventInfoGetter(0x04, 0x14, 0x01); //f72c
        #endregion
    }
}