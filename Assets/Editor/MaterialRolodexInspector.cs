using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{
    [CustomEditor(typeof(MaterialRolodex))]
	public class MaterialRolodexInspector :  Editor 
    {
        MaterialRolodex _rolodex;
        public MaterialRolodex rolodex
        {
            get
            {
                if (_rolodex == null)
                {
                    _rolodex = (MaterialRolodex)target;
                }
                return _rolodex;
            }
        }

        public override void OnInspectorGUI()
        {
            bool doCleanup = false;
            foreach (MaterialRolodex.TexMatsPair tmp in rolodex.texMatPairs)
            {
                GUILayout.Label("Pairs of "+tmp.textureName);
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                Texture2D newTex = (Texture2D)EditorGUILayout.ObjectField(tmp.texture, typeof(Texture2D), false, GUILayout.Height(64.0f), GUILayout.Width(64.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    if (newTex == null)
                    {
                        tmp.texture = null;
                        doCleanup = true;
                    }
                    else
                    {
                        EditorUtility.CopySerialized(newTex, tmp.texture);
                        AssetDatabase.SaveAssets();
                    }
                }

                EditorGUILayout.BeginVertical();
                if (tmp.diffuse == null)
                {
                    if (GUILayout.Button("Create diffuse"))
                    {
                        tmp.GetOrCreateDiffuse();
                    }
                }
                else
                {
                    EditorGUILayout.ObjectField("Diffuse", tmp.diffuse, typeof(Material), false);
                }

                if (tmp.transparent == null)
                {
                    if (GUILayout.Button("Create transparent"))
                    {
                        tmp.GetOrCreateTransparent();
                    }
                }
                else
                {
                    EditorGUILayout.ObjectField("Transparent", tmp.transparent, typeof(Material), false);
                }

                if (tmp.cutout == null)
                {
                    if (GUILayout.Button("Create cutout"))
                    {
                        tmp.GetOrCreateCutout();
                    }
                }
                else
                {
                    EditorGUILayout.ObjectField("Cutout", tmp.cutout, typeof(Material), false);
                }

                if (tmp.selfIllum == null)
                {
                    if (GUILayout.Button("Create self-illuminated"))
                    {
                        tmp.GetOrCreateSelfIllum();
                    }
                }
                else
                {
                    EditorGUILayout.ObjectField("Self Illuminated", tmp.selfIllum, typeof(Material), false);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10.0f);
            }

            if (doCleanup)
            {
                rolodex.Cleanup();
            }

            EditorGUI.BeginChangeCheck();
            Texture2D newnewTex = (Texture2D)EditorGUILayout.ObjectField("Add Texture", null, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (!String.IsNullOrEmpty(AssetDatabase.GetAssetPath(newnewTex)))
                {
                    Texture2D newnewnewTex = new Texture2D(newnewTex.width, newnewTex.height);
                    EditorUtility.CopySerialized(newnewTex, newnewnewTex);
                    rolodex.AddTexture(newnewnewTex);
                }
                else
                {
                    rolodex.AddTexture(newnewTex);
                }
                AssetDatabase.SaveAssets();
            }
        }
	}
}