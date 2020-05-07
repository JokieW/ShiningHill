using UnityEngine;

namespace SH.Unity.Shared
{
    public abstract class SourceHandler : ScriptableObject
    {
        public abstract void ImportSource();
        public abstract void ExportSource();
    }
}
