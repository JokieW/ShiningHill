﻿using System;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace SH.Editor
{
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

        private static string _goToAddressText = "";

        static HexDisplay.DisplayType _dispType = HexDisplay.DisplayType.Byte4;
        static bool _equalizePreview = true;
        static int _bytesPerRow = 16;
        static int _grouping = 4;
        static EndianDisplay _endian = EndianDisplay.Little;

        public unsafe static Vector2 Display(Vector2 scroll, byte[] data, GUILayoutOption[] layoutOptions)
        {
            try
            {
                float _dataWidth = ((_bytesPerRow * 2) + (_bytesPerRow / _grouping) - 1) * 10.25f;

                StringBuilder indexSB = new StringBuilder();
                StringBuilder centerSB = new StringBuilder();
                StringBuilder previewSB = new StringBuilder();


                long pointer = (long)scroll.y;
                int currentRow = 0;
                while (pointer < data.LongLength && currentRow <= _MAXROWS)
                {
                    indexSB.AppendFormat("{0:X8}", pointer);
                    long chunkSize = Min(_bytesPerRow, data.LongLength - pointer);
                    int curGrouping = 0;
                    int endianGroup = _endian == EndianDisplay.Big ? _grouping - 1 : 0;
                    for (long i = 0L; i != chunkSize; i++)
                    {
                        centerSB.AppendFormat("{0:X2}", data[pointer + i + endianGroup]);
                        curGrouping++;
                        if (_endian == EndianDisplay.Big) endianGroup -= 2;
                        if (curGrouping == _grouping && i != chunkSize - 1)
                        {
                            curGrouping = 0;
                            endianGroup = _endian == EndianDisplay.Big ? _grouping - 1 : 0;
                            centerSB.Append(" ");
                        }
                    }

                    fixed (byte* ptr = data)
                    {
                        for (long i = 0L; i != chunkSize; i++)
                        {
                            int length = 1;
                            string value = "BAD";
                            byte* dataptr = ptr + pointer + i;
                            long ptrRelative = (long)dataptr - (long)ptr;
                            if (_dispType == DisplayType.Byte)
                            {
                                if (ptrRelative >= data.Length) goto SKIP;
                                length = 4;
                                value = (*dataptr).ToString();
                            }
                            else if (_dispType == DisplayType.Byte2)
                            {
                                if (ptrRelative + 1L >= data.Length) goto SKIP;
                                length = 7;
                                value = (*(short*)dataptr).ToString();
                                i += 1;
                            }
                            else if (_dispType == DisplayType.Byte4)
                            {
                                if (ptrRelative + 3L >= data.Length) goto SKIP;
                                length = 12;
                                value = (*(int*)dataptr).ToString();
                                i += 3;
                            }
                            else if (_dispType == DisplayType.Byte8)
                            {
                                if (ptrRelative + 7L >= data.Length) goto SKIP;
                                length = 21;
                                value = (*(long*)dataptr).ToString();
                                i += 7;
                            }
                            else if (_dispType == DisplayType.Float)
                            {
                                if (ptrRelative + 3L >= data.Length) goto SKIP;
                                length = 14;
                                value = (*(float*)dataptr).ToString();
                                i += 3;
                            }
                            else if (_dispType == DisplayType.Double)
                            {
                                if (ptrRelative + 7L >= data.Length) goto SKIP;
                                length = 23;
                                value = (*(double*)dataptr).ToString();
                                i += 7;
                            }
                            else //ANSI
                            {
                                if (ptrRelative >= data.Length) goto SKIP;
                                length = 1;
                                byte b = *dataptr;
                                value = new string(BadChar(b) ? '.' : System.Convert.ToChar(b), 1);
                            }

                            SKIP:
                            if (length != 1)
                            {
                                if (_equalizePreview)
                                {
                                    char* str = stackalloc char[length];
                                    for (int j = length - 1, k = value.Length - 1; j != -1; j--, k--)
                                    {
                                        if (k >= 0) { str[j] = value[k]; } else { str[j] = ' '; }
                                    }
                                    value = new string(str, 0, length);
                                }
                                else
                                {
                                    value += " ";
                                }
                            }
                            previewSB.Append(value);
                        }
                    }

                    indexSB.Append("\n");
                    centerSB.Append("\n");
                    previewSB.Append("\n");

                    currentRow++;
                    pointer += chunkSize;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Offset (h)", GUILayout.Width(90.0f));
                GUILayout.Label("Data", GUILayout.Width(_dataWidth));
                GUILayout.Label("Preview");
                GUILayout.EndHorizontal();

                //Vector2 temp = EditorGUILayout.BeginScrollView(scroll, GUI.skin.horizontalScrollbar, GUIStyle.none);
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.TextArea(indexSB.ToString(), _style, GUILayout.Width(90.0f));
                GUILayout.TextArea(centerSB.ToString(), _style, GUILayout.Width(_dataWidth));
                GUILayout.TextArea(previewSB.ToString(), _style);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                _goToAddressText = GUILayout.TextField(_goToAddressText, _style, GUILayout.Width(90.0f));
                if (GUILayout.Button("Go", GUILayout.Width(50.0f)))
                {
                    long address = Convert.ToInt32(_goToAddressText, 16);
                    if (address < 0L)
                    {
                        address = 0L;
                    }
                    else if (address >= data.LongLength - (16L * (_MAXROWS - 1)))
                    {
                        address = data.LongLength - (16L * (_MAXROWS - 1));
                    }
                    else
                    {
                        address = (long)Mathf.Round((int)address / _bytesPerRow) * _bytesPerRow;
                    }
                    scroll.y = address;
                }

                GUILayout.Space(30.0f);
                _bytesPerRow = EditorGUILayout.IntPopup(_bytesPerRow, new string[] { "8 bytes per row", "16 bytes per row", "24 bytes per row", "32 bytes per row" }, new int[] { 8, 16, 24, 32 }, GUILayout.Width(120.0f));
                _grouping = EditorGUILayout.IntPopup(_grouping, new string[] { "1 byte per group", "2 bytes per group", "4 bytes per group", "8 bytes per group" }, new int[] { 1, 2, 4, 8 }, GUILayout.Width(120.0f));
                _endian = (EndianDisplay)EditorGUILayout.IntPopup((int)_endian, new string[] { "Little Endian", "Big Endian" }, new int[] { 0, 1 }, GUILayout.Width(100.0f));
                GUILayout.Space(60.0f);
                _dispType = (HexDisplay.DisplayType)EditorGUILayout.EnumPopup("Preview format", _dispType, GUILayout.Width(240.0f));
                _equalizePreview = EditorGUILayout.Toggle("Equalize Preview", _equalizePreview);

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                scroll.y = GUILayout.VerticalScrollbar(scroll.y, currentRow, 0.0f, Min(data.LongLength, data.LongLength - (16L * (_MAXROWS - 2))), GUILayout.ExpandHeight(true));
                scroll.y = scroll.y >= 0 ? Mathf.Round(scroll.y / _bytesPerRow) * _bytesPerRow : 0.0f;

                GUILayout.EndHorizontal();



                //EditorGUILayout.EndScrollView();
                return scroll;
            }
            catch (Exception e)
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

        public enum EndianDisplay
        {
            Little = 0,
            Big = 1
        }
    }
}
