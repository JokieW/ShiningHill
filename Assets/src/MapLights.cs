using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace ShiningHill
{
	public class MapLights : MonoBehaviour 
	{
        public static MapLights ReadCollisions(string path)
        {
            /*string assetPath = path.Replace(".ded", ".asset");*/
            GameObject subGO = Map.BeginEditingPrefab(path, "Lights");

            try
            {
                MapLights cols = subGO.AddComponent<MapLights>();

                BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                


                reader.Close();

                Map.FinishEditingPrefab(path, subGO);

                return cols;

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }
	}
}