using System.Diagnostics;
using System.Windows;
using CursorPro.Core.Services;

namespace CursorPro.Client;

/// Fereastra de Preferințe — echivalentul PreferencesWindowController.swift
/// (Mac). Doar tab-urile General (placeholder) și Licență sunt reale în
/// acest prim schelet; restul (Halo/Spotlight/Desen/Zoom/Taste) urmează.
public partial class PreferencesWindow : Window
{
    public PreferencesWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Versiune {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?"}";
        MachineIdText.Text = MachineID.Display;
        RefreshLicenseStatus();
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
