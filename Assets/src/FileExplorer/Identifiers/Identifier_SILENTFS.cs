using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShiningHill
{
    public class Identifier_SILENTFS : IdentifierBase
    {
        public Identifier_SILENTFS(byte id) : base(id) { }

        public override byte GetTargetID()
        {
            return FileSystemBase.GetIdForType<SH1FileSystem>();
        }

        public override bool Run(DirectoryEntry entries)
        {
            DirectoryBrowser browser = new DirectoryBrowser(entries);
            return browser.Exists("/SYSTEM.CNF") && (browser.Exists("/SILENT.") || browser.Exists("/SILENT"));
        }
    }
}
