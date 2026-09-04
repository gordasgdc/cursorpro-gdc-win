using System.Windows.Forms;
using System.Windows.Threading;
using CursorPro.Core.Services;

namespace CursorPro.Client;

/// Creează câte o OverlayWindow pentru fiecare ecran conectat și le
/// ține sincronizate cu o singură buclă de ~60fps — echivalentul
/// combinat al AppDelegate.swift (crearea ferestrelor, câte una per
/// NSScreen) + bucla de refresh proprie fiecărei OverlayView (Mac).
///
/// Ferestrele se creează O SINGURĂ DATĂ, la pornirea aplicației, și
/// rămân vii cât timp rulează CursorPro GDC — NU se recreează dinamic
/// la conectare/deconectare de monitor. Ecranele adăugate/scoase live
/// (docking station, proiector) NU sunt încă gestionate — TODO explicit,
/// vezi CHANGELOG.md.
public sealed class OverlayManager
{
    private readonly List<OverlayWindow> _windows = new();
    private readonly ZoomController _zoom = new();
    private DispatcherTimer? _timer;

    public void Start()
    {
        _zoom.Start();
        // Factor DPI aproximat de pe ecranul primar, aplicat tuturor
        // ecranelor la poziționarea inițială — vezi nota din
        // OverlaySurface.DpiScale despre configurațiile multi-monitor cu
        // procente de scalare DIFERITE (neverificat pe hardware real).
        double dpiScale;
        using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
        {
            dpiScale = g.DpiX / 96.0;
        }

        DebugLog.Log($"OverlayManager.Start: dpiScale={dpiScale:F2}, {Screen.AllScreens.Length} ecran(e)");

        foreach (var screen in Screen.AllScreens)
        {
            double widthDip = screen.Bounds.Width / dpiScale;
            double heightDip = screen.Bounds.Height / dpiScale;
            var window = new OverlayWindow
            {
                Left = screen.Bounds.Left / dpiScale,
                Top = screen.Bounds.Top / dpiScale,
                Width = widthDip,
                Height = heightDip,
            };
            window.Surface.ScreenOrigin = screen.Bounds.Location;
            window.Surface.DpiScale = dpiScale;
            window.Surface.ScreenWidthDip = widthDip;
            window.Surface.ScreenHeightDip = heightDip;
            window.Show();
            _windows.Add(window);
            DebugLog.Log($"  ecran '{screen.DeviceName}': bounds={screen.Bounds}, fereastră DIP={widthDip:F0}x{heightDip:F0}");
        }

        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromSeconds(1.0 / 60.0),
        };
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    private void Tick()
    {
        InputMonitor.Tick();
        foreach (var window in _windows)
        {
            window.Surface.Refresh();
        }
        _zoom.Tick();
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer = null;
        foreach (var window in _windows)
        {
            window.Close();
        }
        _windows.Clear();
        _zoom.Stop();
    }
}
