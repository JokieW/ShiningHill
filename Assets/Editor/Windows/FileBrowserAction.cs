using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static ShiningHill.FileBrowserSources;

namespace ShiningHill
{
    public partial class FileBrowser
    {
        //
        //General
        //

        //
        //Sources
        //
        private void AddSourceButtonAction()
        {
            FileBrowser_AddSource win = EditorWindow.GetWindow<FileBrowser_AddSource>(true);
            win.listUpdateCallback = () => _sourcesView.Refresh();
            win.position = new Rect(1000, 520, 400, 200);
            win.titleContent = new GUIContent("Add Source...");
        }

        private VisualElement SourceListOnMakeItem()
        {
            return new Label();
        }

        private void SourceListOnBindItem(VisualElement e, int i)
        {
            (e as Label).text = _sources.sources[i].name;
        }

        private void OnSourceListSelectionChanged(List<object> selections)
        {
            SourceEntry se = (SourceEntry)selections[0];
            _currentSource = SourceBase.GetHandlerForID(se.handlerID).Instantiate(se.path);
        }

        //
        //Tree
        //

        //
        //Files
        //

        //
        //Selection
        //
    }
}
