﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using SH.GameData.SH3;
using SH.Unity.SH3;
using SH.GameData.Shared;
using SH.GameData.SH1;
using SH.Core.Stream;

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

        public static Mesh mesh;
        public static Texture2D texture;
        unsafe void OnGUI()
        {
            //if (GUILayout.Button("Open"))
            {
                //ArcFileSystem.DecompressArcs();
            }

            //if (GUILayout.Button("compress"))
            {
                //ArcFileSystem.CompressArcs();
            }
            if (GUILayout.Button("Load exe"))
            {
                try
                {
                    ExeData.RegionData[] regionPointers = ExeExtractor.ExtractRegionEventData();
                    ExeExtractor.UpdateAssetsFromRegions(regionPointers);
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
            if (GUILayout.Button("test cld"))
            {
                MapCollisions mc = MapCollisions.MakeDebug();
                /*using (FileStream file = new FileStream(@"C:\Silent Hill 3\arc\bgam\data\bg\am\am1e.cld", FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(file))
                {
                    mc = new MapCollisions(reader);
                }*/

                using (FileStream file = new FileStream(@"C:\Silent Hill 3\arc\bgmr\data\bg\mr\mrff.cld", FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    mc.Write(writer);
                }
            }
            mesh = (Mesh)EditorGUILayout.ObjectField(mesh, typeof(Mesh), true);
            texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), true);
            if (GUILayout.Button("test map"))
            {
                MapGeometry m;
                using (FileStream file = new FileStream(@"C:\Silent Hill 3 - Copy\arc\bgmr\data\tmp\mrff.map", FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(file))
                {
                    m = new MapGeometry(reader);
                }

                m.DoHack(mesh, texture);

                using (FileStream file = new FileStream(@"C:\Silent Hill 3\arc\bgmr\data\tmp\mrff.map", FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    m.Write(writer);
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