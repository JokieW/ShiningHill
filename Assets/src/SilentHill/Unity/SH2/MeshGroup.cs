using System.IO;

using UnityEngine;

namespace SH.Unity.SH2
{
	public class MeshGroup
	{
        public int MainID;
        public int Unknown1;
        public int Unknown2;

        public short indexCount;
        public short Unknown4;
        public short firstVertex;
        public short lastVertex;

        public static MeshGroup Initialise(BinaryReader reader, GameObject parent)
        {
            MeshGroup group = new MeshGroup();
            group.MainID = reader.ReadInt32();
            group.Unknown1 = reader.ReadInt32();
            group.Unknown2 = reader.ReadInt32();
            group.indexCount = reader.ReadInt16();
            group.Unknown4 = reader.ReadInt16();
            group.firstVertex = reader.ReadInt16();
            group.lastVertex = reader.ReadInt16();

            return group;
        }

        /*public void DeserialiseMesh(BinaryReader reader, List<Vector3> verts, List<Vector3> norms, List<Color32> colors, List<Vector2> uvs)
        {
            List<Vector3> newVerts = verts.GetRange(firstVertex, lastVertex - firstVertex);
            List<Vector3> newNorms = norms.GetRange(firstVertex, lastVertex - firstVertex);
            List<Color32> newColors = null;
            if (colors.Count > 0)
            {
                newColors = colors.GetRange(firstVertex, lastVertex - firstVertex);
            }
            List<Vector2> newUVs = uvs.GetRange(firstVertex, lastVertex - firstVertex);

            short[] indices = new short[indexCount / 2];
            for (int i = 0; i != indexCount / 2; i++)
            {
                short ind = reader.ReadInt16();
                indices[i] = (short)(ind - firstVertex);
            }

            gameObject.AddComponent<MeshRenderer>().sharedMaterial = MaterialRolodex.defaultDiffuse;
            gameObject.AddComponent<MeshFilter>().sharedMesh =  MeshUtils.MakeIndexStrip(newVerts, indices, newNorms, newUVs, colors.Count == 0 ? null : newColors);
        }*/

       /* public static int Deserialise(BinaryReader reader, GameObject parent)
        {
            GameObject go = new GameObject("Mesh Group SH2");
            go.isStatic = true;
            MeshGroupSH2 group = go.AddComponent<MeshGroupSH2>();
            go.transform.SetParent(parent.transform);



            return 0;// NextOffset;
        }*/
	}
}
