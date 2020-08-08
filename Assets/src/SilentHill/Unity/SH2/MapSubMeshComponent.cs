using UnityEngine;

using SH.GameData.SH2;
using UnityEditor;
using System.Collections.Generic;
using SH.Core;

namespace SH.Unity.SH2
{
    public class MapSubMeshComponent : MonoBehaviour
    {
        public FileGeometry.Geometry.MapMesh.MapSubMesh subMesh;

        /*void OnDrawGizmosSelected()
        {
            Matrix4x4 prevMatrix = Gizmos.matrix;
            Matrix4x4 prevMatrix2 = Handles.matrix;

            Gizmos.color = Color.white;
            if (subMesh != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Handles.matrix = transform.localToWorldMatrix;
                Mesh m = GetComponent<MeshFilter>().sharedMesh;
                CollectionPool.Request(out List<Vector3> verts);
                m.GetVertices(verts);
                for (int i = 0; i < verts.Count; i++)
                {
                    Gizmos.DrawSphere(verts[i], 10.0f);
                    Handles.Label(verts[i], i.ToString("X"));
                }
                CollectionPool.Return(ref verts);
            }
            Gizmos.matrix = prevMatrix;
            Handles.matrix = prevMatrix2;
        }*/
    }
}
