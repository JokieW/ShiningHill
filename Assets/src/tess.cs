using ShiningHill;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tess : MonoBehaviour
{
    [ContextMenu("doit")]
    public void doit()
    {
        Output = ConvertMesh(mesh);
        GetComponent<MeshFilter>().sharedMesh = Output;
    }

    public unsafe static Mesh ConvertMesh(Mesh input)
    {
        int[] indices = input.GetTriangles(0);
        ushort[] shortIndices = new ushort[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            shortIndices[i] = (ushort)indices[i];
        }
        NvTriStrip.SetStitchStrips(true);
        fixed (ushort* ptr = shortIndices)
        {
            NvTriStrip.GenerateStrips(ptr, (uint)shortIndices.Length, out NvTriStrip.PrimitiveGroup* prims, out ushort count, true);

            try
            {
                for (int i = 0; i != count; i++)
                {
                    NvTriStrip.PrimitiveGroup currentPrim = *(prims + i);
                    List<Vector3> pr = new List<Vector3>();
                    List<Vector3> normals = new List<Vector3>();
                    List<Vector2> uvs = new List<Vector2>();
                    List<Color32> colors = new List<Color32>();
                    Vector3[] prs = input.vertices;
                    Vector3[] normalss = input.normals;
                    Vector2[] uvss = input.uv;
                    //Color32[] colorss = input.colors32;
                    for (int j = 0; j != currentPrim.numIndices; j++)
                    {
                        int index = (int)*(currentPrim.indices + j);
                        pr.Add(prs[index]);
                        normals.Add(normalss[index]);
                        uvs.Add(uvss[index]);
                        //colors.Add(colorss[index]);
                    }
                    return MeshUtils.MakeStripped(pr, normals, uvs, colors);
                }
            }
            finally
            {
                NvTriStrip.DeletePrimitives(prims);
            }
        }
        return null;
    }

    public Mesh mesh;
    public Mesh Output;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
