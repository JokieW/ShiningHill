using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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

                Skybox sky = null;
                do
                {
                    sky = Skybox.Deserialise(reader, go);
                } while (sky.NextSkyboxOffset != 0);

                MeshGroup group = null;
                do
                {
                    group = MeshGroup.Deserialise(reader, go);
                } while (group.headers[0].NextSceneGeoOffset != 0);

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