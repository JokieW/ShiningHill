using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum UnpackDirectory
{
    Workspace = 1,
    Proxy = 2,
    Unity = 3
}

public readonly struct UnpackPath
{
    private const string _unpackDirectory = "Assets/upk/";
    private const string _workspaceDirectory = "work/";
    private const string _proxyDirectory = "proxy/";
    private const string _unityDirectory = "unity/";

    public readonly string projectName;
    public readonly string relativePath;
    public readonly string name;
    private readonly string _directoryPath;
    private readonly string _fullPath;
    public readonly UnpackDirectory unpackDirectory;

    public string extension
    {
        get => Path.GetExtension(name);
    }

    public string nameWithoutExtension
    {
        get => Path.GetFileNameWithoutExtension(name);
    }

    public UnpackPath(string projectName, UnpackDirectory unpackDirectory, string relativePath, string name, bool makeSureDirectoryExists = false)
    {
        if (String.IsNullOrEmpty(projectName)) throw new ArgumentNullException(nameof(projectName));
        if (relativePath == null) throw new ArgumentNullException(nameof(relativePath));

        this.projectName = projectName;
        this.unpackDirectory = unpackDirectory;
        if (relativePath.Length > 0)
        {
            relativePath = relativePath.Replace('\\', '/');
            if (relativePath[relativePath.Length - 1] != '/')
            {
                relativePath += '/';
            }
        }
        this.relativePath = relativePath;
        this.name = name;

        this._directoryPath = string.Concat(_unpackDirectory,
            string.Concat(projectName + "/"),
            FolderTypeToString(unpackDirectory),
            relativePath);

        this._fullPath = this._directoryPath + name;

        if (makeSureDirectoryExists)
        {
            MakeSureFolderExists();
        }
    }

    public UnpackPath(string projectName, UnpackDirectory unpackDirectory, string mixedRelativePath, bool makeSureDirectoryExists = false)
    {
        if (String.IsNullOrEmpty(projectName)) throw new ArgumentNullException(nameof(projectName));
        if (mixedRelativePath == null) throw new ArgumentNullException(nameof(mixedRelativePath));

        this.projectName = projectName;
        this.unpackDirectory = unpackDirectory;
        if (mixedRelativePath.Length > 0)
        {
            mixedRelativePath = mixedRelativePath.Replace('\\', '/');
        }

        this.name = Path.GetFileName(mixedRelativePath);

        this.relativePath = mixedRelativePath.Substring(0, mixedRelativePath.Length - this.name.Length);

        this._directoryPath = string.Concat(_unpackDirectory,
            string.Concat(projectName + "/"),
            FolderTypeToString(unpackDirectory),
            relativePath);

        this._fullPath = this._directoryPath + name;

        if (makeSureDirectoryExists)
        {
            MakeSureFolderExists();
        }
    }

    public UnpackPath(string fullPath)
    {
        if (String.IsNullOrEmpty(fullPath))
        {
            throw new ArgumentException();
        }

        string strPath = fullPath.Replace('\\', '/');

        if (strPath.StartsWith(_unpackDirectory))
        {
            strPath = strPath.Substring(_unpackDirectory.Length);

            string projectName = strPath.Substring(0, strPath.IndexOf('/'));
            strPath = strPath.Substring(strPath.IndexOf('/') + 1);

            UnpackDirectory unpackDirectory;
            if (strPath.StartsWith(_workspaceDirectory))
            {
                unpackDirectory = UnpackDirectory.Workspace;
                strPath = strPath.Substring(_workspaceDirectory.Length);
            }
            else if (strPath.StartsWith(_proxyDirectory))
            {
                unpackDirectory = UnpackDirectory.Proxy;
                strPath = strPath.Substring(_proxyDirectory.Length);
            }
            else if (strPath.StartsWith(_unityDirectory))
            {
                unpackDirectory = UnpackDirectory.Proxy;
                strPath = strPath.Substring(_unityDirectory.Length);
            }
            else
            {
                throw new ArgumentException();
            }

            string name = Path.GetFileName(strPath);
            string relativePath = strPath.Substring(0, strPath.Length - name.Length);

            this.projectName = projectName;
            this.unpackDirectory = unpackDirectory;
            this.relativePath = relativePath;
            this.name = name;

            this._directoryPath = string.Concat(_unpackDirectory,
            string.Concat(projectName + "/"),
            FolderTypeToString(unpackDirectory),
            relativePath);

            this._fullPath = this._directoryPath + name;

            return;
        }
        throw new ArgumentException();
    }

    public static UnpackPath GetPath(UnityEngine.Object obj)
        => new UnpackPath(AssetDatabase.GetAssetPath(obj));

    public static UnpackPath GetDirectory(UnityEngine.Object obj)
        => new UnpackPath(AssetDatabase.GetAssetPath(obj)).GetDirectory();

    public static UnpackPath GetWorkspaceDirectory(string projectName, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, UnpackDirectory.Workspace, "", "", makeSureDirectoryExists);

    public static UnpackPath GetProxyDirectory(string projectName, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, UnpackDirectory.Proxy, "", "", makeSureDirectoryExists);

    public static UnpackPath GetUnityDirectory(string projectName, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, UnpackDirectory.Unity, "", "", makeSureDirectoryExists);

    public UnpackPath WithDirectory(UnpackDirectory newUnpackDirectory, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, newUnpackDirectory, relativePath, name, makeSureDirectoryExists);

    public UnpackPath WithPath(string newRelativePath, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, unpackDirectory, newRelativePath, name, makeSureDirectoryExists);

    public UnpackPath WithName(string newName, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, unpackDirectory, relativePath, newName, makeSureDirectoryExists);

    public UnpackPath WithDirectoryAndPath(UnpackDirectory newUnpackDirectory, string newRelativePath, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, newUnpackDirectory, newRelativePath, name, makeSureDirectoryExists);

    public UnpackPath WithDirectoryAndPathAndName(UnpackDirectory newUnpackDirectory, string newRelativePath, string newName, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, newUnpackDirectory, newRelativePath, newName, makeSureDirectoryExists);

    public UnpackPath WithDirectoryAndName(UnpackDirectory newUnpackDirectory, string newName, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, newUnpackDirectory, relativePath, newName, makeSureDirectoryExists);

    public UnpackPath WithPathAndName(string newRelativePath, string newName, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, unpackDirectory, newRelativePath, newName, makeSureDirectoryExists);

    public UnpackPath WithMixedPath(string newMixedRelativePath, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, unpackDirectory, newMixedRelativePath, makeSureDirectoryExists);

    public UnpackPath AddToPath(string relativePathAddition, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, unpackDirectory, relativePath + relativePathAddition, name, makeSureDirectoryExists);

    public UnpackPath AddToName(string nameAddition, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, unpackDirectory, relativePath, name + nameAddition, makeSureDirectoryExists);

    public UnpackPath AddToPathAndName(string relativePathAddition, string nameAddition, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, unpackDirectory, relativePath + relativePathAddition, name + nameAddition, makeSureDirectoryExists);

    public UnpackPath RemoveToPath(string relativePathDeletion, bool makeSureDirectoryExists = false)
        => new UnpackPath(projectName, unpackDirectory, relativePath.Replace(relativePathDeletion, ""), name, makeSureDirectoryExists);

    public bool IsFile()
        => !String.IsNullOrEmpty(name);

    public bool IsDirectory()
        => String.IsNullOrEmpty(name);

    public bool FileExists()
        => File.Exists(GetFullPath());

    public bool DirectoryExists()
        => Directory.Exists(GetDirectoryPath());

    public void MakeSureFolderExists()
    {
        string path = GetDirectoryPath();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static string FolderTypeToString(UnpackDirectory unpackDirectory)
    {
        if (unpackDirectory == UnpackDirectory.Workspace) return _workspaceDirectory;
        if (unpackDirectory == UnpackDirectory.Proxy) return _proxyDirectory;
        if (unpackDirectory == UnpackDirectory.Unity) return _unityDirectory;
        throw new InvalidDataException();
    }

    public UnpackPath GetDirectory()
    {
        return new UnpackPath(projectName, unpackDirectory, relativePath, "");
    }

    public string GetDirectoryPath()
    {
        return _directoryPath;
    }

    public string GetFullPath()
    {
        return _fullPath;
    }

    public string GetRelativePath()
    {
        return relativePath + name;
    }

    public override string ToString()
    {
        return GetFullPath();
    }

    public static implicit operator string(in UnpackPath path)
    {
        return path.ToString();
    }
}

public static class ExplorerUtil
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
        if(IsAssetEditing())
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
        for(int i = 0; i < files.Length; i++)
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
