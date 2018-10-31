using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ShiningHill
{
    public class ArcExporter : EditorWindow
    {
        //static EditorWindow _currentWindow;

        [MenuItem("ShiningHill/ArcExporter")]
        public static void ShowWindow()
        {
            /*_currentWindow = */EditorWindow.GetWindow(typeof(ArcExporter));
        }

        unsafe void OnGUI()
        {
            if (GUILayout.Button("Open"))
            {
                new ArcFileSystem();
            }
            if (GUILayout.Button("Load exe"))
            {
                try
                {
                    SH3_ExeData.RegionData[] regionPointers = SH3exeExtractor.ExtractRegionEventData();
                    SH3exeExtractor.UpdateAssetsFromRegions(regionPointers);
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
            if (GUILayout.Button("doit"))
            {
                //TIMFile.ScanForTIMs("Assets/SILENT");

                /*string[] files = Directory.GetFiles("Assets/sh1/Data/1ST/");
                foreach(string f in files)
                {
                    if (Path.GetExtension(f) != ".TIM") continue;

                    TIMReader tim = TIMReader.ReadFile(f);
                    Texture2D t2d = null;
                    try
                    {
                        t2d = tim.GenerateImage();
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                        continue;
                    }
                    File.WriteAllBytes("C:/results/" + Path.GetFileNameWithoutExtension(f) + ".png", t2d.EncodeToPNG());
                    Object.DestroyImmediate(t2d);
                }*/

                int writeSize = 2048;
                byte[] buf = new byte[writeSize];
                
                using (ISO9660FS fs = new ISO9660FS(CDROMStream.MakeFromCue("G:/SH/SH1/1999.02.09 [SLUS-00707][US][PS] Silent Hill.cue")))
                {
                }
            }
        }
    }
}