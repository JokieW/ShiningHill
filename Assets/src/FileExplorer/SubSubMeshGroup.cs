using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ShiningHill
{
	public class SubSubMeshGroup : MonoBehaviour 
	{
        public int Illumination; //9 ambient?, 8 self-illum, 4 unknown (i.e. bu1f), 0 no illum?

        public float Unknown1; //1
        public float Unknown2; //50 168
        public float Unknown3; //0
        public float Unknown4; //1

        public static int Deserialise(BinaryReader reader, GameObject parent)
        {
            GameObject go = new GameObject("SubSubMesh Group");
            go.isStatic = true;
            SubSubMeshGroup group = go.AddComponent<SubSubMeshGroup>();
            go.transform.SetParent(parent.transform);

            int NextOffset = reader.ReadInt32();
            reader.SkipInt32(48);
            reader.SkipInt32(); //Length
            reader.SkipInt32(0);

            group.Illumination = reader.ReadInt32();
            reader.SkipInt32(0);
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            group.Unknown1 = reader.ReadSingle();
            group.Unknown2 = reader.ReadSingle();
            group.Unknown3 = reader.ReadSingle();
            group.Unknown4 = reader.ReadSingle();

            int next;
            do
            {
                next = MeshPart.Deserialise(reader, go);

            } while (next != 0);


            return NextOffset;
        }
	}
}