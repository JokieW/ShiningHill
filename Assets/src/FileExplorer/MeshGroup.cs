using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ShiningHill
{
	public class MeshGroup : MonoBehaviour 
	{
        public int TextureGroup;
        public int TextureIndex;
        public int Unknown1;

        public static int Deserialise(BinaryReader reader, GameObject parent)
        {
            GameObject go = new GameObject("Mesh Group");
            go.isStatic = true;
            MeshGroup group = go.AddComponent<MeshGroup>();
            go.transform.SetParent(parent.transform);

            int NextOffset = reader.ReadInt32();
            reader.SkipInt32(48);
            reader.SkipInt32(); // Length
            reader.SkipInt32(0);

            group.TextureGroup = reader.ReadInt32();
            group.TextureIndex = reader.ReadInt32();
            group.Unknown1 = reader.ReadInt32();
            reader.SkipInt32(0);

            reader.SkipBytes(16, 0);

            int next;
            do
            {
                next = SubMeshGroup.Deserialise(reader, go);
            } while (next != 0);

            return NextOffset;
        }
	}
}