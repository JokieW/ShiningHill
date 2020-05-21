using SH.Core;
using SH.GameData.Shared;
using SH.Native;
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

    [ContextMenu("doit2")]
    public void doit2()
    {
        Matrix4x4 m = Matrix4x4.identity;
        m[0] = 0.0f;
        m[1] = -0.73f;
        m[2] = 0.68f;
        m[3] = 0.0f;
        m[4] = 0.0f;
        m[5] = 0.68f;
        m[6] = 0.73f;
        m[7] = 0.0f;
        m[8] = -1.0f;
        m[9] = 0.0f;
        m[10] = 0.0f;
        m[11] = 0.0f;
        m[12] = 0.0f;
        m[13] = 0.0f;
        m[14] = -57.20f;
        m[15] = 1.0f;
        Vector3 pos = Matrix4x4Util.ExtractTranslationFromMatrix(in m);
        Quaternion rot = Matrix4x4Util.ExtractRotationFromMatrix(in m);
        Vector3 scale = Matrix4x4Util.ExtractScaleFromMatrix(in m);
        transform.localPosition = pos;
        transform.localRotation = rot;
        transform.localScale = scale;
    }

    [ContextMenu("doit3")]
    public void doit3()
    {
        Debug.Log(transform.localToWorldMatrix);
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
                    return MeshUtil.MakeStripped(pr, normals, uvs, colors);
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
