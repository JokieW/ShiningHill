using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SilentParty
{
	public class Skybox : MonoBehaviour 
	{
        public int NextSkyboxOffset; //0 if no more
        public int HeaderLength; //32 for skybox
        public int SkyboxLength;
        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;
        public int Unknown5;
        public Matrix4x4 Matrix;
        public Vector3[] Vertices;

        public static Skybox Deserialise(BinaryReader reader, GameObject parent)
        {
            GameObject go = new GameObject("Skybox");
            Skybox sky = go.AddComponent<Skybox>();
            go.transform.SetParent(parent.transform);

            sky.NextSkyboxOffset = reader.ReadInt32();
            sky.HeaderLength = reader.ReadInt32();
            sky.SkyboxLength = reader.ReadInt32();
            sky.Unknown1 = reader.ReadInt32();

            sky.Unknown2 = reader.ReadInt32();
            sky.Unknown3 = reader.ReadInt32();
            sky.Unknown4 = reader.ReadInt32();
            sky.Unknown5 = reader.ReadInt32();

            sky.Matrix = reader.ReadMatrix4x4();

            List<Vector3> _verts = new List<Vector3>();
            for (int i = 0; i != 8; i++)
            {
                _verts.Add(reader.ReadVector3());
                reader.SkipInt32();
            }
            sky.Vertices = _verts.ToArray();

            return sky;
        }
	}
}