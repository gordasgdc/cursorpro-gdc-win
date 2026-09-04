# Changelog — CursorPro GDC (Windows)

Jurnal scurt, orientat spre utilizator. Complementar jurnalului tehnic
detaliat din CLAUDE.md.

## v1.3.1 (2026-09-04) — Fix: crash la deschiderea Preferințelor

Prima deschidere a ferestrei de Preferințe (după instalarea v1.3.0)
arunca `NullReferenceException` la pornire — controalele Halo & Spotlight
(sloidere/combo-uri) își declanșau propriul eveniment chiar în timpul
construirii ferestrei, înainte ca toate să fie gata, iar handler-ul le
citea pe toate deodată. Confirmat și reprodus pe Windows real, remediat.

## v1.3.0 (2026-09-04) — Halo cursor + Spotlight, reale

**Ce e nou**: Halo cursor (inel/umplut/cruce, culoare/diametru/grosime
configurabile) și Spotlight (întunecă ecranul în jurul cursorului, se
activează ținând apăsată o tastă configurabilă — implicit Ctrl) —
funcționează acum pe Windows, pe TOATE ecranele conectate, exact ca pe
Mac. Configurabile din Preferințe → tab nou „Halo & Spotlight".

**Arhitectură** (adaptare deliberată față de Mac, vezi CLAUDE.md):
poziția cursorului și starea tastelor modificator se citesc direct
(`GetCursorPos`/`GetAsyncKeyState`, interogare, nu abonare la evenimente)
în loc de hook-uri globale (`SetWindowsHookEx`) — echivalent funcțional
cu monitoarele globale NSEvent de pe Mac, pentru acest caz de folosire,
fără capcanele de gestionare a hook-urilor native. Câte o fereastră
transparentă, click-through, per ecran conectat
(`WS_EX_LAYERED`/`WS_EX_TRANSPARENT`), cu suport DPI per-monitor
(`PerMonitorV2` în `app.manifest`, nou).

**Neschimbat / tot NEPORTAT**: Desen, Zoom (lupă), Efecte de Clic,
Afișare Taste Rapide, Preseturi Focus, Semnal multi-display — vezi lista
completă mai jos (v1.2.0).

## v1.2.0-preview (2026-09-04) — Primul build public, marcat explicit Preview

Publicat DELIBERAT ca preview (`gh release`, `--prerelease` scos ulterior
ca `releases/latest/download/` să funcționeze — vezi titlul release-ului,
care rămâne clar "Early Preview"), nu ca lansare completă: doar tray icon
+ Licență funcționează, exact ca în schelet (v1.2.0 de mai jos). Cerut
explicit de Cristi ("să-l descarc ca și client, să văd cum funcționează")
— fără să pretindă că Halo/Spotlight/Desen/Zoom există deja. Asset-uri:
`CursorProGDC-Windows-1.2.0-preview.exe` (nume versionat, Regula 17) +
`CursorProGDCSetup.exe` (nume stabil, pentru linkul `releases/latest/
download/` de pe gordas.dev/cursorpro-gdc).

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
