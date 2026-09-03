# CursorPro GDC (Windows) — reguli de arhitectură

> **[SYSTEM DIRECTIVE FOR CLAUDE: DO NOT DELETE OR OVERWRITE EXISTING RULES. ONLY APPEND NEW RULES.]**
> Jurnal viu, nu document care se rescrie. La orice actualizare, adaugă la finalul secțiunii potrivite — nu șterge/înlocui reguli vechi decât dacă sunt explicit invalidate de o schimbare reală (și atunci marchează-le **[ÎNVECHIT]** cu motivul, nu le șterge din istoric).

Citit automat de Claude Code la fiecare sesiune în acest repo. Client Windows nativ (C#/.NET 8, WPF), contrapartea `CursorPro` (Mac). Vezi și `CursorPro/CLAUDE.md` (Mac) pentru arhitectura originală pe care acest repo o portează.

## [PARTEA 1: REGULI GLOBALE ECOSISTEM GDC — identică în toate proiectele GDC]

> Acest bloc e sincronizat manual în `CLAUDE.md`-ul TUTUROR proiectelor din
> `~/Developer/` (CGConvertor, CursorPro, CursorProWin, DataMover, GDCPluginManager,
> GDCPluginManagerWin, GDCVault, GDCVaultWin, gdc-plugin-manager-catalog-vendor,
> gdc-plugin-manager-files, gdc-production-manager, gdc-resolve-encoder, și
> orice proiect GDC nou). Dacă modifici o regulă aici, propag-o manual și în
> celelalte fișiere — nu există un fișier partajat/include, fiecare
> `CLAUDE.md` e citit independent per-repo.

**1. Directoare & structură.** Toate proiectele GDC trăiesc exclusiv în
`~/Developer/<NumeProiect>/`, niciodată în `~/Downloads` sau `~/Desktop`
(curățate automat de CleanMyMac/Hazel pe acest Mac — au șters repo-uri de
sursă în trecut). Niciun repo nou nu se creează/clonează în afara
`~/Developer/`. Certificatele Apple (`.p12`/`.cer`) și orice cheie privată
(`.p8`/`.key`/`.pem`/`.mobileprovision`) stau EXCLUSIV în
`~/Developer/Certificates/` (folder în afara oricărui repo git) — niciodată
comise, indiferent de `.gitignore`.

**2. Securitate — zero secrete în git.** `.git/config` nu conține niciodată
un token în clar în URL-ul remote-ului (`https://user:TOKEN@github.com/...`)
— autentificare exclusiv prin `gh` (credential helper) sau SSH. Orice token
găsit expus se elimină din config imediat; revocarea efectivă din GitHub
Settings e un pas manual al lui Cristi (Claude nu poate revoca un token).
Un secret comis vreodată în istoricul git (verificat cu
`git log --all -p | grep` sau echivalent) trebuie semnalat explicit, nu doar
curățat din starea curentă.

**3. Licențiere & Donație.** Toate aplicațiile standalone GDC folosesc
`LicenseCore`/`MachineID` (Ed25519, aceeași cheie publică hardcodată în tot
ecosistemul — copiată byte-for-byte, NU printr-o dependință de pachet
între repo-uri). Valoarea susținerii aplicației se exprimă EXCLUSIV ca
**donație** — NICIODATĂ cu cuvintele „preț", „cumpără" sau „vânzare"
(RO/EN/ES). Formularea trebuie să apară clar în: UI-ul aplicației
(ecran/pop-up de licență), ghidul PDF, și orice pagină web dedicată.

**4. Manager de Dependențe (Standard GDC, opt-in).** Aplicația de bază
rămâne lightweight — orice dependință externă opțională/grea se descarcă
LA CERERE, nu bundle-uită implicit dacă poate fi evitat. Indicator global
🔴/🟢 vizibil în header/meniu: verde doar dacă TOATE componentele
obligatorii sunt OK.

**5. Instalare Autonomă.** Windows: installer Inno Setup cu
`DefaultDirName={autopf}\GDC\<App>` (Program Files), scurtături automate
Desktop + Start Menu, dezinstalare nativă prin "Apps & Features".

**6. Packaging.** Windows: instalatorul (`.exe`) + dezinstalare curată
prin `[UninstallDelete]` (Inno Setup) pentru orice fișier scris în
`%LocalAppData%`/Registry.

**7. UI Standard — varianta "Shift".** Temă dark, profesională, accent
cald cupru/amber sau altă culoare distinctă per-aplicație. Număr de
versiune vizibil în UI, fără excepție. Update Checker automat la lansare +
verificare manuală.

**8. Documentație PDF — standard ultra-detaliat.** Orice ghid PDF (RO/EN/ES)
se redactează pentru un utilizator complet începător, zero presupuneri.

**9-10.** Site-ul public trebuie să pointeze mereu la
`releases/latest/download/...`. Fiecare `CLAUDE.md` rămâne un jurnal
append-only.

**11. Sincronizare dinamică a Standardului Master.** Orice
adăugare/modificare a unei reguli globale din Partea 1 — indiferent din ce
proiect pornește — devine automat noul Standard Master și TREBUIE
propagată manual în `CLAUDE.md`-ul tuturor celorlalte proiecte. Orice
aplicație NOUĂ primește Partea 1 completă încă din primul `CLAUDE.md`.

**12-19.** Vezi `CursorPro/CLAUDE.md` (Mac) sau
`gdc-plugin-manager-catalog-vendor/CLAUDE.md` pentru textul complet
(Profil Utilizator/Revocare Licențe, Update Checker UX, Versionare
semantică, fișiere descărcabile cu versiune în nume, Standard
UX/Arhitectură aplicație nouă, Regulă Legală & Packaging UE/Global) —
nereproduse aici cuvânt cu cuvânt ca să nu divergă fișierul; aplică-le
identic când relevante pentru acest repo.

**14. Versionare semantică obligatorie la FIECARE schimbare.** Format
`MAJOR.MINOR.PATCH`. Sincron în TOATE punctele care îl țin (`.csproj`
`<Version>`, `installer.iss` `MyAppVersion`, `docs/update.json` dacă
aplicabil). Un bump fără schimbare reală de cod e la fel de greșit ca
schimbarea de cod fără bump.

**20. Self-Updater real — obligatoriu, niciodată deschidere de browser/
GitHub.** Windows: descarcă installer-ul (`.exe`) cu `HttpClient` direct
pe disc, redenumit cu versiunea, apoi îl lansează
(`Process.Start(UseShellExecute:true)`) — fereastra NATIVĂ Inno Setup
apare, NICIODATĂ browserul. Vezi `SelfUpdater.cs` (`GDCPluginManagerWin`,
`GDCVaultWin`) ca implementare de referință. **Status acest repo: TODO,
neportat încă** — vezi CHANGELOG.md.

**21. Memory & I/O Performance.** Obligatoriu doar pentru aplicații care
procesează fișiere/fluxuri mari. **Nu se aplică** — CursorPro nu
procesează fișiere mari (overlay + capturi mici de ecran pentru lupă).

**22. `PlatformTarget` explicit obligatoriu pentru orice proiect .NET/WPF
cu pachete NuGet native.** `ArchitecturesInstallIn64BitMode=x64compatible`
în `installer.iss` acoperă asta la nivel de instalator; dacă se adaugă
vreodată o dependință nativă (native NuGet), setează explicit
`<PlatformTarget>x64</PlatformTarget>` în `.csproj`.

**25. `CHANGELOG.md` obligatoriu la fiecare bump de versiune + Log de
Diagnostic permanent.** `DebugLog.cs` (Core) — un singur fișier de log,
`%USERPROFILE%\Desktop\cursorpro_debug.log`, port 1:1 al
`DebugLog.swift` (Mac).

**28. Auditul licenței active NU e opțional la nicio modificare de
licențiere.** Verifică explicit — cu `grep`, nu presupunere — că
`IsUnlocked` e efectiv REFERENȚIAT într-un `if`/`guard` care blochează o
acțiune reală, nu doar afișat într-un banner informativ.

**29. Zero informație internă în orice loc PUBLIC** (release notes GitHub,
fișiere comise într-un repo public, commit messages vizibile). Acest repo
e PUBLIC (`cursorpro-gdc-win`) — nimic cu nume proprii/citate/cauze de
debugging în `gh release create`/`edit`.

**30. Zero cod "impur" sau nelalocul lui — orice implementare TREBUIE
finalizată complet, nu doar compilată.** O funcționalitate nouă/modificată
se declară "gata" abia după ce TOATE piesele ei sunt implementate și
verificate — cod, build, versiune sincronizată, paritate Mac/Windows dacă
aplică, `CHANGELOG.md`. O piesă lăsată "pentru mai târziu" se spune
EXPLICIT (vezi CHANGELOG.md — lista TODO paritate de mai jos e exact
acest lucru, nu ascunsă).

**31. Paritate Mac/Windows imediată, în aceeași sesiune.** Orice schimbare
de cod pe Mac care are echivalent Windows se portează 1:1 ÎN ACEEAȘI
SESIUNE. Excepție reală pentru acest repo (nu o abatere): funcțiile
Halo/Spotlight/Desen/Zoom/Taste rapide NECESITĂ un mediu Windows real
pentru testare (overlay-uri `WS_EX_LAYERED`, hook-uri globale, Magnification
API) — imposibil de verificat complet doar prin `dotnet build` de pe Mac.
Marcat EXPLICIT ca "TODO paritate Windows" în CHANGELOG.md, nu ascuns.

## [PARTEA 2: SPECIFICAȚII TEHNICE PROIECT]

### Structura repo-ului
- `src/CursorPro.Core/` — model de date + servicii comune, fără UI
  (`LicenseCore.cs`, `LicenseManager.cs`, `MachineID.cs`, `WhatsAppLink.cs`,
  `DebugLog.cs`) — port 1:1 din `Sources/CursorPro/*.swift` (repo Mac).
  `net8.0-windows` (nu `net8.0` simplu — `System.Management`/WMI e
  Windows-only, la fel ca la GDCVault.Core).
- `src/CursorPro.Client/` — aplicația WPF: `App.xaml(.cs)` (tray icon,
  fără fereastră principală — echivalentul `LSUIElement`/`AppDelegate.swift`
  Mac), `PreferencesWindow.xaml(.cs)` (echivalentul
  `PreferencesWindowController.swift`).
- `installer.iss` + `installer/license.txt` — instalator Inno Setup, port
  1:1 al tiparului `GDCVaultWin`.
- `.github/workflows/build-windows.yml` — CI/CD, identic ca structură cu
  `gdc-vault-win` (mai simplu decât `GDCPluginManagerWin`, care are un
  pas suplimentar de secret pentru `PrivateCatalogAuth.cs` — CursorPro nu
  are un repo privat de fișiere, deci nu e nevoie de acel pas).

### De ce `net8.0-windows` și nu `net8.0` pe Core
`GDCPluginManagerWin` (Core) e `net8.0` simplu — nu are nicio dependință
Windows-only. `GDCVaultWin`/`CursorProWin` (acest repo) au nevoie de
`System.Management` (WMI, pentru `MachineID`) direct în Core, deci
`net8.0-windows` de la început — verificat prin analogie cu
`GDCVault.Core.csproj`, nu presupus.

### Arhitectura NEPORTATĂ încă (planul complet, pentru sesiunea următoare)
Fiecare din următoarele necesită un mediu Windows real pentru
testare/verificare, nu doar `dotnet build` de pe Mac (Regula 31,
excepția documentată mai sus):
- **Overlay transparent per-monitor** (Halo/Spotlight/Desen) — o fereastră
  WPF per ecran, `WS_EX_LAYERED | WS_EX_TRANSPARENT` (click-through, la
  fel ca `ignoresMouseEvents = true` pe Mac), poziționată pe fiecare
  `System.Windows.Forms.Screen.AllScreens`, cu suport per-monitor DPI
  (`PerMonitorV2` în `app.manifest` — de creat).
- **Input global** — echivalentul `NSEvent.addGlobalMonitorForEvents`:
  `SetWindowsHookEx(WH_MOUSE_LL)`/`WH_KEYBOARD_LL` (P/Invoke,
  `user32.dll`) — NU există un echivalent managed nativ în WPF.
- **Zoom/lupă** — Windows Magnification API (`Magnification.dll`,
  P/Invoke: `MagInitialize`, `MagSetWindowSource`, fereastră magnifier
  child) în loc de ScreenCaptureKit (Mac). Alternativ (mai simplu, mai
  puțin performant): `Graphics.CopyFromScreen` pe un timer, într-o
  fereastră mică — de evaluat care se potrivește mai bine cerinței de
  "CPU 0% la staționare".
- **Efecte de Clic / Afișare Taste / Preseturi Focus / Semnal
  multi-display** (v1.1.0 Mac) — toate depind de InputMonitor +
  OverlayView de mai sus; vin în același pas.
- **Update Checker + Self-Updater** — port direct din `GDCVaultWin`
  (deja funcțional acolo), doar schimbat URL-ul de releases la
  `gordasgdc/cursorpro-gdc-win`.

### Jurnal tehnic

**2026-09-04 — Repo creat, primul schelet.** `gh repo create
gordasgdc/cursorpro-gdc-win --public` (rulat manual de Cristi — creare de
repo blocată de clasificatorul automat al mediului Claude Code, acțiune
ireversibilă către exterior). Scaffold local + push inițial: Core
(License/MachineID/WhatsApp/DebugLog, port verificat byte-for-byte
compatibil cu Mac — aceeași cheie publică Ed25519) + Client (tray icon +
Preferințe/Licență) + CI/CD (adaptat din `gdc-vault-win`, cel mai apropiat
ca arhitectură — fără dependința de `PrivateCatalogAuth` a
`GDCPluginManagerWin`) + installer.iss + license.txt. Verificat cu
`dotnet build` (`EnableWindowsTargeting=true`, de pe Mac) — 0 erori pe
Core și Client. NU verificat încă: XAML→BAML real (necesită Windows/CI),
tray icon real la runtime, instalatorul compilat efectiv.
