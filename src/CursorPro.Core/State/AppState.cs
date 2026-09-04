using System.Drawing;

namespace CursorPro.Core.State;

public enum HaloStyle { Ring, Filled, Crosshair }

/// Taste modificator disponibile pentru legarea unui mod (Spotlight etc.)
/// — oglindă AppState.ModifierKey (Mac), fără `Function` (fn pe Windows
/// nu se poate observa fiabil printr-un hook global, la fel ca pe Mac —
/// vezi comentariul din InputMonitor.swift) și cu `Windows` în loc de
/// `Command`.
public enum ModifierKey { Alt, Control, Shift, Windows }

/// Stare partajată, în memorie — oglindă AppState.swift (Mac). Portate
/// complet: Halo + Spotlight (v1.3.0), Zoom (v1.4.0) — toate cu gating pe
/// licență. NEPORTATE încă, deliberat: Desen, Efecte de Clic, Afișare
/// Taste Rapide, Preseturi Focus, Semnal multi-display — vezi
/// CHANGELOG.md.
/// La fel ca pe Mac, NU e persistată între lansări (doar Licența și Limba
/// sunt persistate, în LicenseManager/Localization) — pornește mereu din
/// valorile implicite de mai jos.
public sealed class AppState
{
    public static readonly AppState Shared = new();
    private AppState() { }

    // MARK: - Live pointer state (actualizat continuu de InputMonitor)
    /// Coordonate globale de ecran (pixeli, origine colț stânga-sus al
    /// ecranului primar — convenția Win32/WinForms, spre deosebire de
    /// originea colț stânga-jos de pe Mac; conversia se face în
    /// OverlayWindow, nu aici).
    public PointF MouseLocation { get; set; }

    // MARK: - Flaguri de mod (comandate de tastele modificator ținute apăsat)
    public bool IsSpotlightActive { get; set; }
    public bool IsZoomActive { get; set; }

    // MARK: - Aspect Halo
    public bool HaloEnabled { get; set; } = true;
    public Color HaloColor { get; set; } = Color.FromArgb(255, 255, 204, 0); // systemYellow
    public float HaloDiameter { get; set; } = 32;
    public float HaloLineWidth { get; set; } = 3;
    public HaloStyle HaloStyle { get; set; } = HaloStyle.Ring;

    // MARK: - Aspect Spotlight
    public float SpotlightRadius { get; set; } = 160;
    /// 0 = complet transparent, 1 = complet negru.
    public double SpotlightDimOpacity { get; set; } = 0.75;

    // MARK: - Aspect Zoom (lupă)
    /// Cât de puternic mărește lupa — factor direct, ales de utilizator
    /// (vezi ZoomFactorMin/Max mai jos). Raza capturată se DERIVĂ din
    /// acest factor + diametrul fix al ferestrei lupei — NU se
    /// controlează independent, la fel ca pe Mac (`zoomRadius`,
    /// AppState.swift).
    public float ZoomFactor { get; set; } = 3;
    public const float ZoomFactorMin = 1.1f;
    public const float ZoomFactorMax = 12f;
    /// Diametrul fix al ferestrei lupei, în DIP.
    public const float ZoomWindowDiameter = 360;
    /// Raza, în DIP, a regiunii de ecran capturate în jurul cursorului.
    public float ZoomRadius => ZoomWindowDiameter / (2 * ZoomFactor);

    // MARK: - Legături de taste
    public ModifierKey SpotlightKey { get; set; } = ModifierKey.Control;
    public ModifierKey ZoomKey { get; set; } = ModifierKey.Shift;
}
