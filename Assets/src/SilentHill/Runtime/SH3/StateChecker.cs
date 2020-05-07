using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SH.Runtime.Shared;

namespace SH.Runtime.SH3
{
    public class StateChecker : MonoBehaviour
    {
        [SerializeField]
        private long _memHandle;
        public IntPtr memHandle { get { return new IntPtr(_memHandle); } set { _memHandle = value.ToInt64(); } }
        public static Func<IntPtr> GetHandleFromWindow;

        public static StateChecker instance;

        public GameObject Map;
        public short lastMap;
        public short lastSector;

        public GameObject Entities;
        public GameObject[] livingEntities;


        private SHPtr s_curMap = 0x00B71C42;
        private SHPtr s_curSector = 0x00B71C40;
        private SHPtr s_curZone = 0x070E66D9;

        void Awake()
        {
            instance = this;
            memHandle = GetHandleFromWindow();
        }
        void Start()
        {
            SH3RunEntity.CreateEntities(this);

        }

        void Update()
        {
            if (instance == null) instance = this;

            short currentMap = Scribe.ReadInt16(memHandle, s_curMap);
            lastSector = Scribe.ReadInt16(memHandle, s_curSector);
            if (lastMap != currentMap)
            {
                lastMap = currentMap;
                StartCoroutine(LoadNewMap());
            }
        }

        [ContextMenu("ReloadLastMap")]
        void LoadMap()
        {
            StartCoroutine(LoadNewMap());
        }

        private static readonly Dictionary<int, string> _idToMapName = new Dictionary<int, string>()
    {
        {0x01, "mr"}, {0x02, "mu"}, {0x03, "tr"}, {0x04, "us"}, {0x05, "bc"}, {0x06, "br"}, {0x07, "bu"}, {0x08, "tg"}, {0x09, "am"}, {0x0A, "cm"},
        {0x0B, "ot"}, {0x0C, "cc"}, {0x0D, "th"}, {0x0E, "hp"}, {0x0F, "hc"}, {0x10, "hu"}, {0x11, "tp"}, {0x12, "cr"}, {0x13, "cu"}, {0x14, "cz"},
    };



        IEnumerator LoadNewMap()
        {
            //fix with new pipeline
#if false
        foreach (Transform child in Map.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        string mapname;
        if (_idToMapName.TryGetValue(lastMap, out mapname))
        {
            string path = "Assets/SilentHill3/Resources/data/data/bg/" + mapname + "/";

            for (byte x = 0; x != 0x10; x++)
            {
                for (byte z = 0; z != 0x10; z++)
                {
                    byte sector = (byte)(z | (x << 4));
                    string sectorname = mapname + sector.ToString("X2");
                    Debug.Log("Tried: " + sectorname);


                    if (File.Exists(path + sectorname + ".map"))
                    {
                        string prefab = path + sectorname + ".prefab";
                        if (!File.Exists(prefab))
                        {
                            try
                            {
                                ShiningHill.Map.ReadMap(new ShiningHill.MapAssetPaths(path + sectorname + ".map", ShiningHill.SHGame.SH3PC));
                            }
                            catch (Exception e)
                            { }
                        }
                        UnityEngine.Object o = Resources.Load("data/data/bg/" + mapname + "/" + sectorname, typeof(GameObject));
                        if (o == null)
                        {
                            Debug.LogError("Failed loading " + sectorname);
                        }
                        else
                        {
                            Instantiate(o, Map.transform);
                        }
                    }
                }
            }
        }
#endif
            yield return null;
        }
    }
}
