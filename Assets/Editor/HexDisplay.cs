using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class HexDisplay 
{
    static GUIStyle _style;

    static HexDisplay()
    {
        _style = new GUIStyle(GUI.skin.textArea);
        _style.font = (Font)Resources.Load("Fonts/cour", typeof(Font));
        _style.wordWrap = false;
    }

    //private static Dictionary<byte[], string>

    private static int _MAXROWS = 40;

    public static Vector2 Display(Vector2 scroll, byte[] data, int bytesPerRow, int grouping, DisplayType dispType, GUILayoutOption[] layoutOptions)
    {
        try
        {
            float _dataWidth = ((bytesPerRow * 2) + (bytesPerRow / grouping) - 1) * 10.25f;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Offset (h)", GUILayout.Width(90.0f));
            GUILayout.Label("Data", GUILayout.Width(_dataWidth));
            GUILayout.Label("Preview");
            GUILayout.EndHorizontal();

            StringBuilder indexSB = new StringBuilder();
            StringBuilder centerSB = new StringBuilder();
            StringBuilder previewSB = new StringBuilder();

            long pointer = (long)scroll.y;
            int currentRow = 0;
            while (pointer < data.LongLength && currentRow <= _MAXROWS)
            {
                indexSB.AppendFormat("{0:X8}", pointer);
                long chunkSize = Min(bytesPerRow, data.LongLength - pointer);
                int curGrouping = 0;
                for (long i = 0L; i != chunkSize; i++)
                {
                    centerSB.AppendFormat("{0:X2}", data[pointer + i]);
                    curGrouping++;
                    if (curGrouping == grouping && i != chunkSize - 1)
                    {
                        curGrouping = 0;
                        centerSB.Append(" ");
                    }
                }


                for (long i = 0L; i != chunkSize; i++)
                {
                    
                    if (dispType == DisplayType.Byte)
                    {
                        previewSB.AppendFormat("{0} ", (int)data[pointer + i]);
                    }
                    else if (dispType == DisplayType.Byte2)
                    {
                        previewSB.AppendFormat("{0} ", BitConverter.ToInt16(new byte[] { data[pointer + i], data[pointer + i + 1 ] }, 0));
                        i += 1;
                    }
                    else if (dispType == DisplayType.Byte4)
                    {
                        previewSB.AppendFormat("{0} ", BitConverter.ToInt32(new byte[] { data[pointer + i], data[pointer + i + 1], data[pointer + i + 2], data[pointer + i + 3] }, 0));
                        i += 3;
                    }
                    else if (dispType == DisplayType.Byte8)
                    {
                        previewSB.AppendFormat("{0} ", BitConverter.ToInt32(new byte[] { data[pointer + i], data[pointer + i + 1], data[pointer + i + 2], data[pointer + i + 3], data[pointer + i + 4], data[pointer + i + 5], data[pointer + i + 6], data[pointer + i + 7] }, 0));
                        i += 7;
                    }
                    else if (dispType == DisplayType.Float)
                    {
                        previewSB.AppendFormat("{0} ", BitConverter.ToSingle(new byte[] { data[pointer + i], data[pointer + i + 1], data[pointer + i + 2], data[pointer + i + 3] }, 0));
                        i += 3;
                    }
                    else if (dispType == DisplayType.Double)
                    {
                        previewSB.AppendFormat("{0} ", BitConverter.ToDouble(new byte[] { data[pointer + i], data[pointer + i + 1], data[pointer + i + 2], data[pointer + i + 3], data[pointer + i + 4], data[pointer + i + 5], data[pointer + i + 6], data[pointer + i + 7] }, 0));
                        i += 7;
                    }
                    else //ANSI
                    {
                        byte b = data[pointer + i];
                        previewSB.AppendFormat("{0:X}", BadChar(b) ? '.' : System.Convert.ToChar(b));
                    }
                }

                indexSB.Append("\n");
                centerSB.Append("\n");
                previewSB.Append("\n");

                currentRow++;
                pointer += chunkSize;
            }


            //Vector2 temp = EditorGUILayout.BeginScrollView(scroll, GUI.skin.horizontalScrollbar, GUIStyle.none);
            GUILayout.BeginHorizontal();
            GUILayout.TextArea(indexSB.ToString(), _style, GUILayout.Width(90.0f));
            GUILayout.TextArea(centerSB.ToString(), _style, GUILayout.Width(_dataWidth));
            GUILayout.TextArea(previewSB.ToString(), _style);
            scroll.y = GUILayout.VerticalScrollbar(scroll.y, currentRow, 0.0f, data.LongLength / (long)bytesPerRow, GUILayout.ExpandHeight(true));
            scroll.y = scroll.y >= 0 ? Mathf.Round(scroll.y / bytesPerRow) * bytesPerRow : 0.0f;
            GUILayout.EndHorizontal();

            //EditorGUILayout.EndScrollView();
            return scroll;
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            return Vector2.zero;
        }
    }

    private static bool BadChar(byte b)
    {
        return b < 0x21 || (b >= 0x7F && b <= 0xA0);
    }

    private static long Min(long a, long b)
    {
        if (a < b) return a;
        return b;
    }

    private struct TextData
    {
        public string index;
        public string center;
        public string preview;

        public TextData(string index, string center, string preview)
        {
            this.index = index;
            this.center = center;
            this.preview = preview;
        }

    }

    public enum DisplayType
    {
        ANSI,
        Byte,
        Byte2,
        Byte4,
        Byte8,
        Float,
        Double
    }
}
