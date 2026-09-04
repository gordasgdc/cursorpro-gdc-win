using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;
using Pen = System.Windows.Media.Pen;
using CursorPro.Core.Services;
using CursorPro.Core.State;
using MediaColor = System.Windows.Media.Color;

namespace CursorPro.Client;

/// Elementul care desenează efectiv Halo-ul și Spotlight-ul pe ecranul
/// acoperit de fereastra lui — echivalentul OverlayView.swift (Mac), dar
/// ca `FrameworkElement.OnRender` (Visual API imediat, fără retained-mode
/// shapes) în loc de Core Graphics direct — cel mai apropiat echivalent
/// WPF ca preț de performanță la 60fps.
public sealed class OverlaySurface : FrameworkElement
{
    /// Colțul stânga-sus al ecranului acoperit de fereastra asta, în
    /// pixeli fizici globali — pentru conversia poziției globale a
    /// cursorului (AppState.MouseLocation, tot în pixeli fizici) în
    /// coordonate locale (DIP) ale acestui element. Setat o singură
    /// dată de OverlayWindow, la creare.
    public System.Drawing.Point ScreenOrigin { get; set; }

    /// Factorul DPI al acestui ecran (1.0 = 96 DPI / 100%) — pentru
    /// conversia pixeli fizici → DIP. Vezi OverlayWindow pentru cum e
    /// calculat (aproximare pe baza DPI-ului ecranului primar — vezi
    /// nota din CLAUDE.md despre configurații multi-monitor cu procente
    /// de scalare DIFERITE între ecrane, neverificat pe hardware real).
    public double DpiScale { get; set; } = 1.0;

    /// Lățimea/înălțimea ecranului (DIP) — setate explicit de OverlayManager
    /// din `screen.Bounds` la creare, NU citite din `ActualWidth`/
    /// `ActualHeight`: acestea depind de un pas de layout WPF finalizat
    /// (Measure/Arrange), care nu e garantat să fi rulat deja la primele
    /// cadre after `Show()` — un dreptunghi Spotlight dimensionat greșit
    /// (0 sau foarte mic) ar fi practic invizibil, spre deosebire de Halo,
    /// care nu depinde deloc de dimensiunea elementului (desenat relativ
    /// la cursor). Valorile astea sunt cunoscute exact dinainte, fără să
    /// depindă de timing-ul intern al WPF.
    public double ScreenWidthDip { get; set; }
    public double ScreenHeightDip { get; set; }

    /// Chemat de OverlayManager de ~60 ori/secundă — echivalentul
    /// `needsDisplay = true` din bucla de refresh a OverlayView (Mac).
    public void Refresh() => InvalidateVisual();

    private Point Local(System.Drawing.PointF globalPixels) => new(
        (globalPixels.X - ScreenOrigin.X) / DpiScale,
        (globalPixels.Y - ScreenOrigin.Y) / DpiScale);

    protected override void OnRender(DrawingContext dc)
    {
        var state = AppState.Shared;
        var cursor = Local(state.MouseLocation);

        // Ordine identică cu OverlayView.draw(_:) (Mac): Spotlight
        // dedesubt, Halo deasupra.
        if (state.IsSpotlightActive)
        {
            DrawSpotlight(dc, state, cursor);
        }

        if (state.HaloEnabled && LicenseManager.Shared.IsUnlocked)
        {
            DrawHalo(dc, state, cursor);
        }
    }

    private static MediaColor ToMediaColor(System.Drawing.Color c) => MediaColor.FromArgb(c.A, c.R, c.G, c.B);

    private static void DrawHalo(DrawingContext dc, AppState state, Point p)
    {
        double d = state.HaloDiameter;
        double r = d / 2;
        var color = ToMediaColor(state.HaloColor);
        var pen = new Pen(new SolidColorBrush(color), state.HaloLineWidth);
        pen.Freeze();

        switch (state.HaloStyle)
        {
            case HaloStyle.Ring:
                dc.DrawEllipse(null, pen, p, r, r);
                break;

            case HaloStyle.Filled:
                // copy(alpha: 0.55) pe Mac ÎNLOCUIEȘTE alpha-ul original
                // cu 0.55 (nu îl multiplică) — la fel aici.
                var fill = new SolidColorBrush(MediaColor.FromArgb((byte)Math.Round(255 * 0.55), color.R, color.G, color.B));
                fill.Freeze();
                dc.DrawEllipse(fill, pen, p, r, r);
                break;

            case HaloStyle.Crosshair:
                dc.DrawEllipse(null, pen, p, r, r);
                double tick = d * 0.4;
                dc.DrawLine(pen, new Point(p.X - r - tick, p.Y), new Point(p.X - r, p.Y));
                dc.DrawLine(pen, new Point(p.X + r, p.Y), new Point(p.X + r + tick, p.Y));
                dc.DrawLine(pen, new Point(p.X, p.Y - r - tick), new Point(p.X, p.Y - r));
                dc.DrawLine(pen, new Point(p.X, p.Y + r), new Point(p.X, p.Y + r + tick));
                break;
        }
    }

    private void DrawSpotlight(DrawingContext dc, AppState state, Point p)
    {
        double r = state.SpotlightRadius;
        byte alpha = (byte)Math.Round(255 * Math.Clamp(state.SpotlightDimOpacity, 0, 1));
        var brush = new SolidColorBrush(MediaColor.FromArgb(alpha, 0, 0, 0));
        brush.Freeze();

        // "Gaură" circulară: dreptunghiul ecranului XOR cercul din jurul
        // cursorului — echivalentul exact al CGMutablePath (addRect +
        // addEllipse, fillPath(using: .evenOdd)) din OverlayView.swift.
        var outer = new RectangleGeometry(new Rect(0, 0, Math.Max(ScreenWidthDip, 1), Math.Max(ScreenHeightDip, 1)));
        var hole = new EllipseGeometry(p, r, r);
        var combined = new CombinedGeometry(GeometryCombineMode.Xor, outer, hole);
        dc.DrawGeometry(brush, null, combined);
    }
}
