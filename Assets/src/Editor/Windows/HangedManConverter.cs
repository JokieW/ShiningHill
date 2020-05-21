using UnityEngine;
using UnityEditor;

namespace SH.Editor
{
    public class HangedManConverter : EditorWindow
    {
        //static EditorWindow _currentWindow;

        [MenuItem("ShiningHill/HangedMan Converter")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(HangedManConverter));
        }

        ushort number = 0;
        void OnGUI()
        {
            if(GUILayout.Button("Gen"))
            {
                number = 0;
                int[] positions = new int[6];
                positions[0] = 0;
                positions[1] = 1;
                positions[2] = 2;
                positions[3] = 3;
                positions[4] = 4;
                positions[5] = 5;
                int i = 0;
                int j = 6;
                do
                {
                    int curProbIndex = i + (UnityEngine.Random.Range(0, int.MaxValue) % j);
                    int currentCandidate =  positions[i++];
                    --j;
                    number = (ushort)(positions[curProbIndex] + (6 * number));
                    positions[curProbIndex] = currentCandidate;
                }
                while (j > 0);
            }
            EditorGUILayout.LabelField("Number");
            number = (ushort)EditorGUILayout.IntField("Number", number);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Numbers");
            EditorGUI.BeginChangeCheck();
            int n1 = EditorGUILayout.IntField("1", GetNumberForX(1, number));
            int n2 = EditorGUILayout.IntField("2", GetNumberForX(2, number));
            int n3 = EditorGUILayout.IntField("3", GetNumberForX(3, number));
            int n4 = EditorGUILayout.IntField("4", GetNumberForX(4, number));
            int n5 = EditorGUILayout.IntField("5", GetNumberForX(5, number));
            int n6 = EditorGUILayout.IntField("6", GetNumberForX(6, number));

            if (EditorGUI.EndChangeCheck())
            {
                number = 0;
                int[] pos = new int[6];
                pos[0] = n1;
                pos[1] = n2;
                pos[2] = n3;
                pos[3] = n4;
                pos[4] = n5;
                pos[5] = n6;
                int ii = 0;
                do
                {
                    number = (ushort)(pos[ii++] + (6 * number));
                }
                while (ii < 6);
            }
            
           /* EditorGUILayout.LabelField("Positions");
            floatField = EditorGUILayout.FloatField("Float", floatField);
            EditorGUILayout.TextField("Hex", DataUtils.SingleToHalfFloat(floatField).ToString("X"));*/
        }

        int GetNumberForX(int x, int num)
        {
            for(int i = 0; i != 6 - x; i++)
            {
                num /= 6;
            }
            return num % 6;
        }
    }
}
