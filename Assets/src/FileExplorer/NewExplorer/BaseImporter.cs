using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class SourceHandler : ScriptableObject
{
    public abstract void ImportSource();
    public abstract void ExportSource();
}
