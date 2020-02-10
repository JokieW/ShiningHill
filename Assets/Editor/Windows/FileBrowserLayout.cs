using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShiningHill
{
    public partial class FileBrowser
    {
        private void OnEnableLayout()
        {
            VisualElement root = this.rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            {
                VisualElement grip_SourceTree;
                VisualElement grip_treeFiles;
                VisualElement grip_filesContext;

                //Source, Tree and Files
                VisualElement sourceTreeFilesBox = new VisualElement()
                {
                    name = "SourceTreeFilesBox",
                    style = {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    overflow = Overflow.Hidden
                    }
                };
                {
                    //
                    //Sources
                    //
                    VisualElement sourcesBox = new VisualElement()
                    {
                        name = "SourcesBox",
                        style = {
                        flexGrow = 1,
                        minWidth = 150
                        }
                    };
                    {
                        sourcesBox.Add(new Label("Sources")
                        {
                            name = "SourcesLabel",
                            style = {
                            fontSize = new StyleLength(16),
                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginLeft = 5,
                            marginTop = 5,
                            marginBottom = 5
                            }
                        });

                        _sourcesView = new ListView(_sources.sources, 16, SourceListOnMakeItem, SourceListOnBindItem)
                        {
                            name = "SourcesListView",
                            style = {
                            borderTopWidth = 2,
                            borderColor = darkLineColor,
                            flexDirection = FlexDirection.Column,
                            flexGrow = 1
                            }
                        };
                        sourcesBox.Add(_sourcesView);
                        _sourcesView.onSelectionChanged += OnSourceListSelectionChanged;

                        sourcesBox.Add(new Button(AddSourceButtonAction)
                        {
                            name = "AddSourceButton",
                            text = "Add Source..."
                        });
                        sourceTreeFilesBox.Add(sourcesBox);
                    }

                    //
                    //Tree
                    //
                    VisualElement treeBox = new VisualElement()
                    {
                        name = "TreeBox",
                        style = {
                        flexGrow = 1,
                        flexDirection = FlexDirection.Row,
                        minWidth = 150
                    }
                    };
                    {
                        VisualElement border = new VisualElement()
                        {
                            name = "Source_TreeSeparator",
                            style = {
                            width = 5,
                            maxWidth = 5,
                            minWidth = 5,
                            backgroundColor = darkLineColor,
                        }
                        };
                        border.Add(grip_SourceTree = new VisualElement()
                        {
                            style =
                        {
                            width = 5,
                            maxWidth = 5,
                            minWidth = 5,
                            flexGrow = 1,
                        }
                        });
                        treeBox.Add(border);

                        VisualElement innerbox = new VisualElement()
                        {
                            name = "Inner box",
                            style = {
                                flexGrow = 1,
                            }
                        };
                        treeBox.Add(innerbox);

                        innerbox.Add(new Label("Tree")
                        {
                            name = "TreeLabel",
                            style = {
                            fontSize = new StyleLength(16),
                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginLeft = 5,
                            marginTop = 5,
                            marginBottom = 5
                            }
                        });

                        //_treeView = new VisualElement.Hierarchy(); // here boi
                        /*{
                            name = "TreeListView",
                            style = {
                            borderTopWidth = 1,
                            borderColor = darkLineColor,
                            flexDirection = FlexDirection.Column,
                            flexGrow = 1
                            }
                        };*/
                        innerbox.Add(_treeView);
                        sourceTreeFilesBox.Add(treeBox);
                    }

                    //
                    //Files
                    // 
                    VisualElement filesBox = new VisualElement()
                    {
                        name = "FilesBox",
                        style = {
                        flexGrow = 1,
                        flexDirection = FlexDirection.Row,
                        minWidth = 150
                        }
                    };
                    {
                        VisualElement border = new VisualElement()
                        {
                            name = "Tree_FilesSeparator",
                            style = {
                            width = 5,
                            maxWidth = 5,
                            minWidth = 5,
                            backgroundColor = darkLineColor,
                            }
                        };
                        border.Add(grip_treeFiles = new VisualElement()
                        {
                            style =
                        {
                            width = 5,
                            maxWidth = 5,
                            minWidth = 5,
                            flexGrow = 1,
                        }
                        });
                        filesBox.Add(border);

                        VisualElement innerbox = new VisualElement()
                        {
                            name = "Inner box",
                            style = {
                                flexGrow = 1,
                            }
                        };
                        filesBox.Add(innerbox);

                        innerbox.Add(new Label("Files")
                        {
                            style = {
                            fontSize = new StyleLength(16),
                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginLeft = 5,
                            marginTop = 5,
                            marginBottom = 5
                            }
                        });

                        _filesView = new ListView()
                        {
                            name = "FilesListView",
                            style = {
                            borderTopWidth = 1,
                            borderColor = darkLineColor,
                            flexDirection = FlexDirection.Column,
                            flexGrow = 1
                            }
                        };
                        innerbox.Add(_filesView);
                        sourceTreeFilesBox.Add(filesBox);
                    }

                    root.Add(sourceTreeFilesBox);
                    grip_SourceTree.AddManipulator(new PanelDragger(sourcesBox, treeBox));
                    grip_treeFiles.AddManipulator(new PanelDragger(treeBox, filesBox));
                }
                {
                    VisualElement border = new VisualElement()
                    {
                        name = "Files_ContextSeparator",
                        style = {
                        width = 5,
                        maxWidth = 5,
                        minWidth = 5,
                        backgroundColor = darkLineColor,
                        }
                    };
                    border.Add(grip_filesContext = new VisualElement()
                    {
                        style =
                        {
                            width = 5,
                            maxWidth = 5,
                            minWidth = 5,
                            flexGrow = 1,
                        }
                    });
                    root.Add(border);
                }

                //
                //Context
                //
                VisualElement contextBox = new VisualElement()
                {
                    style = {
                        minWidth = 350,
                        maxWidth = 350
                    }
                };
                {
                    contextBox.Add(new Label("Selection")
                    {
                        style = {
                        fontSize = new StyleLength(16),
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginLeft = 5,
                        marginTop = 5,
                        marginBottom = 5
                    }
                    });
                    root.Add(contextBox);
                }

                //grip_filesContext.AddManipulator(new PanelDragger(filesBox, contextBox));
            }
        }

        private void OnDisableLayout()
        {
            _sourcesView.onSelectionChanged -= OnSourceListSelectionChanged;
        }

        class PanelDragger : MouseManipulator
        {
            VisualElement _left;
            VisualElement _right;
            bool isDragging;
            Vector2 dragStart;

            public PanelDragger(VisualElement left, VisualElement right)
            {
                _left = left;
                _right = right;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
                target.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
                target.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<MouseUpEvent>(OnMouseUpEvent);
                target.UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
                target.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent);
            }

            void OnMouseDownEvent(MouseEventBase<MouseDownEvent> evt)
            {
                //Debug.Log("Receiving " + evt + " in " + evt.propagationPhase + " for target " + evt.target);

                if (isDragging)
                {
                    evt.StopImmediatePropagation();
                }

                //if (CanStartManipulation(evt))
                {
                    dragStart = evt.localMousePosition;
                    isDragging = true;
                    target.CaptureMouse();
                    evt.StopPropagation();
                }
            }

            void OnMouseMoveEvent(MouseEventBase<MouseMoveEvent> evt)
            {
                //Debug.Log("Receiving " + evt + " in " + evt.propagationPhase + " for target " + evt.target);
                if (isDragging && target.HasMouseCapture())
                {
                    Vector2 diff = evt.localMousePosition - dragStart;
                    dragStart += diff;

                    float leftWidth = _left.style.width.value.value;
                    float leftMinWidth = _left.style.minWidth.value.value;

                    leftWidth += diff.x;
                    if (leftWidth < leftMinWidth) leftWidth = leftMinWidth;

                    float rightWidth = _right.style.width.value.value;
                    float rightMinWidth = _right.style.minWidth.value.value;
                    if (rightWidth < rightMinWidth) rightWidth = rightMinWidth;

                    _left.style.width = leftWidth;
                    _right.style.width = rightWidth;

                    evt.StopPropagation();
                }
            }

            void OnMouseUpEvent(MouseEventBase<MouseUpEvent> evt)
            {
                //Debug.Log("Receiving " + evt + " in " + evt.propagationPhase + " for target " + evt.target);
                isDragging = false;
                target.ReleaseMouse();
                evt.StopPropagation();
            }
        }
    }
}
