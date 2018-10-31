using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ShiningHill
{
    public class FileBrowser : EditorWindow
    {
        [MenuItem("ShiningHill/File Explorer")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(FileBrowser));
        }

        static GUIStyle splitter;
        static Color darkLineColor;
        static Color lightLineColor;
        static Color blueSelectionColor;
        static Color unfocusedSelectionColor;
        static GUIStyle backgroundStyle;
        static GUIStyle foldoutStyle;

        SourcesView srcs = new SourcesView(260.0f, 100.0f, true);
        TreeView tree = new TreeView(260.0f, 100.0f, true);
        ContextView ctxt = new ContextView(500.0f, 100.0f, false);

        void SetSkin()
        {
            GUISkin skin = GUI.skin;

            splitter = new GUIStyle();
            splitter.normal.background = EditorGUIUtility.whiteTexture;
            splitter.stretchWidth = true;
            splitter.stretchHeight = true;
            splitter.margin = new RectOffset(0, 0, 0, 0);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("File Browser");
            darkLineColor = EditorGUIUtility.isProSkin ? new Color(0.1294f, 0.1294f, 0.1294f) : new Color(0.5098f, 0.5098f, 0.5098f);
            lightLineColor = EditorGUIUtility.isProSkin ? new Color(0.1608f, 0.1608f, 0.1608f) : new Color(0.6353f, 0.6353f, 0.6353f);
            blueSelectionColor = EditorGUIUtility.isProSkin ? new Color(0.2431f, 0.3725f, 0.5882f) : new Color(0.2235f, 0.4431f, 0.8980f);
            unfocusedSelectionColor = EditorGUIUtility.isProSkin ? new Color(0.2824f, 0.2824f, 0.2824f) : new Color(0.4235f, 0.4235f, 0.4235f);
            backgroundStyle = new GUIStyle { normal = { background = Texture2D.whiteTexture } };

            srcs.OnEnable();
            tree.OnEnable();
            ctxt.OnEnable();
        }

        private void OnDisable()
        {
            srcs.OnDisable();
            tree.OnDisable();
            ctxt.OnDisable();
        }

        void OnFocus()
        {
            srcs.OnFocus();
            tree.OnFocus();
            ctxt.OnFocus();
        }

        void OnLostFocus()
        {
            srcs.OnLostFocus();
            tree.OnLostFocus();
            ctxt.OnLostFocus();
        }

        void OnGUI()
        {
            UnityEngine.Profiling.Profiler.BeginSample("FileBrowser");
            if (splitter == null)
            {
                SetSkin();
                tree.sourceView = srcs;
            }

            GUILayout.BeginHorizontal();

            bool repaint = false;
            UnityEngine.Profiling.Profiler.BeginSample("Display");
            repaint = srcs.Display() || repaint;
            repaint = tree.Display() || repaint;
            repaint = ctxt.Display() || repaint;
            UnityEngine.Profiling.Profiler.EndSample();

            GUILayout.EndHorizontal();

            UnityEngine.Profiling.Profiler.BeginSample("Drag");
            repaint = srcs.UpdateDrag() || repaint;
            repaint = tree.UpdateDrag() || repaint;
            UnityEngine.Profiling.Profiler.EndSample();

            if (repaint)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Repaint");
                Repaint();
                UnityEngine.Profiling.Profiler.EndSample();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        class View
        {
            float _minWidth;
            float _width;
            bool _hasDrag;
            bool _dragging;
            Rect _dragRect;
            protected bool _hasFocus;

            public float width { get { return _width; } }

            public View(float baseWidth, float baseMinWidth, bool canDrag)
            {
                _width = baseWidth;
                _minWidth = baseMinWidth;
                _hasDrag = canDrag;
            }

            public bool Display()
            {
                if (_hasDrag)
                {
                    GUILayout.BeginVertical(
                        GUILayout.Width(_width),
                        GUILayout.MaxWidth(_width),
                        GUILayout.MinWidth(_width));
                }
                else
                {
                    GUILayout.BeginVertical();
                }

                bool repaint = Draw();

                GUILayout.EndVertical();

                if (_hasDrag)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    _dragRect = new Rect(rect.xMax, rect.y, 4, rect.height);
                    Rect drawRect = new Rect(rect.xMax, rect.y, 1, rect.height);
                    DrawSquare(drawRect, lightLineColor);
                    EditorGUIUtility.AddCursorRect(_dragRect, MouseCursor.ResizeHorizontal);
                    GUILayout.Space(4);
                }
                return repaint;
            }

            protected void DrawSquare(Rect rect, Color color)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    Color restoreColor = GUI.color;
                    GUI.color = color;
                    splitter.Draw(rect, false, false, false, false);
                    GUI.color = restoreColor;
                }
            }

            protected void TitleField(string text)
            {
                GUILayout.Space(2);
                EditorGUILayout.LabelField(text, EditorStyles.largeLabel);
                Rect rect = GUILayoutUtility.GetLastRect();
                DrawSquare(new Rect(rect.x, rect.yMax + 3, rect.xMax, 1), lightLineColor);
                GUILayout.Space(4);
            }

            public virtual bool Draw()
            {
                if (GUILayout.Button("エロい事が大好き"))
                {
                    System.Media.SystemSounds.Beep.Play();
                }
                return false;
            }

            public virtual void OnEnable() { }

            public virtual void OnDisable() { }

            public virtual void OnFocus()
            {
                _hasFocus = true;
            }

            public virtual void OnLostFocus()
            {
                _hasFocus = false;
            }

            public bool UpdateDrag()
            {
                bool result = false;
                if (Event.current != null)
                {
                    switch (Event.current.rawType)
                    {
                        case EventType.MouseDown:
                            if (_dragRect.Contains(Event.current.mousePosition))
                            {
                                _dragging = true;
                            }
                            break;
                        case EventType.MouseDrag:
                            if (_dragging)
                            {
                                _width += Event.current.delta.x;
                                if (_width < _minWidth) _width = _minWidth;
                                result = true;
                            }
                            break;
                        case EventType.MouseUp:
                            if (_dragging)
                            {
                                _dragging = false;
                            }
                            break;
                    }
                }
                return result;
            }
        }

        class ScrollableView : View
        {
            protected Vector2 _scrollPos;
            public ScrollableView(float baseWidth, float baseMinWidth, bool canDrag) : base(baseWidth, baseMinWidth, canDrag)
            { }
        }

        class SourcesView : ScrollableView
        {
            FileBrowserSources _sources;
            Listable list;

            SourceBase _currentSource;
            public SourceBase currentSource { get { return _currentSource; } }

            public SourcesView(float baseWidth, float baseMinWidth, bool canDrag) : base(baseWidth, baseMinWidth, canDrag)
            { }

            public override void OnFocus()
            {
                base.OnFocus();
                list.focused = _hasFocus;
            }

            public override void OnLostFocus()
            {
                base.OnLostFocus();
                list.focused = _hasFocus;
            }

            public override void OnEnable()
            {
                _sources = FileBrowserSources.GetSources();
                list = new Listable();
                list.debug = 1;
            }

            void SetCurrentSource(FileBrowserSources.SourceEntry se)
            {
                if (_currentSource != null)
                {
                    _currentSource.Close();
                }
                _currentSource = SourceBase.GetHandlerForID(se.handlerID).Instantiate(se.path);

            }

            public override bool Draw()
            {
                TitleField("Sources");
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false);

                bool repaint = false;
                for (int i = 0; i != _sources.sources.Count; i++)
                {
                    IListable sel1 = list.currentSelected;
                    FileBrowserSources.SourceEntry source = _sources.sources[i];
                    repaint = list.Display(source, 0) || repaint;
                    if (sel1 != list.currentSelected)
                    {
                        SetCurrentSource(source);
                    }
                }

                GUILayout.EndScrollView();
                if (GUILayout.Button("Add Source..."))
                {
                    FileBrowser_AddSource win = EditorWindow.GetWindow<FileBrowser_AddSource>(true);
                    win.position = new Rect(1000, 520, 400, 200);
                    win.titleContent = new GUIContent("Add Source...");
                }

                return repaint;
            }


        }

        class TreeView : ScrollableView
        {
            public TreeView(float baseWidth, float baseMinWidth, bool canDrag) : base(baseWidth, baseMinWidth, canDrag)
            { }

            public SourcesView sourceView;
            Listable list;

            public override void OnFocus()
            {
                base.OnFocus();
                list.focused = _hasFocus;

            }

            public override void OnLostFocus()
            {
                base.OnLostFocus();
                list.focused = _hasFocus;
            }

            public override void OnEnable()
            {
                list = new Listable();
                list.debug = 2;
            }

            public override bool Draw()
            {
                TitleField("Tree");
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false);

                bool repaint = false;
                SourceBase src = sourceView.currentSource;
                if (src != null)
                {
                    foreach (IListable i in src.GetDirectories())
                    {
                        repaint = list.Display(i, 0) || repaint;
                    }
                }

                GUILayout.EndScrollView();
                return repaint;
            }
        }

        class ContextView : View
        {
            public ContextView(float baseWidth, float baseMinWidth, bool canDrag) : base(baseWidth, baseMinWidth, canDrag)
            { }

            public override bool Draw()
            {
                TitleField("Context");
                return base.Draw();
            }
        }

        class Listable
        {
            public IListable currentSelected;
            public IListable currentFoldout;
            public bool focused;
            public int debug;

            public bool Display(IListable ilist, int level)
            {
                bool repaint = false;

                if (currentSelected == ilist)
                {
                    Color restoreColor = GUI.backgroundColor;
                    GUI.backgroundColor = focused ? blueSelectionColor : unfocusedSelectionColor;
                    GUILayout.BeginHorizontal(backgroundStyle);
                    GUI.backgroundColor = restoreColor;
                }
                else
                {
                    GUILayout.BeginHorizontal(new GUIStyle());
                }

                GUILayout.Space(level * 16);
                EditorGUILayout.LabelField("", GUILayout.Width(8), GUILayout.Height(10));
                Rect foldRect = GUILayoutUtility.GetLastRect();
                foldRect = new Rect(foldRect.x, foldRect.y, 14, 16);
                if (ilist.hasChildren && Event.current.type == EventType.Repaint)
                {
                    if (currentFoldout == ilist)
                    {
                        Debug.Log(EditorStyles.foldout.CalcScreenSize(new Vector2(16, 16)));
                        EditorStyles.foldout.Draw(foldRect, true, true, ilist.isExpanded, false);
                    }
                    else
                    {
                        EditorStyles.foldout.Draw(foldRect, false, false, ilist.isExpanded, false);
                    }

                }

                GUILayout.BeginVertical();

                ilist.Draw();

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                Rect fullRect = GUILayoutUtility.GetLastRect();

                if (ilist.isExpanded)
                {
                    foreach (IListable i in ilist)
                    {
                        repaint = Display(i, level + 1) || repaint;
                    }
                }

                if (Event.current != null)
                {
                    if (Event.current.rawType == EventType.MouseDown)
                    {
                        if (foldRect.Contains(Event.current.mousePosition))
                        {
                            repaint = true;
                            currentFoldout = ilist;
                        }
                        else if (fullRect.Contains(Event.current.mousePosition))
                        {
                            repaint = true;
                            currentSelected = ilist;
                        }
                    }
                    else if (Event.current.rawType == EventType.MouseUp)
                    {
                        if (foldRect.Contains(Event.current.mousePosition))
                        {
                            ilist.isExpanded = !ilist.isExpanded;
                        }
                        repaint = true;
                        currentFoldout = null;
                    }
                }

                return repaint;
            }
        }
    }

    public class FileBrowser_AddSource : EditorWindow
    {
        static readonly string[] FILE_FILTERS = new string[] { "Bin/Cue", "cue", "ISO Image", "iso" };

        string path = "";
        string sourceName = "";
        bool namePlayerSet = false;
        SourceBase.SourceHandler handler = null;

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Path", GUILayout.Width(40));
            path = GUILayout.TextField(path);
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select folder..."))
            {
                path = EditorUtility.OpenFolderPanel("Select game source", "", "");
            }
            if (GUILayout.Button("Select file..."))
            {
                path = EditorUtility.OpenFilePanelWithFilters("Select game source", "", FILE_FILTERS);
            }
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                handler = SourceBase.GetHandlerForPath(path);
                if (!namePlayerSet)
                {
                    sourceName = Path.GetFileNameWithoutExtension(path);
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Auto-Detect:");
            if (handler != null)
            {
                GUILayout.Label("    " + handler.description);
            }
            else
            {
                GUILayout.Label("Nothing detected");
            }

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Name", GUILayout.Width(40));
            sourceName = GUILayout.TextField(sourceName);
            if (EditorGUI.EndChangeCheck())
            {
                namePlayerSet = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(handler == null || string.IsNullOrEmpty(sourceName));
            if (GUILayout.Button("Add"))
            {
                FileBrowserSources.AddSource(path, sourceName, handler.id);
                this.Close();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}