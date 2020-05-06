using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShiningHill
{
    public class Identifier_SILENTFS : IdentifierBase
    {
        public Identifier_SILENTFS(byte id) : base(id) { }

        public override void Run(DirectoryEntry entries)
        {
            DirectoryBrowser browser = new DirectoryBrowser(entries);
            if(browser.Exists("/SYSTEM.CNF"))
            {
                if (browser.Exists("/SILENT."))
                {
                    browser.GetEntry("/SILENT.").specialFS = FileSystemBase.GetIdForType<SH1FileSystem>();
                }
                else if (browser.Exists("/SILENT"))
                {
                    browser.GetEntry("/SILENT").specialFS = FileSystemBase.GetIdForType<SH1FileSystem>();
                }
            }
        }
    }
}
