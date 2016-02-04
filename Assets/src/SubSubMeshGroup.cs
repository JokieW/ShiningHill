using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ShiningHill
{
	public class SubSubMeshGroup : MonoBehaviour 
	{
        public int NextSceneGeoOffset;
        public int HeaderLength; //48
        public int Length;
        public int Unknown1;

        public int MainId;
        public int SubId;
        public int Unknown2;
        public int Unknown3;

        public float Unknown4;//48 and 64
        public float Unknown5;
        public float Unknown6;
        public float Unknown7;

        public static SubSubMeshGroup Deserialise(BinaryReader reader, GameObject parent, string path)
        {
            GameObject go = new GameObject("SubSubMesh Group");
            go.isStatic = true;
            SubSubMeshGroup group = go.AddComponent<SubSubMeshGroup>();
            go.transform.SetParent(parent.transform);

            group.NextSceneGeoOffset = reader.ReadInt32();
            group.HeaderLength = reader.ReadInt32();
            group.Length = reader.ReadInt32();
            group.Unknown1 = reader.ReadInt32();

            group.MainId = reader.ReadInt32();
            group.SubId = reader.ReadInt32();
            group.Unknown2 = reader.ReadInt32();
            group.Unknown3 = reader.ReadInt32();

            group.Unknown4 = reader.ReadSingle();
            group.Unknown5 = reader.ReadSingle();
            group.Unknown6 = reader.ReadSingle();
            group.Unknown7 = reader.ReadSingle();

            MeshPart meshpart = null;
            do
            {
                meshpart = MeshPart.Deserialise(reader, go, path);

            } while (meshpart.NextSceneGeoOffset != 0);


            return group;
        }
	}
}