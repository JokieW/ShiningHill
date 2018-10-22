using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SH1_BinCueSource : SourceHandler
{
    ISO9660FS fs;
    
    ~SH1_BinCueSource()
    {
        Close();
    }

    public override string description
    {
        get
        {
            return "Silent Hill 1 .bin/.cue CD Image";
        }
    }

    public override void Init(string path)
    {
        fs = new ISO9660FS(CDROMStream.MakeFromCue(path));
    }

    public override void Close()
    {
        if (fs != null)
        {
            fs.Close();
            fs = null;
        }
    }

    public static SourceHandler DetectCompatibility(string path)
    {
        if(Path.GetExtension(path) == ".cue")
        {
            return new SH1_BinCueSource();
        }
        return null;
    }

    public override DirectoryEntry GetDirectories()
    {
        return fs.GetUniformDirectories();
    }
}
