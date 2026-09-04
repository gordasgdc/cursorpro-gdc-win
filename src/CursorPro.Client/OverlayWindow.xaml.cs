using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CursorPro.Client;

/// O fereastră fără chenar, transparentă, care lasă clicurile să treacă
/// prin ea, și acoperă exact un singur ecran fizic — echivalentul
/// OverlayWindow.swift (Mac). Se creează câte una per
/// `Screen.AllScreens` (vezi OverlayManager), astfel încât coordonatele
/// de desenare se aliniază 1:1 cu ecranul respectiv.
public partial class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;   // nu fură clicuri destinate aplicațiilor reale
    private const int WS_EX_TOOLWINDOW = 0x80;    // nu apare în Alt+Tab / taskbar
    private const int WS_EX_NOACTIVATE = 0x8000000; // niciodată nu ia focus-ul de la fereastra activă

    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public OverlaySurface Surface { get; } = new();

    public OverlayWindow()
    {
        InitializeComponent();
        Content = Surface;
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
        };
    }
}
