using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ShiningHill
{
	public class SubMeshGroup : MonoBehaviour 
	{
        public short Unknown11;
        public short IsTransparent;

        public static int Deserialise(BinaryReader reader, GameObject parent)
        {
            GameObject go = new GameObject("SubMesh Group");
            go.isStatic = true;
            SubMeshGroup group = go.AddComponent<SubMeshGroup>();
            go.transform.SetParent(parent.transform);

            int NextOffset = reader.ReadInt32();
            reader.SkipInt32(48);
            reader.SkipInt32(); //Length
            reader.SkipInt32(0);

            group.Unknown11 = reader.ReadInt16();
            reader.SkipInt16(1);
            reader.SkipInt16(0);
            group.IsTransparent = reader.ReadInt16();
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            reader.SkipBytes(16, 0);

            int next;
            do
            {
                next = SubSubMeshGroup.Deserialise(reader, go);
            } while (next != 0);


            return NextOffset;
        }
	}
}