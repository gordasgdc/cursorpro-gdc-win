using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CursorPro.Client;

/// Fereastra circulară a lupei — echivalentul ferestrei create în
/// `ZoomWindowController.showWindow()` (Mac): fără chenar, deasupra
/// tuturor, urmărește cursorul cât timp Zoom e activ. Formă circulară
/// obținută cu `SetWindowRgn` (clipping la nivel de fereastră nativă —
/// FUNCȚIONEAZĂ corect peste un HWND copil găzduit, spre deosebire de
/// `AllowsTransparency` WPF, care are o limitare cunoscută: conținutul
/// unui `HwndHost` nu se compune corect într-o fereastră WPF layered).
///
/// NEPORTAT încă (TODO explicit — vezi CHANGELOG.md): bordura albă/
/// portocalie (blocată/normal), reticula-cruce din centru, citirea de
/// culoare (color picker) — toate cer desenare WPF DEASUPRA
/// conținutului HwndHost, care are propria ei complicație reală
/// ("HWND airspace": un HwndHost se randează mereu deasupra fraților
/// WPF simpli, indiferent de ordinea din arborele vizual).
public partial class ZoomWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x80;
    private const int WS_EX_NOACTIVATE = 0x8000000;

    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateEllipticRgn(int x1, int y1, int x2, int y2);
    [DllImport("user32.dll")] private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

    public MagnifierHost Magnifier { get; }
    private readonly double _dpiScale;

    /// `diameterDip`: diametrul dorit al lupei, în DIP (vezi
    /// AppState.ZoomWindowDiameter). `dpiScale`: factorul DPI al
    /// ecranului pe care se va afișa (vezi nota din OverlaySurface
    /// despre configurațiile multi-monitor cu scalare diferită).
    public ZoomWindow(double diameterDip, double dpiScale)
    {
        InitializeComponent();
        _dpiScale = dpiScale;
        Width = diameterDip;
        Height = diameterDip;

        int diameterPx = (int)Math.Round(diameterDip * dpiScale);
        Magnifier = new MagnifierHost(diameterPx, diameterPx);
        Content = Magnifier;

        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
            SetWindowRgn(hwnd, CreateEllipticRgn(0, 0, diameterPx, diameterPx), true);
        };
    }

    /// Poziționează lupa centrată pe cursor (coordonate globale DIP),
    /// limitată să rămână în interiorul `screenBoundsDip` — echivalentul
    /// `reposition()` (Mac).
    public void PositionAt(System.Drawing.PointF cursorDip, Rect screenBoundsDip)
    {
        double x = Math.Clamp(cursorDip.X - Width / 2, screenBoundsDip.Left, screenBoundsDip.Right - Width);
        double y = Math.Clamp(cursorDip.Y - Height / 2, screenBoundsDip.Top, screenBoundsDip.Bottom - Height);
        Left = x;
        Top = y;
    }
}
