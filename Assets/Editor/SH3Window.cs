using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using Process = System.Diagnostics.Process;

[CustomEditor(typeof(WinTest))]
public class SH3Window : EditorWindow
{
    static SH3Window()
    {
        MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        _IsDocked = (Func<EditorWindow, bool>)Delegate.CreateDelegate(typeof(Func<EditorWindow, bool>), isDockedMethod);
    }

    //EXE Settings
    [SerializeField]
    private string _exePath = "";

    [SerializeField]
    private int _sh3Process;
    public Process sh3Process { get { try { return Process.GetProcessById(_sh3Process); } catch { _sh3Process = 0; return null; } } set { _sh3Process = value == null ? 0 : value.Id; } }

    //Window settings
    static Func<EditorWindow, bool> _IsDocked;
    public bool isDocked { get { return _IsDocked(this); } }

    [SerializeField]
    private long _sh3Window;
    public IntPtr sh3Window { get { return new IntPtr(_sh3Window); } set { _sh3Window = value.ToInt64(); } }

    [SerializeField]
    private long _unityWindow;
    public IntPtr unityWindow { get { return new IntPtr(_unityWindow); } set { _unityWindow = value.ToInt64(); } }

    [SerializeField]
    private Rect _windowSizeAtCapture;

    public bool isCaptured { get { return _sh3Window != 0L; } }

    //Toolbar settings
    [SerializeField]
    private bool _autoRestart;

    [SerializeField]
    private bool _trapMouse;

    [SerializeField]
    private RatioSizes _currentRatio = RatioSizes.Ratio4x3;

    //EditorWindow settings
    private Rect _displayRect;

    private IntPtr _lastForegroundWindow;

    public enum RatioSizes
    {
        FreeRatio = 0x00,
        Ratio4x3 = 0x01,
        Ratio1x1 = 0x02,
        Ratio5x4 = 0x03,
        Ratio3x2 = 0x04,
        Ratio16x9 = 0x05,
        Ratio16x10 = 0x06,
        Ratio21x9 = 0x07,
    }

    [MenuItem("ShiningHill/Silent Hill 3")]
    public static void ShowWindow()
    {
        SH3Window ew = (SH3Window)EditorWindow.GetWindow(typeof(SH3Window));
        ew.titleContent = new GUIContent("Silent Hill 3");
        
        Process cproc = Process.GetCurrentProcess();
        ew.unityWindow = User32.util.GetMainWindow(cproc);

    }

    void OnGUI()
    {
        bool triggerWindowUpdate = false;
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        //GUILayout.Button("0x"+sh3Window.ToString("X8"), EditorStyles.toolbarButton);
        if (sh3Process != null)
        {
            if (GUILayout.Button("Release", EditorStyles.toolbarButton, GUILayout.Width(75)))
            {
                ReleaseWindow();
            }
        }
        else if(Process.GetProcessesByName("sh3").Length != 0)
        {
            if (GUILayout.Button("Capture", EditorStyles.toolbarButton, GUILayout.Width(75)))
            {
                sh3Process = Process.GetProcessesByName("sh3")[0];
                CaptureWindow();
                triggerWindowUpdate = true;
            }
        }
        else if(sh3Process == null)
        {
            if (GUILayout.Button("Start", EditorStyles.toolbarButton, GUILayout.Width(75)))
            {
                string path = EditorUtility.OpenFilePanel("Start Silent Hill 3", _exePath, "exe");

                if (!String.IsNullOrEmpty(path))
                {
                    _exePath = path;
                    StartWindow();
                    triggerWindowUpdate = true;
                }
            }
        }
        _autoRestart = GUILayout.Toggle(_autoRestart, "Auto Restart", EditorStyles.toolbarButton, GUILayout.Width(75));

        _trapMouse = GUILayout.Toggle(_trapMouse, _trapMouse ? "M. Trapped" : "M. Free", EditorStyles.toolbarButton, GUILayout.Width(75));

        EditorGUILayout.Space();
        
        EditorGUI.BeginChangeCheck();
        _currentRatio = (RatioSizes)EditorGUILayout.EnumPopup(_currentRatio, EditorStyles.toolbarPopup, GUILayout.Width(80));
        if (EditorGUI.EndChangeCheck())
        {
            triggerWindowUpdate = true;
        }


        EditorGUILayout.EndHorizontal();
        
        Rect mainRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
        if (_displayRect != mainRect && mainRect != new Rect(0.0f, 0.0f, 1.0f, 1.0f))
        {
            _displayRect = mainRect;
            triggerWindowUpdate = true;
        }

        if (triggerWindowUpdate)
        {
            MoveWindowTo(_displayRect);
        }
    }

    private void Update()
    {
        if (!String.IsNullOrEmpty(_exePath) && sh3Process == null && _autoRestart)
        {
            StartWindow();
        }

        IntPtr currentForegroundWindow = User32.GetForegroundWindow();
        if (currentForegroundWindow != IntPtr.Zero)
        {
            if (currentForegroundWindow != _lastForegroundWindow)
            {
                //Get into sh3
                if (currentForegroundWindow == sh3Window)
                {
                    FixMouseTrap(_trapMouse);
                }

                //Get out of sh3
                if (_lastForegroundWindow == sh3Window)
                {
                    FixMouseTrap(false);
                }
                _lastForegroundWindow = currentForegroundWindow;
            }
        }        
    }

