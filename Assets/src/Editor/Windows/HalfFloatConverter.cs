using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using SH.DataFormat.Shared;

namespace ShiningHill
{
    public class HalfFloatConverter : EditorWindow
    {
        //static EditorWindow _currentWindow;

        [MenuItem("ShiningHill/Half-float Converter")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(HalfFloatConverter));
        }

        string hexField = "0";
        float floatField = 0.0f;
        void OnGUI()
        {
            EditorGUILayout.LabelField("Hex to float");
            hexField = EditorGUILayout.TextField("Hex", hexField);
            hexField = hexField.Length <= 4 ? hexField : hexField.Substring(0, 4);
            hexField = hexField.ToUpper();
            hexField = Regex.Replace(hexField, @"[^A-F0-9]", "");
            ushort result = 0;
            ushort.TryParse(hexField, System.Globalization.NumberStyles.HexNumber, null, out result);
            EditorGUILayout.FloatField("Float", Util.HalfToSingleFloat(result));

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Float to hex");
            floatField = EditorGUILayout.FloatField("Float", floatField);
            EditorGUILayout.TextField("Hex", Util.SingleToHalfFloat(floatField).ToString("X"));
        }
    }
}