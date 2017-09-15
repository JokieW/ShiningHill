using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Test2))]
public class Test2Editor : Editor {

    Test2 test2
    {
        get
        {
            return (Test2)target;
        }
    }

    Vector3 v = new Vector3();
    public override void OnInspectorGUI()
    {
        v = EditorGUILayout.Vector3Field("Posi", v);
        if (GUILayout.Button("Global"))
        {
            test2.transform.position = v;
        }
        if (GUILayout.Button("Local"))
        {
            test2.transform.localPosition = v;
        }
    }

    void OnSceneGUI()
    {
        //test2.transform.localScale = Handles.ScaleHandle(test2.transform.localScale, test2.transform.localPosition, test2.transform.localRotation, 5.0f);
        test2.transform.localScale = new Vector3(Handles.ScaleSlider(test2.transform.localScale.x, test2.transform.localPosition, Vector3.right, test2.transform.localRotation, 2.0f, 0.0f), 1.0f, 1.0f);
        Handles.Disc(Quaternion.identity, Vector3.zero, Vector3.up, 5, false, 1);
    }
}
