using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityAssetUpdater
{
    public UnpackPath filePath;
    public List<SubFile> subFiles;

    public UnityAssetUpdater(in UnpackPath filePath)
    {
        if(!filePath.IsFile()) throw new ArgumentException("Path is not of a file.", nameof(filePath));

        this.filePath = filePath;
        subFiles = new List<SubFile>();
    }

    public class SubFile
    {
        public string name;
        public UnityEngine.Object file;

        public SubFile(string name, UnityEngine.Object file)
        {
            this.name = name;
            this.file = file;
        }
    }

    public void AddFile(string name, UnityEngine.Object file)
    {
        subFiles.Add(new SubFile(name, file));
    }

    public void WriteAssets()
    {

    }
}
