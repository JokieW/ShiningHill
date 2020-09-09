using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

using SH.GameData.Shared;

namespace SH.GameData.SH3
{
    [Serializable]
    public struct VirtualAddress
    {
        [SerializeField]
        [Hex] long address;

        public VirtualAddress(long virtualaddress)
        {
            this.address = virtualaddress;
        }

        public long raw
        {
            get
            {
                long rawAddr = address - 0x00400000L;
                return rawAddr < 0L ? 0L : rawAddr;
            }
        }

        public bool IsAboveRawSpace()
        {
            return raw > 0x0032CFFFL;
        }

        public bool IsRawSpace()
        {
            return raw >= 0x1000 && raw <= 0x0032CFFFL;
        }

        public bool IsNull()
        {
            return raw < 0x1000L;
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

    public unsafe static class ExeData
    {
        #region Region
        [Serializable]
        public class RegionData
        {
            public VirtualAddress address;
            public string name;
            public List<EventInfo> events;
            public List<EventMarker> markers;
            public List<EventInfo> secondEvents;
            public List<EntityInfo> entityInfos;
        }
        #endregion

        #region Events
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct EventInfo
        {
            [SerializeField]
            [Hex] public int eventNumber;
            [SerializeField]
            [Hex] public int int0;
            [SerializeField]
            [Hex] public int int1;
            [SerializeField]
            [Hex] public int int2;
            [SerializeField]
            [Hex] public int int3;
            [SerializeField]
            [Hex] public int int4;


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
            [Hex] public short offset;
            [SerializeField]
            [Hex] public byte type;
            [SerializeField]
            public float x;
            [SerializeField]
            public float y;
            [SerializeField]
            public float z;
            [SerializeField]
            public float offset1;
            [SerializeField]
            public float offset2;
            [SerializeField]
            public float offset3;
            [SerializeField]
            public float offset4;
            [SerializeField]
            public float offset5;
            [SerializeField]
            public float offset6;
            [SerializeField]
            public float offset7;
            [SerializeField]
            public float offset8;

            private Vector3[] GetAppliedOffsets()
            {
                int type2 = type >= 16 ? type - 15 : type;
                switch (type2)
                {
                    case 1:
                        return null;
                    case 2:
                        return new Vector3[] { new Vector3(x, y, z - offset1) };
                    case 3:
                        return new Vector3[] { new Vector3(x, y, z + offset1) };
                    case 4:
                        return new Vector3[] { new Vector3(x - offset1, y, z) };
                    case 5:
                        return new Vector3[] { new Vector3(x + offset1, y, z) };
                    case 6:
                        return new Vector3[] { new Vector3(x + offset1, y, z + offset2) };
                    case 7:
                        return new Vector3[] { new Vector3(x + offset1, y, z + offset2), new Vector3(x + offset3, y, z + offset4) };
                    case 8:
                        return new Vector3[] { new Vector3(x + offset1, y, z + offset2), new Vector3(x + offset3, y, z + offset4),
                            new Vector3(x + offset5, y, z + offset6) };
                    case 9:
                        return new Vector3[] { new Vector3(x + offset1, y, z + offset2), new Vector3(x + offset3, y, z + offset4),
                            new Vector3(x + offset5, y, z + offset6), new Vector3(x + offset7, y, z + offset8) };
                    case 13:
                        return new Vector3[] { new Vector3(x + offset1, y, z + offset2) };
                    case 15:
                        return new Vector3[] { new Vector3(x + offset1, y, z + offset2), new Vector3(x + offset3, y, z + offset4), new Vector3(x + offset5, y, z + offset6) };
                    default:
                        return null;
                }
            }

            public Vector3 GetCenter()
            {
                Vector3 center = new Vector3(x, y, z);
                int count = 1;
                Vector3[] offs = GetAppliedOffsets();
                if (offs != null)
                {
                    for (int i = 0; i != offs.Length; i++)
                    {
                        center += offs[i];
                        count++;
                    }
                }

                return center / count;

            }

            public void GenerateMesh(out Mesh mesh, out Vector3[] lines)
            {
                Vector3[] offsets = GetAppliedOffsets();
                int type2;
                float lowYAdj = 0.0f;
                float highYAdj = -1000.0f;
                if (type >= 16)
                {
                    type2 = type - 15;
                }
                else
                {
                    type2 = type;
                    lowYAdj = 100.0f;
                    highYAdj = -900.0f;
                }
                
                switch(type2)
                {
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        {
                            Vector3 v0 = new Vector3(x, y + lowYAdj, z);
                            Vector3 v1 = new Vector3(x, y + highYAdj, z);
                            Vector3 v2 = offsets[0] + new Vector3(0, highYAdj, 0);
                            Vector3 v3 = offsets[0] + new Vector3(0, lowYAdj, 0);
                            mesh = new Mesh();
                            mesh.vertices = new Vector3[] { v0, v1, v2, v3 };
                            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
                            mesh.RecalculateNormals();
                            lines = new Vector3[] { v0, v3, v1, v2, v0, v1, v2, v3 };
                            return;
                        }
                    case 7:
                        {
                            Vector3 v0 = new Vector3(x, y + lowYAdj, z);
                            Vector3 v1 = new Vector3(x, y + highYAdj, z);
                            Vector3 v2 = offsets[0] + new Vector3(0, highYAdj, 0);
                            Vector3 v3 = offsets[0] + new Vector3(0, lowYAdj, 0);
                            Vector3 v4 = offsets[1] + new Vector3(0, highYAdj, 0);
                            Vector3 v5 = offsets[1] + new Vector3(0, lowYAdj, 0);
                            mesh = new Mesh();
                            mesh.vertices = new Vector3[] { v0, v1, v2, v3, v4, v5 };
                            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3, 3, 2, 5, 5, 2, 4 };
                            mesh.RecalculateNormals();
                            lines = new Vector3[] { v0, v3, v3, v5, v1, v2, v2, v4, v0, v1, v4, v5 };
                            return;
                        }
                    case 8:
                        {
                            Vector3 v0 = new Vector3(x, y + lowYAdj, z);
                            Vector3 v1 = new Vector3(x, y + highYAdj, z);
                            Vector3 v2 = offsets[0] + new Vector3(0, highYAdj, 0);
                            Vector3 v3 = offsets[0] + new Vector3(0, lowYAdj, 0);
                            Vector3 v4 = offsets[1] + new Vector3(0, highYAdj, 0);
                            Vector3 v5 = offsets[1] + new Vector3(0, lowYAdj, 0);
                            Vector3 v6 = offsets[2] + new Vector3(0, highYAdj, 0);
                            Vector3 v7 = offsets[2] + new Vector3(0, lowYAdj, 0);
                            mesh = new Mesh();
                            mesh.vertices = new Vector3[] { v0, v1, v2, v3, v4, v5, v6, v7 };
                            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3, 3, 2, 5, 5, 2, 4, 5, 4, 7, 7, 4, 6 };
                            mesh.RecalculateNormals();
                            lines = new Vector3[] { v0, v3, v3, v5, v5, v7, v1, v2, v2, v4, v4, v6, v0, v1, v7, v6 };
                            return;
                        }
                    case 9:
                        {
                            Vector3 v0 = new Vector3(x, y + lowYAdj, z);
                            Vector3 v1 = new Vector3(x, y + highYAdj, z);
                            Vector3 v2 = offsets[0] + new Vector3(0, highYAdj, 0);
                            Vector3 v3 = offsets[0] + new Vector3(0, lowYAdj, 0);
                            Vector3 v4 = offsets[1] + new Vector3(0, highYAdj, 0);
                            Vector3 v5 = offsets[1] + new Vector3(0, lowYAdj, 0);
                            Vector3 v6 = offsets[2] + new Vector3(0, highYAdj, 0);
                            Vector3 v7 = offsets[2] + new Vector3(0, lowYAdj, 0);
                            Vector3 v8 = offsets[3] + new Vector3(0, highYAdj, 0);
                            Vector3 v9 = offsets[3] + new Vector3(0, lowYAdj, 0);
                            mesh = new Mesh();
                            mesh.vertices = new Vector3[] { v0, v1, v2, v3, v4, v5, v6, v7, v8, v9 };
                            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3, 3, 2, 5, 5, 2, 4, 5, 4, 7, 7, 4, 6, 7, 6, 9, 9, 6, 8};
                            mesh.RecalculateNormals();
                            lines = new Vector3[] { v0, v3, v3, v5, v5, v7, v7, v9, v1, v2, v2, v4, v4, v6, v6, v8, v0, v1, v9, v8 };
                            return;
                        }
                    case 13:
                        {
                            Vector3 v0 = new Vector3(x, y + lowYAdj, z);
                            Vector3 v1 = new Vector3(x + offset1, y + lowYAdj, z);
                            Vector3 v2 = new Vector3(x + offset1, y + lowYAdj, z + offset2);
                            Vector3 v3 = new Vector3(x, y + lowYAdj, z + offset2);
                            Vector3 ex = new Vector3(0, -1000, 0);
                            mesh = new Mesh();
                            mesh.vertices = new Vector3[] { v0, v1, v2, v3 };
                            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
                            mesh.RecalculateNormals();
                            lines = new Vector3[] { v0, v1, v0, v3, v2, v1, v2, v3, v0, v0 + ex, v1, v1 + ex, v2, v2 + ex, v3, v3 + ex };
                            return;
                        }
                    case 15:
                        {
                            Vector3 v0 = new Vector3(x, y + lowYAdj, z);
                            Vector3 v1 = new Vector3(x + offset1, y + lowYAdj, z + offset2);
                            Vector3 v2 = new Vector3(x + offset3, y + lowYAdj, z + offset4);
                            Vector3 v3 = new Vector3(x + offset5, y + lowYAdj, z + offset6);
                            Vector3 ex = new Vector3(0, -1000, 0);
                            mesh = new Mesh();
                            mesh.vertices = new Vector3[] { v0, v1, v2, v3 };
                            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
                            mesh.RecalculateNormals();
                            lines = new Vector3[] { v0, v1, v0, v3, v2, v1, v2, v3, v0, v0 + ex, v1, v1 + ex, v2, v2 + ex, v3, v3 + ex };
                            return;
                        }
                    default:
                        mesh = null;
                        lines = null;
                        return;
                }
            }

        }
        #endregion

