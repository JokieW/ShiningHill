﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ShiningHill
{

    public class Scene : MonoBehaviour 
    {
        public int Unknown1;
        public int Unknown2;
        public short TotalTextures;
        public short LocalTextureBaseIndex;
        public short LocalTextureCount;
        public short Unknown3;

        public static Scene ReadMap(string path)
        {
            GameObject go = new GameObject("Map");
            go.isStatic = true;
            try
            {
                Scene scene = go.AddComponent<Scene>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

                //Header
                reader.SkipInt32(-1); //marker
                reader.SkipInt32(0);
                reader.SkipInt32(0);
                reader.SkipInt32(80); //Main header size
                int TextureGroupOffset = reader.ReadInt32();
                reader.SkipInt32(0);
                reader.SkipInt32(80); //Alt main header size
                reader.SkipInt32(); //Total main header size
                scene.Unknown1 = reader.ReadInt32();
                reader.SkipInt32(); //Scene star header offset
                reader.SkipInt32(0);
                reader.SkipInt32(0);
                reader.SkipInt32(); //TextureGroupOffset2
                reader.SkipInt32(); //TransformOffset
                scene.Unknown2 = reader.ReadInt32();
                reader.SkipInt32(0);
                scene.TotalTextures = reader.ReadInt16();
                scene.LocalTextureBaseIndex = reader.ReadInt16();
                scene.LocalTextureCount = reader.ReadInt16();
                scene.Unknown3 = reader.ReadInt16();
                reader.SkipInt32(0);
                reader.SkipInt32(0);

                //Read textures
                long goBack = reader.BaseStream.Position;
                reader.BaseStream.Position = TextureGroupOffset;
                Texture2D[] textures = TextureUtils.ReadTex32("", reader);

                reader.BaseStream.Position = goBack;

                //Read Skyboxes
                Skybox sky = null;
                do
                {
                    sky = Skybox.Deserialise(reader, go);
                } while (sky.NextSkyboxOffset != 0);

                //Read meshgroups
                int next;
                do
                {
                    next = MeshGroup.Deserialise(reader, go);
                } while (next != 0);

                MeshGroup[] groups = go.GetComponentsInChildren<MeshGroup>();

                reader.Close();

                //Associate materials
                path = path.Replace(".map", ".prefab");
                UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(path);

                for (int i = 0; i != textures.Length; i++)
                {
                    Material mat;
                    mat = new Material(MaterialRolodex.defaultDiffuse);
                    mat.mainTexture = textures[i];

                    mat.name = Path.GetFileName(path).Replace(".prefab", "_mat_" + i);
                    textures[i].name = Path.GetFileName(path).Replace(".prefab", "_tex_" + i);

                    MeshRenderer[] filters = groups[groups.Length - textures.Length + i].GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in filters)
                    {
                        mr.sharedMaterial = mat;
                    }

                    AssetDatabase.AddObjectToAsset(textures[i], path);
                    AssetDatabase.AddObjectToAsset(mat, path);
                }

                foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>())
                {
                    AssetDatabase.AddObjectToAsset(mf.sharedMesh, path);
                }

                PrefabUtility.ReplacePrefab(go, prefab);
                
                AssetDatabase.SaveAssets();

                DestroyImmediate(go);

                return scene;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }
    }
}