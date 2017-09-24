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

            GZipInputStream gzipStream = new GZipInputStream(new FileStream(CustomPostprocessor.GetHardDataPathFor(SHGame.SH3PC) + "arc.arc", FileMode.Open, FileAccess.Read));

            gzipStream.Read(data, 0, 90016);

            ReadAll();
        }

        public void ReadAll()
        {
            CustomPostprocessor.DoImports = false;
            string hardAssetPath = CustomPostprocessor.GetHardDataPathFor(SHGame.SH3PC);
            try
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
                        arc = new Archive(name, AssetDatabase.LoadAssetAtPath(hardAssetPath + name + ".arc", typeof(UnityEngine.Object)));
                        arc.OpenArchive();
                    }

                    if (entryType == 3)
                    {
                        try
                        {
                            name = name.Replace("tmp", arc.NameAsPath());
                            string path = hardAssetPath + name;
                            path = Path.GetDirectoryName(path);
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            File.WriteAllBytes(hardAssetPath + name, arc.AllFiles[entryCount].data);
                        }
                        catch (Exception)
                        {
                            Debug.LogError("Problem at " + name + " [" + entryIndex + "](" + arc.AllFiles.Count + ") sub " + subType.ToString("X"));
                        }
                    }
                }
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            CustomPostprocessor.DoImports = true;
        }
	}
}