        #region EventInfoGetters
        public struct EventInfoGetter
        {
            [Hex] public byte integerOffset;
            [Hex] public byte bitShift;
            [Hex] public short bitMask;
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

        #region Entities
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct EntityInfo
        {
            [SerializeField]
            [Hex] public short entityTypeID;
            [SerializeField]
            [Hex] public short globalID;
            [SerializeField]
            public float x;
            [SerializeField]
            public float y;
            [SerializeField]
            public float z;
            [SerializeField]
            public float rotY;
            [SerializeField]
            [Hex] public short a;
            [SerializeField]
            [Hex] public short b;
            [SerializeField]
            [Hex] public short c;
            [SerializeField]
            [Hex] public short d;

            public EntityInfo(BinaryReader reader)
            {
                entityTypeID = reader.ReadInt16();
                globalID = reader.ReadInt16();
                x = reader.ReadInt32();
                z = reader.ReadInt32();
                y = reader.ReadHalf();
                rotY = reader.ReadHalf();
                a = reader.ReadInt16();
                b = reader.ReadInt16();
                c = reader.ReadInt16();
                d = reader.ReadInt16();
                if (entityTypeID == 0x216) Debug.Log("id " + globalID + " d " + d);
            }

            public bool IsNull()
            {
                return entityTypeID == 0;
            }
        }
        #endregion
    }
}
