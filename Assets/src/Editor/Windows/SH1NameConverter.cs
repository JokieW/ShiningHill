using System.Text.RegularExpressions;

using UnityEditor;

using SH.GameData.SH1;

namespace SH.Editor
{
    public class SH1NameConverter : EditorWindow
    {
        //static EditorWindow _currentWindow;

        [MenuItem("ShiningHill/SH1 Name Converter")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SH1NameConverter));
        }

        string hexbField = "00000000";
        string hexcField = "00000000";
        string nameField = "";
        void OnGUI()
        {
            EditorGUILayout.LabelField("SH1 Hex to name");
            hexbField = EditorGUILayout.TextField("Hex b", hexbField);
            hexbField = hexbField.Length <= 8 ? hexbField : hexbField.Substring(0, 8);
            hexbField = hexbField.ToUpper();
            hexbField = Regex.Replace(hexbField, @"[^A-F0-9]", "");
            uint resultb = 0;
            uint.TryParse(hexbField, System.Globalization.NumberStyles.HexNumber, null, out resultb);
            hexcField = EditorGUILayout.TextField("Hex c", hexcField);
            hexcField = hexcField.Length <= 8 ? hexcField : hexcField.Substring(0, 8);
            hexcField = hexcField.ToUpper();
            hexcField = Regex.Replace(hexcField, @"[^A-F0-9]", "");
            uint resultc = 0;
            uint.TryParse(hexcField, System.Globalization.NumberStyles.HexNumber, null, out resultc);
            EditorGUILayout.TextField("Name", Util.DecodeSH1Name(resultb, resultc));

            EditorGUILayout.Separator();

            /*EditorGUILayout.LabelField("Float to hex");
            floatField = EditorGUILayout.FloatField("Float", floatField);
            EditorGUILayout.TextField("Hex", DataUtils.SingleToHalfFloat(floatField).ToString("X"));*/
        }
    }
}
