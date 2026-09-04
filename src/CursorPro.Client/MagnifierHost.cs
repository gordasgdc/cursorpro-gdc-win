using System.Runtime.InteropServices;
using System.Windows.Interop;
using CursorPro.Core.Services;

namespace CursorPro.Client;

/// Găzduiește fereastra nativă „Magnifier" (Windows Magnification API) ca
/// HWND copil, prin `HwndHost` (mecanismul standard WPF pentru încorporat
/// ferestre native) — echivalentul funcțional al SCStream (Mac): conținut
/// LIVE, continuu, al regiunii de ecran indicate de `SetSource`, nu
/// capturi statice pe un timer.
public sealed class MagnifierHost : HwndHost
{
    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    private IntPtr _magHwnd;
    private readonly int _widthPx;
    private readonly int _heightPx;

    public MagnifierHost(int widthPx, int heightPx)
    {
        _widthPx = widthPx;
        _heightPx = heightPx;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _magHwnd = CreateWindowEx(
            0, MagnificationInterop.WindowClassName, "MagnifierWindow",
            WS_CHILD | WS_VISIBLE,
            0, 0, _widthPx, _heightPx,
            hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        return new HandleRef(this, _magHwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (hwnd.Handle != IntPtr.Zero) DestroyWindow(hwnd.Handle);
    }

    /// Regiunea de ecran (pixeli fizici, coordonate globale) pe care lupa
    /// o afișează, mărită. Chemat la fiecare cadru cât timp Zoom e activ
    /// — reconfigurarea sursei e ieftină (proprie API-ului), spre
    /// deosebire de a porni o captură nouă de fiecare dată.
    public void SetSource(int left, int top, int right, int bottom)
    {
        if (_magHwnd == IntPtr.Zero) return;
        MagnificationInterop.MagSetWindowSource(_magHwnd, new MagRect { Left = left, Top = top, Right = right, Bottom = bottom });
    }

    public void SetScale(float factor)
    {
        if (_magHwnd == IntPtr.Zero) return;
        var t = MagTransform.Scale(factor);
        MagnificationInterop.MagSetWindowTransform(_magHwnd, ref t);
    }
}
