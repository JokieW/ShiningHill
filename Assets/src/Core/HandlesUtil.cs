using UnityEditor;
using UnityEngine;

namespace SH.Core
{
    public static class HandlesUtil
    {
        public static void DrawBoundingCube(Vector3 a, Vector3 b)
        {
            Vector3 a_xy_b_z = new Vector3(a.x, a.y, b.z);
            Vector3 a_xz_b_y = new Vector3(a.x, b.y, a.z);
            Vector3 a_yz_b_x = new Vector3(b.x, a.y, a.z);

            Vector3 b_xy_a_z = new Vector3(b.x, b.y, a.z);
            Vector3 b_xz_a_y = new Vector3(b.x, a.y, b.z);
            Vector3 b_yz_a_x = new Vector3(a.x, b.y, b.z);

            Handles.DrawLine(a, a_xy_b_z);
            Handles.DrawLine(a, a_xz_b_y);
            Handles.DrawLine(a, a_yz_b_x);

            Handles.DrawLine(b, b_xy_a_z);
            Handles.DrawLine(b, b_xz_a_y);
            Handles.DrawLine(b, b_yz_a_x);

            Handles.DrawLine(a_xy_b_z, b_xz_a_y);
            Handles.DrawLine(a_xy_b_z, b_yz_a_x);

            Handles.DrawLine(a_xz_b_y, b_xy_a_z);
            Handles.DrawLine(a_xz_b_y, b_yz_a_x);

            Handles.DrawLine(a_yz_b_x, b_xy_a_z);
            Handles.DrawLine(a_yz_b_x, b_xz_a_y);
        }
    }
}
