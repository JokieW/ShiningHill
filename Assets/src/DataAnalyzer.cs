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
            
            for(int i = 0; i < cols.group0Quads.Length; i++)
            {
                ref readonly CollisionQuad quad = ref cols.group0Quads[i];
                if(quad.field_04 != 4)
                {
                    Debug.LogWarning("Group0 " + i + " non4 " + quad.field_04);
                }
            }
            for (int i = 0; i < cols.group1Quads.Length; i++)
            {
                ref readonly CollisionQuad quad = ref cols.group1Quads[i];
                if (quad.field_04 != 4)
                {
                    Debug.LogWarning("Group1 " + i + " non4 " + quad.field_04);
                }
            }
            for (int i = 0; i < cols.group2Quads.Length; i++)
            {
                ref readonly CollisionQuad quad = ref cols.group2Quads[i];
                if (quad.field_04 != 4)
                {
                    Debug.LogWarning("Group2 " + i + " non4 " + quad.field_04);
                }
            }
            for (int i = 0; i < cols.group3Quads.Length; i++)
            {
                ref readonly CollisionQuad quad = ref cols.group3Quads[i];
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
