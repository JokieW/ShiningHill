using UnityEditor;
using UnityEngine;

using SH.Unity.Shared;

namespace SH.Unity.SH2
{
    [CanEditMultipleObjects]
    public abstract class RootFolderProxy : BaseImportProxy
    {
        protected string rootFolderName;
        protected UnpackPath workFolderPath;

        public void SetFolder(string rootFolderName, UnpackPath rootFolderPath)
        {
            this.rootFolderName = rootFolderName;
            this.workFolderPath = rootFolderPath;
        }
    }
}