    private void StartWindow()
    {
        sh3Process = Process.Start(_exePath);

        EditorUtility.DisplayProgressBar("Waiting", "Waiting for Silent Hill 3 to start", 0.5f);
        try
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            float timeout = 0.0f;
            while (User32.util.GetMainWindow(sh3Process) == IntPtr.Zero && timeout <= 1.0f)
            {
                System.Threading.Thread.Sleep(250);
                timeout = (float)sw.ElapsedMilliseconds / 60000.0f;
                EditorUtility.DisplayProgressBar("Waiting", "Waiting for Silent Hill 3 to start", 0.5f + (0.5f * timeout));
            }

            CaptureWindow();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void CaptureWindow()
    {
        sh3Window = User32.util.GetMainWindow(sh3Process);

        User32.Rect windowSize = new User32.Rect();
        User32.GetWindowRect(sh3Window, ref windowSize);
        _windowSizeAtCapture = new Rect(windowSize.left, windowSize.top, windowSize.right - windowSize.left, windowSize.bottom - windowSize.top);

        uint style = User32.GetWindowLong(sh3Window, User32.GWL_STYLE);
        User32.SetWindowLong(sh3Window, User32.GWL_STYLE, (style & ~User32.WS_CAPTION & ~User32.WS_SIZEBOX));

        MoveWindowTo(_displayRect);

        Process cproc = Process.GetCurrentProcess();
        User32.SetParent(sh3Window, unityWindow);


        //_target.Size = new System.Drawing.Size(windowSize.right - windowSize.left, windowSize.bottom - windowSize.top);
        //ShowWindowAsync(_window, SW_SHOWMAXIMIZED);
        //Scribe.InitTo(_proc);
    }

    private void ReleaseWindow()
    {
        if (isCaptured)
        {
            User32.SetParent(sh3Window, IntPtr.Zero);

            uint style = User32.GetWindowLong(sh3Window, User32.GWL_STYLE);
            User32.SetWindowLong(sh3Window, User32.GWL_STYLE, (style | User32.WS_CAPTION));

            User32.SetWindowPos(sh3Window, 0, (int)_windowSizeAtCapture.x, (int)_windowSizeAtCapture.y, (int)_windowSizeAtCapture.width, (int)_windowSizeAtCapture.height, User32.SWP_NOZORDER | User32.SWP_NOACTIVATE);
        }
        sh3Window = IntPtr.Zero;
        sh3Process = null;
    }

    private void MoveWindowTo(Rect controlRect)
    {
        if (isCaptured)
        {
            Rect final = position;
            Process cproc = Process.GetCurrentProcess();
            unityWindow = User32.util.GetMainWindow(cproc);
            User32.Rect windowSize = new User32.Rect();
            User32.GetWindowRect(unityWindow, ref windowSize);
            final.x -= windowSize.left;
            final.y -= windowSize.top;

            //for win7 at least
            final.x -= 8;
            final.y -= 50;

            final.x += controlRect.x;
            final.y += controlRect.y;

            int rdx, rdy;
            GetRatiod(_currentRatio, (int)controlRect.width, (int)controlRect.height, out rdx, out rdy);
            final.width = (float)rdx;
            final.height = (float)rdy;

            final.x += (controlRect.width / 2) - (final.width / 2);
            final.y += (controlRect.height / 2) - (final.height / 2);

            User32.SetWindowPos(sh3Window, User32.HWND_TOP, (int)final.x, (int)final.y, (int)final.width, (int)final.height, User32.SWP_NOACTIVATE);
        }
    }

    private void FixMouseTrap(bool trap)
    {
        if (trap)
        {
            User32.Rect windowSize = new User32.Rect();
            User32.GetWindowRect(sh3Window, ref windowSize);
            windowSize = new User32.Rect() { left = windowSize.left + 2, top = windowSize.top + 2, right = windowSize.right - 2, bottom = windowSize.bottom - 2 };
            User32.ClipCursor(windowSize);
        }
        else
        {
            User32.ClipCursor(IntPtr.Zero);
        }
    }

    void OnDestroy()
    {
        ReleaseWindow();
    }

    private void GetRatiod(RatioSizes ratio, int maxX, int maxY, out int adjustedX, out int adjustedY)
    {
        int rx, ry;
        if (ratio == RatioSizes.Ratio4x3) { rx = 4; ry = 3; }
        else if(ratio == RatioSizes.Ratio1x1) { rx = 1; ry = 1; }
        else if(ratio == RatioSizes.Ratio5x4) { rx = 5; ry = 4; }
        else if(ratio == RatioSizes.Ratio3x2) { rx = 3; ry = 2; }
        else if(ratio == RatioSizes.Ratio16x9) { rx = 16; ry = 6; }
        else if(ratio == RatioSizes.Ratio16x10) { rx = 16; ry = 10; }
        else if(ratio == RatioSizes.Ratio21x9) { rx = 21; ry = 9; }
        else { adjustedX = maxX; adjustedY = maxY; return; }

        if (maxX > maxY)
        {
            adjustedX = (rx * maxY) / ry;
            adjustedY = maxY;

            if (adjustedX > maxX)
            {
                adjustedX = maxX;
                adjustedY = (ry * maxX) / rx;
            }
            return;
        }
        else if (maxY > maxX)
        {
            adjustedX = maxX;
            adjustedY = (ry * maxX) / rx;

            if (adjustedY > maxY)
            {
                adjustedX = (rx * maxY) / ry;
                adjustedY = maxY;
            }
            return;
        }

        adjustedX = maxX;
        adjustedY = maxY;
    }
}
