using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SilentParty
{
	public class MeshGroup : MonoBehaviour 
	{
        public Header[] headers = new Header[3];

        public static MeshGroup Deserialise(BinaryReader reader, GameObject parent, string path)
        {
            GameObject go = new GameObject("Mesh Group");
            MeshGroup group = go.AddComponent<MeshGroup>();
            go.transform.SetParent(parent.transform);

            for (int i = 0; i != group.headers.Length; i++)
            {
                group.headers[i] = new Header(reader);
            }

            MeshPart result = null;
            do
            {
                result = MeshPart.Deserialise(reader, go, path);

            } while (result.NextSceneGeoOffset != 0);


            return group;
        }

        [Serializable]
        public struct Header
        {
            public int NextSceneGeoOffset;
            public int HeaderLength; //48
            public int Length;
            public int Unknown1;

            public int VertexCount; //Also id thing
            public int SubId;
            public int Unknown2;
            public int Unknown3;

            public int Unknown4;//48 and 64
            public int Unknown5;
            public int Unknown6;
            public int Unknown7;

            public Header(BinaryReader reader)
            {
                NextSceneGeoOffset = reader.ReadInt32();
                HeaderLength = reader.ReadInt32();
                Length = reader.ReadInt32();
                Unknown1 = reader.ReadInt32();

                VertexCount = reader.ReadInt32();
                SubId = reader.ReadInt32();
                Unknown2 = reader.ReadInt32();
                Unknown3 = reader.ReadInt32();

                Unknown4 = reader.ReadInt32();
                Unknown5 = reader.ReadInt32();
                Unknown6 = reader.ReadInt32();
                Unknown7 = reader.ReadInt32();
            }
        }
	}
}