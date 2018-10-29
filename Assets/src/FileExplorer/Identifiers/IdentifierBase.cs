using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShiningHill
{
    public class IdentifierBase
    {
        static IdentifierBase[] _archiveIdentifiers = new IdentifierBase[]
        {
            new IdentifierBase(0),
            new Identifier_SILENTFS(1)
        };

        static IdentifierBase[] _identifiers = new IdentifierBase[]
        {
            new IdentifierBase(0),
        };

        public static void RunArchiveIdentifiers(DirectoryEntry entries)
        {
            for (int i = 0; i != _archiveIdentifiers.Length; i++)
            {
                _archiveIdentifiers[i].Run(entries);
            }
        }

        protected byte _id;
        public byte id { get { return _id; } }
        public IdentifierBase(byte id) { _id = id; }

        public virtual byte GetTargetID() { return 0; }
        public virtual bool Run(DirectoryEntry entries) { return false; }
    }
}
