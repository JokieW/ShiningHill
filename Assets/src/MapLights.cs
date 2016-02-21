using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
	public class MapLights : MonoBehaviour 
	{
        public List<GlobalLight> globalLights = new List<GlobalLight>();
        public List<LocalLight> localLights = new List<LocalLight>();

        public Vector4 Unknown1;
        public Vector4 Unknown2;
        public Color ambientColor;
        public Vector4 Unknown3;


        public static MapLights ReadLights(string path)
        {
            /*string assetPath = path.Replace(".ded", ".asset");*/
            GameObject subGO = Map.BeginEditingPrefab(path, "Lights");

            try
            {
                MapLights lights = subGO.AddComponent<MapLights>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

                reader.SkipInt16(); //Usually the area's code
                reader.SkipInt16(); //Usually 1, saw a 20
                short globalLightsCount = reader.ReadInt16();
                reader.SkipInt16(0);
                short globalLightsOffset = reader.ReadInt16();
                
                reader.SkipBytes(26, 0);
                
                short lightsCount = reader.ReadInt16();
                reader.SkipInt16(0);
                short lightsOffset = reader.ReadInt16();
                reader.SkipInt16(0);

                reader.SkipBytes(40, 0);
                reader.SkipInt32(0xC0);
                reader.SkipBytes(24, 0);

                reader.BaseStream.Position = globalLightsOffset;
                for (int i = 0; i != globalLightsCount; i++)
                {
                    GlobalLight gl = new GlobalLight();
                    gl.rotation = reader.ReadQuaternion();
                    gl.Unknown1 = reader.ReadVector3();
                    reader.SkipInt16(0);
                    gl.Unknown2 = reader.ReadInt16();
                    lights.globalLights.Add(gl);
                }

                reader.BaseStream.Position = lightsOffset;
                for (int i = 0; i != lightsCount; i++)
                {
                    LocalLight ll = new LocalLight();
                    ll.color = reader.ReadColor();
                    ll.Unknown1 = reader.ReadSingle();
                    ll.Range = reader.ReadSingle();
                    reader.SkipBytes(8, 0);
                    ll.position = reader.ReadVector3YInverted();
                    reader.SkipInt16(0x0);
                    reader.SkipInt16(0xA);
                    lights.localLights.Add(ll);

                    GameObject lightGO = new GameObject("Light " + i);
                    lightGO.transform.SetParent(subGO.transform);
                    lightGO.transform.localPosition = ll.position;

                    Light light = lightGO.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.range = ll.Range * Map.GLOBAL_SCALE;
                    light.color = ll.color;
                    light.intensity = 8.0f;
                    light.bounceIntensity = 1.0f;
                }

                lights.Unknown1 = reader.ReadVector4();
                lights.Unknown2 = reader.ReadVector4();
                lights.ambientColor = reader.ReadColor();
                lights.Unknown3 = reader.ReadVector4();

                reader.Close();

                Map.FinishEditingPrefab(path, subGO);

                return lights;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        [Serializable]
        public class GlobalLight
        {
            public Quaternion rotation;
            public Vector3 Unknown1;
            public short Unknown2;
        }

        [Serializable]
        public class LocalLight
        {
            public Color color;
            public float Unknown1;
            public float Range;
            public Vector3 position;
        }
	}
}