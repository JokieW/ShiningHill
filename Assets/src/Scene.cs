using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SilentParty
{

    public class Scene : MonoBehaviour 
    {
#region Header
        //Header
        public int MainHeaderSegMarker; //Usually FFFFFFFF
        public int Unknown1;
        public int Unknown2;
        public int MainHeaderSize;
        public int TextureGroupOffset; //From start, leads to the marker of the texture group
        public int Unknown3;
        public int AltMainHeaderSize; //p?
        public int TotalMainHeaderSize; //p?
        public int Unknown4;
        public int SceneStartHeaderOffset; //p?
        public int Unknown5;
        public int Unknown6;
        public int TextureGroupOffset2; // Same as TextureGroupOffset AFAIK
        public int TransformOffset;  // From itself, leads to the end of vertices?
        public int Unknown7; //p? Called "SomeWeirdDataOffset"
        public int Unknown8;
        public short TotalTextures; //m?
        public short LocalTextureBaseIndex; //m?
        public short LocalTextureCount;
        public short Q1; //p? Called "q1"
        public int Unknown9;
        public int Unknown10;
#endregion

        public static Scene AttemptRecovery(Archive.ArcFile file)
        {
            try
            {
                GameObject go = new GameObject("Scene");
                Scene scene = go.AddComponent<Scene>();

                BinaryReader reader = new BinaryReader(new MemoryStream(file.data));

                //Header
                scene.MainHeaderSegMarker = reader.ReadInt32();
                scene.Unknown1 = reader.ReadInt32();
                scene.Unknown2 = reader.ReadInt32();
                scene.MainHeaderSize = reader.ReadInt32();
                scene.TextureGroupOffset = reader.ReadInt32();
                scene.Unknown3 = reader.ReadInt32();
                scene.AltMainHeaderSize = reader.ReadInt32();
                scene.TotalMainHeaderSize = reader.ReadInt32();
                scene.Unknown4 = reader.ReadInt32();
                scene.SceneStartHeaderOffset = reader.ReadInt32();
                scene.Unknown5 = reader.ReadInt32();
                scene.Unknown6 = reader.ReadInt32();
                scene.TextureGroupOffset2 = reader.ReadInt32();
                scene.TransformOffset = reader.ReadInt32();
                scene.Unknown7 = reader.ReadInt32();
                scene.Unknown8 = reader.ReadInt32();
                scene.TotalTextures = reader.ReadInt16();
                scene.LocalTextureBaseIndex = reader.ReadInt16();
                scene.LocalTextureCount = reader.ReadInt16();
                scene.Q1 = reader.ReadInt16();
                scene.Unknown9 = reader.ReadInt32();
                scene.Unknown10 = reader.ReadInt32();

                if (!Directory.Exists("Assets/Resources/Silent Hill 3/" + file.ArchiveName))
                {
                    Directory.CreateDirectory("Assets/Resources/Silent Hill 3/" + file.ArchiveName);
                }
                string finalPath = "Assets/Resources/Silent Hill 3/" + file.ArchiveName + "/" + file.FileNumber;
                if (!Directory.Exists(finalPath))
                {
                    Directory.CreateDirectory(finalPath);
                }

                Skybox sky = null;
                do
                {
                    sky = Skybox.Deserialise(reader, go);
                } while (sky.NextSkyboxOffset != 0);

                List<MeshGroup> groups = new List<MeshGroup>();
                do
                {
                    groups.Add(MeshGroup.Deserialise(reader, go, finalPath));
                } while (groups[groups.Count - 1].headers[0].NextSceneGeoOffset != 0);

                List<Texture2D> textures = new List<Texture2D>();

                reader.BaseStream.Position = scene.TextureGroupOffset;
                reader.SkipBytes(12);
                int texGroupLength = reader.ReadInt32();
                reader.SkipBytes(4);
                int texCount = reader.ReadInt32();
                reader.SkipBytes(8);

                for (int i = 0; i != texCount; i++)
                {
                    reader.SkipBytes(8);
                    short width = reader.ReadInt16();
                    short height = reader.ReadInt16();
                    reader.SkipByte();
                    byte buffer = reader.ReadByte();
                    reader.SkipBytes(2);
                    int lengthOfTex = reader.ReadInt32();
                    int nextDataRelativeOffset = reader.ReadInt32();
                    reader.SkipBytes(24+buffer);
                    List<Color32> _pixels = new List<Color32>(lengthOfTex / 4);
                    for(int j = 0; j != lengthOfTex ; j += 4)
                    {
                        _pixels.Add(reader.ReadColor32());
                    }
                    Texture2D text = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    text.SetPixels32(_pixels.ToArray());
                    text.Apply();
                    textures.Add(text);
                    _pixels.Clear();
                }

                Material defaultMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DefaultMaterial.mat");

                for (int i = 0; i != groups.Count; i++)
                {
                    Material mat = new Material(defaultMat);
                    mat.mainTexture = textures[i];

                    AssetDatabase.CreateAsset(textures[i], finalPath + "/texture_" + i + ".asset");
                    AssetDatabase.CreateAsset(mat, finalPath + "/material_" + i + ".mat");

                    MeshRenderer[] filters = groups[i].GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in filters)
                    {
                        mr.sharedMaterial = mat;
                    }
                }

                AssetDatabase.SaveAssets();

                GameObject root = GameObject.Find("World");
                if (root != null)
                {
                    root.transform.localScale = Vector3.one;
                    scene.transform.SetParent(root.transform);
                    root.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                }

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