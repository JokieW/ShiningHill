using System;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace SH.Unity.Shared
{
    public enum UnpackDirectory
    {
        Workspace = 1,
        Proxy = 2,
        Unity = 3
    }

    [Serializable]
    public struct UnpackPath
    {
        private const string _unpackDirectory = "Assets/upk/";
        private const string _workspaceDirectory = "work/";
        private const string _proxyDirectory = "proxy/";
        private const string _unityDirectory = "unity/";

        [SerializeField]
        private string _projectName;
        [SerializeField]
        private string _relativePath;
        [SerializeField]
        private string _name;
        [SerializeField]
        private string _directoryPath;
        [SerializeField]
        private string _fullPath;
        [SerializeField]
        private UnpackDirectory _unpackDirectoryEnum;

        public string projectName
        {
            get => _projectName;
        }

        public string relativePath
        {
            get => _relativePath;
        }

        public string name
        {
            get => _name;
        }

        public UnpackDirectory unpackDirectoryEnum
        {
            get => _unpackDirectoryEnum;
        }

        public string extension
        {
            get => Path.GetExtension(_name);
        }

        public string nameWithoutExtension
        {
            get => Path.GetFileNameWithoutExtension(_name);
        }

        public UnpackPath(string projectName, UnpackDirectory unpackDirectory, string relativePath, string name, bool makeSureDirectoryExists = false)
        {
            if (String.IsNullOrEmpty(projectName)) throw new ArgumentNullException(nameof(projectName));
            if (relativePath == null) throw new ArgumentNullException(nameof(relativePath));

            this._projectName = projectName;
            this._unpackDirectoryEnum = unpackDirectory;
            if (relativePath.Length > 0)
            {
                relativePath = relativePath.Replace('\\', '/');
                if (relativePath[relativePath.Length - 1] != '/')
                {
                    relativePath += '/';
                }
            }
            this._relativePath = relativePath;
            this._name = name;

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

            this._projectName = projectName;
            this._unpackDirectoryEnum = unpackDirectory;
            if (mixedRelativePath.Length > 0)
            {
                mixedRelativePath = mixedRelativePath.Replace('\\', '/');
            }

            this._name = Path.GetFileName(mixedRelativePath);

            this._relativePath = mixedRelativePath.Substring(0, mixedRelativePath.Length - this._name.Length);

            this._directoryPath = string.Concat(_unpackDirectory,
                string.Concat(projectName + "/"),
                FolderTypeToString(unpackDirectory),
                _relativePath);

            this._fullPath = this._directoryPath + _name;

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

                this._projectName = projectName;
                this._unpackDirectoryEnum = unpackDirectory;
                this._relativePath = relativePath;
                this._name = name;

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
            => new UnpackPath(_projectName, newUnpackDirectory, _relativePath, _name, makeSureDirectoryExists);

        public UnpackPath WithPath(string newRelativePath, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, _unpackDirectoryEnum, newRelativePath, _name, makeSureDirectoryExists);

        public UnpackPath WithName(string newName, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, _unpackDirectoryEnum, _relativePath, newName, makeSureDirectoryExists);

        public UnpackPath WithDirectoryAndPath(UnpackDirectory newUnpackDirectory, string newRelativePath, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, newUnpackDirectory, newRelativePath, _name, makeSureDirectoryExists);

        public UnpackPath WithDirectoryAndPathAndName(UnpackDirectory newUnpackDirectory, string newRelativePath, string newName, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, newUnpackDirectory, newRelativePath, newName, makeSureDirectoryExists);

        public UnpackPath WithDirectoryAndName(UnpackDirectory newUnpackDirectory, string newName, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, newUnpackDirectory, _relativePath, newName, makeSureDirectoryExists);

        public UnpackPath WithPathAndName(string newRelativePath, string newName, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, _unpackDirectoryEnum, newRelativePath, newName, makeSureDirectoryExists);

        public UnpackPath WithMixedPath(string newMixedRelativePath, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, _unpackDirectoryEnum, newMixedRelativePath, makeSureDirectoryExists);

        public UnpackPath AddToPath(string relativePathAddition, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, _unpackDirectoryEnum, _relativePath + relativePathAddition, _name, makeSureDirectoryExists);

        public UnpackPath AddToName(string nameAddition, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, _unpackDirectoryEnum, _relativePath, _name + nameAddition, makeSureDirectoryExists);

        public UnpackPath AddToPathAndName(string relativePathAddition, string nameAddition, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, _unpackDirectoryEnum, _relativePath + relativePathAddition, _name + nameAddition, makeSureDirectoryExists);

        public UnpackPath RemoveToPath(string relativePathDeletion, bool makeSureDirectoryExists = false)
            => new UnpackPath(_projectName, _unpackDirectoryEnum, _relativePath.Replace(relativePathDeletion, ""), _name, makeSureDirectoryExists);

        public bool IsFile()
            => !String.IsNullOrEmpty(_name);

        public bool IsDirectory()
            => String.IsNullOrEmpty(_name);

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
            return new UnpackPath(_projectName, _unpackDirectoryEnum, _relativePath, "");
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
            return _relativePath + _name;
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
}
