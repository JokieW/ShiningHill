using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShiningHill
{
    public class IdentifierBase
    {
        static IdentifierBase[] _identifiers = new IdentifierBase[]
        {
            new IdentifierBase(0),
            new Identifier_SILENTFS(1)
        };


        public static void RunIdentifiers(DirectoryEntry entries)
        {
            for (int i = 1; i != _identifiers.Length; i++)
            {
                _identifiers[i].Run(entries);
            }
        }

        protected byte _id;
        public byte id { get { return _id; } }
        public IdentifierBase(byte id) { _id = id; }
        
        public virtual void Run(DirectoryEntry entries) { }
    }
}
