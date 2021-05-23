using System;
using System.IO;

using UnityEditor;

namespace SH.Unity.Shared
{
    public static class AssetUtil
    {
        private static bool _IsAssetEditing = false;
        public static void StartAssetEditing()
        {
            AssetDatabase.StartAssetEditing();
            _IsAssetEditing = true;
        }

        public static bool IsAssetEditing()
        {
            return _IsAssetEditing;
        }

        public static void ShortStopAssetEditing()
        {
            if (IsAssetEditing())
            {
                StopAssetEditing();
                StartAssetEditing();
            }
        }

        public static void StopAssetEditing()
        {
            AssetDatabase.StopAssetEditing();
            _IsAssetEditing = false;
        }

        //https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        public static bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, Func<string, float, bool> progressCallback)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];
                string temppath = Path.Combine(destDirName, file.Name);
                if (progressCallback(temppath, (float)i / (float)files.Length)) return true;
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, progressCallback);
                }
            }
            return false;
        }
    }
}
