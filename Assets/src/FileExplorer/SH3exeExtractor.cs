using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace ShiningHill
{
    public static class SH3exeExtractor
    {
        static class SH3ExeAssetPaths
        {
            public static string GetHardAssetPath()
            {
                string path = CustomPostprocessor.GetHardDataPathFor(SHGame.SH3PC);
                return path + "sh3.exe";
            }

            public static string GetExtractAssetPath()
            {
                string path = CustomPostprocessor.GetExtractDataPathFor(SHGame.SH3PC);
                return path + "sh3.asset";
            }

            public static string GetPrefabPath()
            {
                string path = CustomPostprocessor.GetExtractDataPathFor(SHGame.SH3PC);
                return path + "Prefabs/sh3.prefab";
            }
        }
        
        static VirtualAddress _regionPointerArrayPtr = 0x006cf7d0;
        static VirtualAddress _regionNamesArrayPtr = 0x006cf730;

        public static SH3_ExeData.RegionData[] ExtractRegionEventData()
        {
            BinaryReader reader = new BinaryReader(new FileStream(SH3ExeAssetPaths.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));
            try
            {
                reader.BaseStream.Position = _regionPointerArrayPtr.raw;
                SH3_ExeData.RegionData[] regionPointers = new SH3_ExeData.RegionData[40];
                for (int i = 0; i != 40; i++)
                {
                    SH3_ExeData.RegionData data = new SH3_ExeData.RegionData();
                    data.address = reader.ReadIntPtr();
                    regionPointers[i] = data;
                }

                reader.BaseStream.Position = _regionNamesArrayPtr.raw;
                for (int i = 0; i != 40; i++)
                {
                    VirtualAddress name = reader.ReadIntPtr();
                    if (name.raw != 0)
                    {
                        long pos = reader.BaseStream.Position;
                        reader.BaseStream.Position = name.raw;
                        regionPointers[i].name = reader.ReadNullTerminatedString();
                        reader.BaseStream.Position = pos;
                    }
                    else
                    {
                        regionPointers[i].name = "";
                    }
                }

                for (int i = 0; i != regionPointers.Length; i++)
                {
                    SH3_ExeData.RegionData data = regionPointers[i];
                    if (data.address.@virtual != 0L)
                    {
                        reader.BaseStream.Position = data.address.raw;
                        VirtualAddress eventsPtr = reader.ReadIntPtr();
                        VirtualAddress markersPtr = reader.ReadIntPtr();
                        // other stuff eventually

                        //Fill events
                        {
                            data.events = new List<SH3_ExeData.EventInfo>();
                            reader.BaseStream.Position = eventsPtr.raw;
                            SH3_ExeData.EventInfo ei;
                            int count = 0;
                            while (!(ei = new SH3_ExeData.EventInfo(reader, count++)).IsNull())
                            {
                                data.events.Add(ei);
                            }
                        }

                        //Fill markers
                        {
                            data.markers = new List<SH3_ExeData.EventMarker>();
                            for (int j = 0; j != data.events.Count; j++)
                            {
                                SH3_ExeData.EventInfo ev = data.events[j];
                                short offset = ev.GetLocationOffset();
                                reader.BaseStream.Position = markersPtr.raw + offset;

                                byte type = reader.ReadByte();
                                switch(type)
                                {
                                    case 0:
                                        break;
                                    case 2:
                                    case 3:
                                    case 4:
                                    case 5:
                                        data.markers.Add(new SH3_ExeData.EventMarker()
                                        {
                                            offset = offset,
                                            type = type,
                                            x = reader.ReadSingle(),
                                            y = reader.ReadHalf(),
                                            z = reader.ReadSingle(),
                                            offsetA = reader.ReadHalf(),
                                            offsetB = 0.0f
                                        });
                                        break;
                                    case 6:
                                        data.markers.Add(new SH3_ExeData.EventMarker()
                                        {
                                            offset = offset,
                                            type = type,
                                            x = reader.ReadSingle(),
                                            y = reader.ReadHalf(),
                                            z = reader.ReadSingle(),
                                            offsetA = reader.ReadHalf(),
                                            offsetB = reader.ReadHalf()
                                        });
                                        break;
                                    case 11:
                                    case 12:
                                    case 13:
                                    case 14:
                                    case 17:
                                    case 18:
                                    case 19:
                                    case 20:
                                        data.markers.Add(new SH3_ExeData.EventMarker()
                                        {
                                            offset = offset,
                                            type = type,
                                            x = reader.ReadSingle(),
                                            y = 0.0f,
                                            z = reader.ReadSingle(),
                                            offsetA = reader.ReadHalf(),
                                            offsetB = 0.0f
                                        });
                                        break;
                                    case 15:
                                    case 21:
                                        data.markers.Add(new SH3_ExeData.EventMarker()
                                        {
                                            offset = offset,
                                            type = type,
                                            x = reader.ReadSingle(),
                                            y = 0.0f,
                                            z = reader.ReadSingle(),
                                            offsetA = reader.ReadHalf(),
                                            offsetB = reader.ReadHalf()
                                        });
                                        break;
                                    default:
                                        Debug.LogWarning("Untreated Event Marker " + type);
                                        data.markers.Add(new SH3_ExeData.EventMarker()
                                        {
                                            offset = offset,
                                            type = type,
                                            x = 0.0f,
                                            y = 0.0f,
                                            z = 0.0f,
                                            offsetA = 0.0f,
                                            offsetB = 0.0f
                                        });
                                        break;
                                }
                            }
                        }
                    }
                }
                return regionPointers;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                reader.Close();
            }
            return null;
        }


    }
}