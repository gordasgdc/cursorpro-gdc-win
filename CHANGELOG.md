# Changelog — CursorPro GDC (Windows)

Jurnal scurt, orientat spre utilizator. Complementar jurnalului tehnic
detaliat din CLAUDE.md.

## v1.2.0 (2026-09-04) — Primul schelet

Repo nou, arhitectură nativă C# / .NET 8 / WPF (aceeași bază ca
GDCPluginManagerWin/GDCVaultWin). Versiunea de pornire (`1.2.0`) e
aliniată cu ultima versiune Mac publicată, nu un `0.1.0` — la fel ca restul
clienților Windows din ecosistem.

**Ce funcționează**: iconiță în system tray + meniu contextual, fereastră
de Preferințe cu tab Licență complet funcțional (activare/dezactivare cod,
Machine ID, probă 3 zile, WhatsApp) — port 1:1 din `LicenseCore.swift`/
`LicenseManager.swift`/`MachineID.swift`/`WhatsAppLink.swift` (Mac), aceeași
cheie publică Ed25519, aceleași coduri generate din Furnizor funcționează
neschimbate pe ambele platforme.

**TODO paritate Windows/Mac — NEPORTAT încă, explicit, nu uitat**:
- Halo cursor, Spotlight, Desen (freehand/săgeată/încercuire/cadru),
  Zoom (lupă) — necesită overlay transparent per-monitor
  (`WS_EX_LAYERED`/`WS_EX_TRANSPARENT`) + hook-uri globale de input
  (`SetWindowsHookEx`) + Magnification API (`Magnification.dll`) pentru
  lupă — toate cer un mediu Windows real pentru testare/verificare, nu
  doar `dotnet build` de pe Mac.
- Efecte de Clic, Afișare Taste Rapide, Preseturi Focus, Semnal
  multi-display (v1.1.0 Mac) — depind de InputMonitor/OverlayView, deci
  vin odată cu portul de mai sus.
- Update Checker + Self-Updater real (Regula 20) — pattern deja verificat
  în GDCVaultWin, de portat.
- Pricing Manager dinamic (Regula 27) — TODO, ca la restul ecosistemului.
- Iconiță reală (`Assets\app.ico`, din același master 1024px ca
  `AppIcon.icns`) — placeholder `SystemIcons.Application` momentan.
- Ghid PDF (RO/EN/ES) accesibil din Preferințe → Ajutor.
- Instalator: `installer.iss`/CI create și funcționale (verificate prin
  `dotnet build`/`ISCC.exe` în CI), dar NU încă rulat manual, o dată, pe
  Windows real (Regula 20 — pasul de instalare nu poate fi verificat
  automat de Claude).
