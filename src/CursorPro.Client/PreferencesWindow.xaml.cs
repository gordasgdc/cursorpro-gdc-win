using System.Diagnostics;
using System.Drawing;
using System.Windows;
using CursorPro.Core.Services;
using CursorPro.Core.State;

namespace CursorPro.Client;

/// Fereastra de Preferințe — echivalentul PreferencesWindowController.swift
/// (Mac). Doar tab-urile General (placeholder) și Licență sunt reale în
/// acest prim schelet; restul (Halo/Spotlight/Desen/Zoom/Taste) urmează.
public partial class PreferencesWindow : Window
{
    /// Paletă fixă pentru ComboBox-ul de culoare Halo — echivalentul
    /// aproximativ al culorilor sistem folosite implicit pe Mac
    /// (systemYellow/systemRed/etc.), fără un color-picker complet (WPF
    /// nu are unul nativ) — de extins la cerere.
    private static readonly Color[] HaloColors =
    {
        Color.FromArgb(255, 255, 204, 0),   // Galben
        Color.FromArgb(255, 255, 59, 48),   // Roșu
        Color.FromArgb(255, 52, 199, 89),   // Verde
        Color.FromArgb(255, 0, 122, 255),   // Albastru
        Color.FromArgb(255, 255, 45, 85),   // Roz
        Color.FromArgb(255, 255, 255, 255), // Alb
    };

    // Implicit `true` (nu doar setat în LoadHaloControls()) — Sliderele/
    // ComboBox-urile din XAML își trag propriul eveniment (ValueChanged/
    // SelectionChanged) chiar în timpul InitializeComponent() (parsare
    // BAML), ÎNAINTE ca restul câmpurilor numite (x:Name) să fie
    // asignate — un handler care le citește pe toate ar arunca
    // NullReferenceException în acel moment. Vezi raportul de crash.
    private bool _loadingHaloControls = true;

    public PreferencesWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Versiune {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?"}";
        MachineIdText.Text = MachineID.Display;
        RefreshLicenseStatus();
        LoadHaloControls();
    }

    /// Citește valorile curente din AppState (NU persistate — repornesc
    /// la valorile implicite la fiecare lansare, la fel ca pe Mac, vezi
    /// AppState.cs) și le pune în controale, fără să declanșeze
    /// handler-ele de mai jos (`_loadingHaloControls`).
    private void LoadHaloControls()
    {
        _loadingHaloControls = true;
        var state = AppState.Shared;

        HaloEnabledCheck.IsChecked = state.HaloEnabled;
        HaloStyleCombo.SelectedIndex = (int)state.HaloStyle;
        HaloColorCombo.SelectedIndex = Math.Max(0, Array.IndexOf(HaloColors, state.HaloColor));
        HaloDiameterSlider.Value = state.HaloDiameter;
        HaloLineWidthSlider.Value = state.HaloLineWidth;

        SpotlightRadiusSlider.Value = state.SpotlightRadius;
        SpotlightDimSlider.Value = state.SpotlightDimOpacity;
        SpotlightKeyCombo.SelectedIndex = (int)state.SpotlightKey;

        _loadingHaloControls = false;
    }

    /// Un singur handler pentru toate controalele Halo/Spotlight — scrie
    /// direct în AppState.Shared, citit continuu de OverlaySurface la
    /// fiecare cadru (nu e nevoie de notificare separată, vezi
    /// OverlayManager). La fel ca binding-ul direct pe @Published din
    /// PreferencesWindowController (Mac), dar imperativ (fără MVVM).
    private void HaloControl_Changed(object sender, RoutedEventArgs e)
    {
        if (_loadingHaloControls) return;
        var state = AppState.Shared;

        state.HaloEnabled = HaloEnabledCheck.IsChecked == true;
        if (HaloStyleCombo.SelectedIndex >= 0) state.HaloStyle = (HaloStyle)HaloStyleCombo.SelectedIndex;
        if (HaloColorCombo.SelectedIndex >= 0) state.HaloColor = HaloColors[HaloColorCombo.SelectedIndex];
        state.HaloDiameter = (float)HaloDiameterSlider.Value;
        state.HaloLineWidth = (float)HaloLineWidthSlider.Value;

        state.SpotlightRadius = (float)SpotlightRadiusSlider.Value;
        state.SpotlightDimOpacity = SpotlightDimSlider.Value;
        if (SpotlightKeyCombo.SelectedIndex >= 0) state.SpotlightKey = (ModifierKey)SpotlightKeyCombo.SelectedIndex;
    }

    public void SelectTab(PreferencesTab tab)
    {
        Tabs.SelectedItem = tab == PreferencesTab.License ? LicenseTab : GeneralTab;
    }

    private void RefreshLicenseStatus()
    {
        var license = LicenseManager.Shared;
        StatusText.Text = license.IsLicensed
            ? "✅ Licențiat" + (license.LicenseExpiresAt == 0 ? " — acces pe viață" : $" — expiră {DateTimeOffset.FromUnixTimeSeconds(license.LicenseExpiresAt):d MMM yyyy}")
            : license.IsTrialActive
                ? $"🕐 Probă gratuită — {license.TrialDaysRemaining} zile rămase"
                : "⚠️ Probă expirată — activează un cod ca să continui";
        DeactivateButton.Visibility = license.IsLicensed ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CopyMachineIdButton_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText(MachineID.Display);
    }

    private void WhatsAppButton_Click(object sender, RoutedEventArgs e)
    {
        var text = $"Salut! Vreau să activez CursorPro GDC (Windows). Machine ID: {MachineID.Display}";
        var url = WhatsAppLink.Url(text);
        Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
    }

    private void ActivateButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = "";
        var ok = LicenseManager.Shared.Activate(ActivationCodeText.Text);
        if (ok)
        {
            ActivationCodeText.Text = "";
            RefreshLicenseStatus();
        }
        else
        {
            ErrorText.Text = LicenseManager.Shared.ActivationError ?? "Cod invalid.";
        }
    }

    private void DeactivateButton_Click(object sender, RoutedEventArgs e)
    {
        LicenseManager.Shared.Deactivate();
        RefreshLicenseStatus();
    }
}
