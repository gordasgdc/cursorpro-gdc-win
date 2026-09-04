using System.Windows;
using System.Windows.Forms;
using CursorPro.Core.Services;
using CursorPro.Core.State;

namespace CursorPro.Client;

/// Creează/poziționează/actualizează fereastra lupei cât timp Zoom e
/// activ — echivalentul `ZoomWindowController.tick()` (Mac). Chemat din
/// aceeași buclă de ~60fps ca Halo/Spotlight (vezi OverlayManager), nu
/// are timer propriu.
public sealed class ZoomController
{
    private ZoomWindow? _window;
    private bool _wasActive;
    private bool _magInitialized;

    public void Start()
    {
        _magInitialized = MagnificationInterop.MagInitialize();
        if (!_magInitialized)
        {
            DebugLog.Log("ZoomController.Start: MagInitialize() a EȘUAT — Zoom nu va funcționa pe acest sistem.");
        }
    }

    public void Tick()
    {
        var state = AppState.Shared;

        if (!state.IsZoomActive)
        {
            if (_wasActive)
            {
                _window?.Hide();
                DebugLog.Log("Zoom: fereastră lupă ascunsă");
            }
            _wasActive = false;
            return;
        }

        if (!_magInitialized) return; // Zoom cerut, dar API-ul nu s-a inițializat — nimic de arătat.

        var cursorPx = state.MouseLocation; // pixeli fizici globali
        var screen = Screen.FromPoint(new System.Drawing.Point((int)cursorPx.X, (int)cursorPx.Y));
        double dpiScale;
        using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) { dpiScale = g.DpiX / 96.0; }

        if (_window is null)
        {
            _window = new ZoomWindow(AppState.ZoomWindowDiameter, dpiScale);
            DebugLog.Log("Zoom: fereastră lupă creată");
        }

        if (!_wasActive)
        {
            _window.Show();
            DebugLog.Log($"Zoom: activat — factor={state.ZoomFactor:F1}x, rază={state.ZoomRadius:F0}dip");
        }
        _wasActive = true;

        var cursorDip = new System.Drawing.PointF((float)(cursorPx.X / dpiScale), (float)(cursorPx.Y / dpiScale));
        var screenBoundsDip = new Rect(
            screen.Bounds.Left / dpiScale, screen.Bounds.Top / dpiScale,
            screen.Bounds.Width / dpiScale, screen.Bounds.Height / dpiScale);
        _window.PositionAt(cursorDip, screenBoundsDip);

        // Sursa capturată e un pătrat centrat pe cursor, cu latura =
        // 2×rază (DIP), convertit în pixeli fizici pentru
        // MagSetWindowSource (care lucrează în pixeli de ecran, nu DIP).
        double radiusPx = state.ZoomRadius * dpiScale;
        int left = (int)Math.Round(cursorPx.X - radiusPx);
        int top = (int)Math.Round(cursorPx.Y - radiusPx);
        int size = (int)Math.Round(radiusPx * 2);
        _window.Magnifier.SetSource(left, top, left + size, top + size);
        _window.Magnifier.SetScale(state.ZoomFactor);
    }

    public void Stop()
    {
        _window?.Close();
        _window = null;
        if (_magInitialized)
        {
            MagnificationInterop.MagUninitialize();
            _magInitialized = false;
        }
    }
}
