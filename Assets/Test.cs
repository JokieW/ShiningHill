using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SH.GameData.SH2;
using SH.Core;
using System.Runtime.InteropServices;

public class Test : MonoBehaviour
{
    [DllImport("sh2data.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int TestFunc();

    [ContextMenu("dll")]
    void dll()
    {
        Debug.Log(TestFunc());
    }

    [ContextMenu("doit")]
    void doit()
    {
        string root = "Assets/upk/sh2dcpc/work/data/";
        string[] allfiles = Directory.GetFiles(root, "*.map", SearchOption.AllDirectories);
        List<SubFileTex> textureFiles = new List<SubFileTex>(8);
        Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();
        foreach(string file in allfiles)
        {
            if (file.Contains("GB")) continue;
            FileMap map = FileMap.ReadMapFile(file, true, false);
            textureFiles.Clear();
            map.GetTextureFiles(textureFiles);
            foreach(SubFileTex tex in textureFiles)
            {
                foreach(SubFileTex.DXTTexture dxt in tex.textures)
                {
                    for(int i = 0; i < dxt.sprites.Length; i++)
                    {
                        //FileTex.DXTTexture.Sprite sub = dxt.sprites[i];
                        /*if(sub.field_1C != dxt.subgroups[0].field_1C)
                        {
                            Debug.Log("mismatch, 1C");
                        }
                        if (sub.field_18 != dxt.subgroups[0].field_18)
                        {
                            Debug.Log("mismatch, 18");
                        }*/
                        /*if(sub.field_18 != dxt.header.width)
                         {
                             Debug.Log("mismatch, 18 = " + sub.field_18 + ", width = " + dxt.header.width);
                         }
                         if (sub.field_1A != dxt.header.height)
                         {
                             Debug.Log("mismatch, 1A = " + sub.field_1A + ", height = " + dxt.header.height + " map " + file);
                         }*/
                        
                        /*AddInt(results, nameof(sub.field_00), sub.field_00);
                        AddInt(results, nameof(sub.field_02), sub.field_02);
                        AddInt(results, nameof(sub.field_04), sub.field_04);
                        AddInt(results, nameof(sub.width), sub.width);
                        AddInt(results, nameof(sub.format), sub.format);
                        AddInt(results, nameof(sub.field_12), sub.field_12);
                        AddInt(results, nameof(sub.field_14), sub.field_14);
                        AddInt(results, nameof(sub.field_18), sub.field_18);
                        AddInt(results, nameof(sub.field_1A), sub.field_1A);
                        AddInt(results, nameof(sub.field_1C), sub.field_1C);
                        AddInt(results, nameof(sub.field_1E), sub.field_1E);*/
                    }
                }
            }
        }
        
        foreach(KeyValuePair<string, List<string>> kvp in results)
        {
            Debug.Log(kvp.Key);
            foreach(string s in kvp.Value)
            {
                Debug.LogWarning(s);
            }
        }
        Debug.Log("end");
    }

    void AddInt(Dictionary<string, List<string>> results, string name, int value)
    {
        AddResult(results, name, value, "X8");
    }

    void AddInt(Dictionary<string, List<string>> results, string name, short value)
    {
        AddResult(results, name, value, "X4");
    }

    void AddResult(Dictionary<string, List<string>> results, string name, int value, string tostring)
    {
        List<string> subresult;
        if(!results.TryGetValue(name, out subresult))
        {
            subresult = new List<string>();
            results.Add(name, subresult);
        }
        string stringValue = value.ToString(tostring);
        if(!subresult.Contains(stringValue))
        {
            subresult.Add(stringValue);
        }
    }
}
