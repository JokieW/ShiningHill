using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;

namespace ShiningHill
{
	public class ArcFileSystem 
	{

        public byte[] data;

        public string[] stuff;
        public ArcFileSystem()
        {
            data = new byte[90016];

            GZipInputStream gzipStream = new GZipInputStream(new FileStream("Assets/Archives/arc.arc", FileMode.Open, FileAccess.Read));

            gzipStream.Read(data, 0, 90016);

            ReadAll();
        }

        public void ReadAll()
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(data));
            reader.SkipBytes(16);
            reader.SkipBytes(12);

            Archive arc = null;

            while (reader.BaseStream.Position <= 90000)
            {
                short entryType = reader.ReadInt16();
                short subType = reader.ReadInt16();
                short entryCount = reader.ReadInt16();
                short entryIndex = reader.ReadInt16();
                string name = reader.ReadNullTerminatedString();

                if (reader.PeekChar() == 0)
                {
                    reader.SkipByte();
                }

                if (entryType == 2)
                {
                    arc = new Archive(AssetDatabase.LoadAssetAtPath("Assets/Archives/" + name + ".arc", typeof(UnityEngine.Object)));
                    arc.OpenArchive();
                }

                if (entryType == 3)
                {
                    try
                    {
                        string path = "Assets/SilentHill3/Resources/" + name;
                        path = Path.GetDirectoryName(path);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        File.WriteAllBytes("Assets/SilentHill3/Resources/" + name, arc.AllFiles[entryCount].data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Problem at " + name + " [" + entryIndex + "](" + arc.AllFiles.Count+") sub "+subType.ToString("X"));
                    }
                }
            }
            AssetDatabase.Refresh();
        }
	}
}