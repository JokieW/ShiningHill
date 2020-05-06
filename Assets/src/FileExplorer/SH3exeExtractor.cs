using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

namespace ShiningHill
{
    public static class SH3exeExtractor
    {
        static VirtualAddress _regionPointerArrayPtr = 0x006cf7d0;
        static VirtualAddress _regionNamesArrayPtr = 0x006cf730;

        public static SH3_ExeData.RegionData[] ExtractRegionEventData()
        {
            BinaryReader reader = null;//new BinaryReader(new FileStream(SH3ExeAssetPaths.GetHardAssetPath(), FileMode.Open, FileAccess.Read, FileShare.Read));
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
                        VirtualAddress eventsPtr = reader.ReadIntPtr(); //[0] General Events
                        VirtualAddress markersPtr = reader.ReadIntPtr(); //[1] Markers
                        VirtualAddress secondEventsPtr = reader.ReadIntPtr(); //[2] Second Events (?)
                        VirtualAddress aaaa = reader.ReadIntPtr();//[3]
                        VirtualAddress entitiesPtr = reader.ReadIntPtr();//[4] Entities
                        //[5] Interest points
                        //[6]
                        //[7] Function
                        //[8] Function
                        //[9] Valtiel death animation location
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

                                bool hasY = false;
                                byte type = reader.ReadByte();
                                byte adjustedType;
                                if(type >= 16)
                                {
                                    adjustedType = (byte)(type - 15); 
                                }
                                else
                                {
                                    adjustedType = type;
                                    hasY = true;
                                }

                                switch(adjustedType)
                                {
                                    case 0:
                                        break;
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                    case 5:
                                    case 6:
                                    case 7:
                                    case 8:
                                    case 9:
                                        data.markers.Add(new SH3_ExeData.EventMarker()
                                        {
                                            offset = offset,
                                            type = type,
                                            x = reader.ReadSingle(),
                                            y = hasY ? reader.ReadHalf() : 0.0f,
                                            z = reader.ReadSingle(),
                                            offset1 = adjustedType >= 2 ? reader.ReadHalf() : 0.0f,
                                            offset2 = adjustedType >= 6 ? reader.ReadHalf() : 0.0f,
                                            offset3 = adjustedType >= 7 ? reader.ReadHalf() : 0.0f,
                                            offset4 = adjustedType >= 7 ? reader.ReadHalf() : 0.0f,
                                            offset5 = adjustedType >= 8 ? reader.ReadHalf() : 0.0f,
                                            offset6 = adjustedType >= 8 ? reader.ReadHalf() : 0.0f,
                                            offset7 = adjustedType >= 9 ? reader.ReadHalf() : 0.0f,
                                            offset8 = adjustedType >= 9 ? reader.ReadHalf() : 0.0f
                                        });
                                        break;
                                    case 13:
                                    case 15:
                                        data.markers.Add(new SH3_ExeData.EventMarker()
                                        {
                                            offset = offset,
                                            type = type,
                                            x = reader.ReadSingle(),
                                            y = hasY ? reader.ReadHalf() : 0.0f,
                                            z = reader.ReadSingle(),
                                            offset1 = adjustedType >= 13 ? reader.ReadHalf() : 0.0f,
                                            offset2 = adjustedType >= 13 ? reader.ReadHalf() : 0.0f,
                                            offset3 = adjustedType >= 15 ? reader.ReadHalf() : 0.0f,
                                            offset4 = adjustedType >= 15 ? reader.ReadHalf() : 0.0f,
                                            offset5 = adjustedType >= 15 ? reader.ReadHalf() : 0.0f,
                                            offset6 = adjustedType >= 15 ? reader.ReadHalf() : 0.0f,
                                        });
                                        break;
                                    default:
                                        Debug.LogWarning("Untreated Event Marker " + type + " region " + i + " offset " + offset);
                                        break;
                                }
                            }

                            //Fill second events
                            {
                                data.secondEvents = new List<SH3_ExeData.EventInfo>();
                                if (secondEventsPtr.IsRawSpace())
                                {
                                    reader.BaseStream.Position = secondEventsPtr.raw;
                                    SH3_ExeData.EventInfo ei;
                                    int count = 0;
                                    while (!(ei = new SH3_ExeData.EventInfo(reader, count++)).IsNull())
                                    {
                                        data.secondEvents.Add(ei);
                                    }
                                }
                            }

                            //Fill entities
                            {
                                data.entityInfos = new List<SH3_ExeData.EntityInfo>();
                                if (entitiesPtr.IsRawSpace())
                                {
                                    reader.BaseStream.Position = entitiesPtr.raw;
                                    SH3_ExeData.EntityInfo ei;
                                    while (!(ei = new SH3_ExeData.EntityInfo(reader)).IsNull())
                                    {
                                        data.entityInfos.Add(ei);
                                    }
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

        public static void UpdateAssetsFromRegions(SH3_ExeData.RegionData[] regions)
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int regioni = 0; regioni != regions.Length; regioni++)
                {
                    SH3_ExeData.RegionData region = regions[regioni];
                    if (region.markers != null)
                    {
                        List<MeshCombineUtility.MeshInstance> _meshes = new List<MeshCombineUtility.MeshInstance>(region.markers.Count);
                        List<Vector3> _lines = new List<Vector3>(region.markers.Count);
                        HashSet<int> _markersDone = new HashSet<int>();
                        for (int i = 0; i != region.markers.Count; i++)
                        {
                            SH3_ExeData.EventMarker marker = region.markers[i];
                            if (!_markersDone.Contains(marker.offset))
                            {
                                Mesh m;
                                Vector3[] lines;
                                marker.GenerateMesh(out m, out lines);
                                if (m != null) _meshes.Add(new MeshCombineUtility.MeshInstance() { mesh = m, subMeshIndex = 0, transform = Matrix4x4.identity });
                                if (lines != null) _lines.AddRange(lines);
                                _markersDone.Add(marker.offset);
                            }
                        }

                        GameObject go = new GameObject("Region " + regioni + ": " + region.name);
                        go.isStatic = true;
                        SH3_Region reg = go.AddComponent<SH3_Region>();
                        reg.markerMesh = MeshCombineUtility.Combine(_meshes, false);
                        reg.markerLines = _lines.ToArray();
                        reg.regionData = region;
                        go.AddComponent<MeshFilter>().sharedMesh = reg.markerMesh;
                        go.AddComponent<MeshRenderer>().sharedMaterial = null;//MaterialRolodex.defaultGizmo;

                        {
                            string path = null;//SH3ExeAssetPaths.GetRegionExtractAssetPath(region.name) + region.name + ".asset";
                            MakeDirectory(path);
                            AssetDatabase.DeleteAsset(path);
                            AssetDatabase.CreateAsset(reg.markerMesh, path);
                        }
                        {
                            string path = null;//SH3ExeAssetPaths.GetRegionPrefabPath(region.name) + region.name + ".prefab";
                            MakeDirectory(path);
                            AssetDatabase.DeleteAsset(path);
                            PrefabUtility.CreatePrefab(path, go);
                        }
                        GameObject.DestroyImmediate(go);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
        }

        public static void MakeDirectory(string path)
        {
            string directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        }
    }
}