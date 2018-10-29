using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShiningHill
{
    public interface IListable : IEnumerable<IListable>
    {
        bool isExpanded { get; set; }
        bool hasChildren { get; }
        void Draw();
    }
}