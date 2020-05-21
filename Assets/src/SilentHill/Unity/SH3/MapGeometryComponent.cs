using UnityEngine;

using SH.GameData.SH3;

namespace SH.Unity.SH3
{
    public class MapGeometryComponent : MonoBehaviour
    {
        public MapGeometry geometry;
        public MapGeometry.Header header;
        public Matrix4x4[] eventMatrices;

        //fix with new pipeline
#if false
        static void ReadSH2Map(BinaryReader reader, Map scene, MapAssetPaths paths)
        {
            GameObject subGO = scene.gameObject;

            /*int fileID = */reader.ReadInt32();
            /*int fileSize = */reader.ReadInt32();
            /*int Unknown1 = */reader.ReadInt32();
            reader.SkipInt32(0);

            //Textures
            Texture[] textures = TextureUtils.ReadDDS(paths.GetTextureName(), reader);


            //Meshes
            reader.SkipInt32(1);
            reader.SkipInt32(); //Length from magic to bottom
            reader.SkipInt32(0);
            reader.SkipInt32(0);

            long magicPosition = reader.BaseStream.Position;
            reader.SkipInt32(0x20010730);
            reader.SkipInt32(1);
            int materialsOffset = reader.ReadInt32() + (int)magicPosition; 
            int meshCount = reader.ReadInt32();

            reader.SkipInt32(0);
            reader.SkipInt32(); // Length of elements from 0^
            reader.SkipInt32(20);
            reader.SkipInt32(0);

            reader.SkipInt32(0);
            reader.SkipInt32(1);
            reader.SkipInt32(8);
            
            long v1offset = reader.BaseStream.Position;
            reader.ReadVector3(); //V1
            reader.SkipInt32(0);
            reader.ReadVector3(); //V2
            reader.SkipInt32(0);
            /*int headerLength = */reader.ReadInt32(); //From v1 to vertexLength

            int indicesOffset = reader.ReadInt32() + (int)v1offset;
            /*int indicesLength = */reader.ReadInt32();
            /*int Unknown = */reader.ReadInt32();
            reader.SkipInt32(meshCount);

            List<MeshGroupSH2> groups = new List<MeshGroupSH2>();
            for (int i = 0; i != meshCount; i++)
            {
                groups.Add(MeshGroupSH2.Initialise(reader, subGO));
            }

            int vertexLength = reader.ReadInt32();
            reader.SkipInt32(1);
            reader.SkipInt32(0);
            int elementLength = reader.ReadInt32();
            reader.SkipInt32(vertexLength);

            int vertexElementsCount = vertexLength / elementLength;
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Color32> colors = new List<Color32>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i != vertexElementsCount; i++)
            {
                verts.Add(reader.ReadVector3());
                norms.Add(reader.ReadVector3());
                if (elementLength == 36)
                {
                    colors.Add(reader.ReadBGRA());
                }
                uvs.Add(reader.ReadVector2());
            }

            reader.BaseStream.Position = indicesOffset;

            List<short[]> indices = new List<short[]>(groups.Count);

            //stupid
            for (int i = 0; i != groups.Count; i++)
            {
                indices.Add(null);
            }

            for(int i = 0; i != groups.Count; i++)
            {
                MeshGroupSH2 group = groups[i];
                indices[group.MainID] = new short[group.indexCount];
                for (int j = 0; j != group.indexCount ; j++)
                {
                    indices[group.MainID][j] = reader.ReadInt16();
                }
                Debug.Log("End of i = " + reader.BaseStream.Position.ToString("X"));
            }

            reader.BaseStream.Position = materialsOffset;

            //Mesh renderer
            MaterialRolodex rolodex = MaterialRolodex.GetOrCreateAt(paths.GetExtractAssetPath());
            rolodex.AddTextures(textures);
            

            MeshRenderer mr = subGO.AddComponent<MeshRenderer>();
            Material[] mats = new Material[groups.Count];
            for (int i = 0; i != groups.Count; i++)
            {
                reader.SkipInt16();
                MaterialRolodex.TexMatsPair tmp = rolodex.GetWithSH2ID(reader.ReadInt16());
                mats[i] = tmp.GetOrCreateDiffuse();
                reader.SkipBytes(12);
            }
            mr.sharedMaterials = mats;

            reader.Close();

            //Mesh filter
            subGO.AddComponent<MeshFilter>().sharedMesh = MeshUtils.MakeIndexedStrip(verts, indices, norms, uvs, colors);

            foreach (MeshFilter mf in subGO.GetComponentsInChildren<MeshFilter>())
            {
                AssetDatabase.AddObjectToAsset(mf.sharedMesh, paths.GetExtractAssetPath());
            }
        }
#endif
    }
}
