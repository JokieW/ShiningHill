using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShiningHill
{
    public class SH1_BinCueSource : SourceBase
    {
        CDROMStream _stream;

        ~SH1_BinCueSource()
        {
            Close();
        }

        public override string description => "Silent Hill 1 .bin/.cue CD Image";

        public override void Init(string path)
        {
            _stream = CDROMStream.MakeFromCue(path);
            DirectoryEntry de;
            using (ISO9660FS fs = new ISO9660FS(_stream.MakeSubStream()))
            {
                de = fs.GetUniformDirectories();
            }
            PostProcessDirectories(de);
            SetDirectories(de);
        }

        protected override XStream GetStream()
        {
            return _stream;
        }

        public override void Close()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
        }

        public override bool DetectCompatibility(string path)
        {
            if (Path.GetExtension(path) == ".cue")
            {
                return true;
            }
            return false;
        }
    }
}
