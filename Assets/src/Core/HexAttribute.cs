using System;
using UnityEditor;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class HexAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(HexAttribute))]
public class HexIntDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUIStyle style = new GUIStyle(GUI.skin.textField);
        style.richText = true;

        string ox = EditorGUIUtility.isProSkin ? "<color=#ffffff50>0x</color>" : "<color=#00000060>0x</color>";
        string hex = ox + property.intValue.ToString("X");
        
        EditorGUI.BeginChangeCheck();
        string newHex = EditorGUI.TextField(position, property.displayName, hex, style);
        if(EditorGUI.EndChangeCheck())
        {
            int intValue = Convert.ToInt32(newHex, 16);
            property.intValue = intValue;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}

