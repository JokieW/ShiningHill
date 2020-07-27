using SH.GameData.Shared;
using SH.Unity.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataAnalyzer : MonoBehaviour
{
    [ContextMenu("Check Collisions")]
    public void CheckCollisions()
    {
        MapCollisionComponent[] colComps = GameObject.FindObjectsOfType<MapCollisionComponent>();

        foreach(MapCollisionComponent comp in colComps)
        {
            FileCollisions cols = comp.collisions;
            
            for(int i = 0; i < cols.group0Faces.Length; i++)
            {
                ref readonly CollisionFace quad = ref cols.group0Faces[i];
                if(quad.field_04 != 4)
                {
                    Debug.LogWarning("Group0 " + i + " non4 " + quad.field_04);
                }
            }
            for (int i = 0; i < cols.group1Faces.Length; i++)
            {
                ref readonly CollisionFace quad = ref cols.group1Faces[i];
                if (quad.field_04 != 4)
                {
                    Debug.LogWarning("Group1 " + i + " non4 " + quad.field_04);
                }
            }
            for (int i = 0; i < cols.group2Faces.Length; i++)
            {
                ref readonly CollisionFace quad = ref cols.group2Faces[i];
                if (quad.field_04 != 4)
                {
                    Debug.LogWarning("Group2 " + i + " non4 " + quad.field_04);
                }
            }
            for (int i = 0; i < cols.group3Faces.Length; i++)
            {
                ref readonly CollisionFace quad = ref cols.group3Faces[i];
                if (quad.field_04 != 4)
                {
                    Debug.LogWarning("Group3 " + i + " non4 " + quad.field_04);
                }
            }
            for (int i = 0; i < cols.group4Cylinders.Length; i++)
            {
                ref readonly CollisionCylinder cylinder = ref cols.group4Cylinders[i];
                if (cylinder.field_04 != 4)
                {
                    Debug.LogWarning("Group4 " + i + " non4 " + cylinder.field_04);
                }
            }
            Debug.Log("Done");
        }
    }
}
