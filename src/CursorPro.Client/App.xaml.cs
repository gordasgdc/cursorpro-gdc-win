using System.Windows;
using System.Windows.Forms;
using CursorPro.Core.Services;
using Application = System.Windows.Application;

namespace CursorPro.Client;

public enum PreferencesTab
{
    General,
    License,
}

/// Punctul de intrare — echivalentul AppDelegate.swift (Mac): construiește
/// iconița din tray + meniul contextual, nicio fereastră principală.
///
/// SCOP ACTUAL (2026-09-04, v1.3.0): tray icon + Preferințe
/// (General/Licență) + Halo cursor + Spotlight (overlay transparent
/// per-monitor, poziție cursor + taste modificator prin polling
/// GetCursorPos/GetAsyncKeyState — vezi OverlayManager/InputMonitor).
/// Desen/Zoom/Efecte de Clic/Afișare taste rapide — NEPORTATE încă, vezi
/// CHANGELOG.md "TODO paritate Windows" și CLAUDE.md pentru planul
/// complet (Magnification API pentru lupă, hook-uri de tastatură pentru
/// scurtături).
public partial class App : Application
{
    private NotifyIcon? _trayIcon;
    private PreferencesWindow? _preferencesWindow;
    private readonly OverlayManager _overlayManager = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DebugLog.Log($"CursorPro GDC {AppVersion} pornit — vezi %USERPROFILE%\\Desktop\\cursorpro_debug.log pentru diagnostic.");

        // Pornește proba/încarcă licența salvată o singură dată, la
        // lansare — la fel ca LicenseManager.shared pe Mac (init lazy,
        // primul acces îl declanșează).
        _ = LicenseManager.Shared;
        LicenseManager.Shared.Changed += RebuildContextMenu;

        BuildTrayIcon();

        // Halo + Spotlight — pornesc mereu la lansare, ca pe Mac
        // (InputMonitor.start() e chemat necondiționat din AppDelegate;
        // gating-ul pe licență se face în AppState/InputMonitor, nu aici).
        _overlayManager.Start();
    }

    private void BuildTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            // TODO: iconiță reală — Assets\app.ico, generată din același
            // master 1024px ca AppIcon.icns (Mac), vezi GDCVaultWin pentru
            // tipar. Placeholder explicit, NU o iconiță finală.
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            Text = "CursorPro GDC",
        };
        _trayIcon.MouseClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Left) OpenPreferences();
        };
        RebuildContextMenu();
    }

    private void RebuildContextMenu()
    {
        if (_trayIcon is null) return;
        var license = LicenseManager.Shared;

        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem($"CursorPro GDC — v{AppVersion}") { Enabled = false });
        menu.Items.Add(new ToolStripSeparator());

        var licenseText = license.IsLicensed
            ? "✅ Licențiat"
            : license.IsTrialActive
                ? $"🕐 Probă — {license.TrialDaysRemaining} zile rămase"
                : "⚠️ Probă expirată";
        menu.Items.Add(new ToolStripMenuItem(licenseText, null, (_, _) => OpenPreferences(PreferencesTab.License)));
        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add(new ToolStripMenuItem("Preferințe…", null, (_, _) => OpenPreferences(PreferencesTab.General)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Închide CursorPro GDC", null, (_, _) => Shutdown()));

        _trayIcon.ContextMenuStrip = menu;
    }

    private static string AppVersion =>
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?";

    private void OpenPreferences(PreferencesTab tab = PreferencesTab.General)
    {
        if (_preferencesWindow is null)
        {
            _preferencesWindow = new PreferencesWindow();
            _preferencesWindow.Closed += (_, _) => _preferencesWindow = null;
        }
        _preferencesWindow.SelectTab(tab);
        _preferencesWindow.Show();
        _preferencesWindow.Activate();
        _preferencesWindow.WindowState = WindowState.Normal;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _overlayManager.Stop();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
