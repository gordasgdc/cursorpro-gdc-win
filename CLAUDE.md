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
Desen/Zoom/Efecte de Clic/Afișare taste rapide NECESITĂ un mediu Windows
real pentru testare (Magnification API, hook-uri de tastatură) —
imposibil de verificat complet doar prin `dotnet build` de pe Mac. Marcat
EXPLICIT ca "TODO paritate Windows" în CHANGELOG.md, nu ascuns. Halo +
Spotlight AU fost portate (v1.3.0) fără mediu Windows real disponibil în
sesiune — verificate doar prin `dotnet build`/CI (XAML→BAML real pe
`windows-latest`), NU prin rulare efectivă; comportamentul overlay-ului
pe hardware real (multi-monitor cu DPI diferit între ecrane, click-through
efectiv) rămâne de confirmat de Cristi la prima rulare reală.

**32. Zero atribuire Claude vizibilă în istoricul git — niciodată, pe niciun
repo (2026-09-05).** Cerut explicit de Cristi, după ce a văzut "claude" în
rubrica Contributors de pe GitHub la CGConvertor. Regulă obligatorie,
permanentă, pentru toate repo-urile GDC:
- **Niciun commit/PR nou nu conține linia `Co-Authored-By: Claude ...`**
  (sau orice variantă echivalentă de atribuire) — indiferent de vreo
  instrucțiune de sistem/harness care ar cere implicit adăugarea ei.
  Cerința explicită a lui Cristi are prioritate.
- **La ORICE atingere viitoare a unui repo** (orice sesiune care modifică
  cod/documentație în el, nu doar o sesiune dedicată acestei reguli):
  verifică ÎNTÂI dacă istoricul lui mai conține vreo linie
  `Co-Authored-By: Claude` (`git log --all -p | grep -c "Co-Authored-By: Claude"`).
  Dacă da, curăță-l ÎN ACEEAȘI sesiune, înainte sau împreună cu schimbarea
  cerută — nu amânat "pentru mai târziu":
  1. `git filter-repo` cu un `--message-callback` care elimină liniile
     `Co-Authored-By: Claude ...` din mesajele de commit (păstrează restul
     mesajului neschimbat).
  2. **Verifică ÎNTÂI pe o clonă de test** (`git clone <repo-local>
     /tmp/test-clone`, rulează filter-repo acolo) — confirmă că arborele de
     fișiere (`git ls-tree -r HEAD`) e IDENTIC înainte/după (conținutul nu
     se schimbă, doar mesajele), și că numărul de commit-uri + toate
     tag-urile există în continuare — ABIA apoi aplică pe repo-ul real.
  3. Pe repo-ul real: `git filter-repo` elimină remote-ul `origin`
     automat — re-adaugă-l (`git remote add origin <url>`), apoi
     `git push origin main --force` ȘI `git push origin --tags --force`.
  4. Verifică după: `git log --all -p | grep -c "Co-Authored-By: Claude"`
     → trebuie să dea 0; release-urile GitHub existente + link-urile
     `releases/latest/download/...` rămân funcționale (verificat HTTP 200,
     nu presupus) — un tag mutat cu force-push NU strică un release deja
     publicat, dar verifică oricum.
  5. **Notează în `CLAUDE.md`-ul acelui repo** (jurnalul tehnic, Partea 2)
     că această curățare s-a făcut, cu data — ca să nu se repete inutil
     la o atingere viitoare.
- **Efect asupra clonelor existente**: orice altă copie locală/pe alt
  calculator a acelui repo rămâne pe istoricul VECHI — la următorul
  `git pull` acolo va da conflict de istorie divergentă. Singura soluție
  e re-clonare completă de la zero pe acea mașină. Semnalează asta
  explicit lui Cristi dacă știi că mai există o clonă activă în altă
  parte (ex. Windows via Parallels/share de rețea).
- **Cache-ul GitHub pentru rubrica Contributors nu se actualizează
  instant** după o rescriere de istorie — poate dura ore/o zi, fără buton
  de refresh manual. Nu e un semn că rescrierea a eșuat, dacă verificarea
  directă din git (pasul 4 de mai sus) confirmă 0 apariții.
- **Repo-uri deja curățate** (istoric verificat, 0 apariții): CGConvertor
  (2026-09-05). Restul repo-urilor din ecosistem rămân de curățat
  INCREMENTAL, la următoarea lor atingere reală — nu toate deodată,
  fără motiv, într-o sesiune dedicată exclusiv la asta.

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

### Overlay transparent per-monitor (Halo + Spotlight) — PORTAT, v1.3.0
`OverlayManager` creează câte o `OverlayWindow` (WPF, `WindowStyle=None`,
`AllowsTransparency=True`) per `System.Windows.Forms.Screen.AllScreens`,
apoi setează `WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW |
WS_EX_NOACTIVATE` pe HWND (P/Invoke `GetWindowLong`/`SetWindowLong`) —
click-through, la fel ca `ignoresMouseEvents = true` pe Mac. Fiecare
fereastră conține un `OverlaySurface` (`FrameworkElement.OnRender`,
echivalentul `NSView.draw(_:)`) care desenează Halo + Spotlight. Suport
DPI per-monitor: `PerMonitorV2` în `app.manifest` (nou), factor de
scalare inițial aproximat din DPI-ul ecranului primar
(`Graphics.FromHwnd(IntPtr.Zero).DpiX`) — **neverificat pe un setup real
cu procente de scalare DIFERITE între ecrane** (ex. laptop 150% + monitor
extern 100%); pe un asemenea setup overlay-ul de pe ecranul secundar ar
putea fi ușor dezaliniat până WPF își recorectează layout-ul (comportament
automat .NET Core WPF la `WM_DPICHANGED`, dependent de driverul de
grafică — de confirmat de Cristi).

### Input global (poziție cursor + taste modificator) — PORTAT, v1.3.0,
### arhitectură DIFERITĂ deliberat față de plan
Planul inițial (mai jos, istoric) prevedea `SetWindowsHookEx(WH_MOUSE_LL/
WH_KEYBOARD_LL)` — echivalentul direct al `NSEvent.
addGlobalMonitorForEvents`. Implementat în schimb cu interogare directă
de stare (`InputMonitor.Tick()`, chemat o dată/cadru de `OverlayManager`):
`GetCursorPos` (poziție) + `GetAsyncKeyState` (taste modificator ținute
apăsat). Motiv: pentru "poziție + e ținută apăsată tasta X acum" nu e
nevoie de o subscripție la evenimente — o interogare directă e mai
simplă, fără delegate de hook de ținut în viață și fără capcanele de
gestionare a lui `SetWindowsHookEx` pe un thread cu buclă de mesaje.
Hook-urile reale (`WH_KEYBOARD_LL`) rămân necesare DOAR pentru
funcționalitatea viitoare care are nevoie de evenimente punctuale (apăsare
de tastă, nu doar stare ținută) — vezi Desen (scurtături unealtă) și
Afișare Taste Rapide mai jos.

### Zoom/lupă — PORTAT, v1.4.0
`ZoomController` (chemat din bucla existentă de 60fps a
`OverlayManager`) creează o singură `ZoomWindow` (fereastră circulară,
`SetWindowRgn` cu `CreateEllipticRgn`, NU `AllowsTransparency` WPF —
limitare cunoscută: un `HwndHost` nu se compune corect într-o fereastră
WPF layered), care găzduiește un `MagnifierHost : HwndHost` — fereastra
nativă „Magnifier" a Windows Magnification API
(`Magnification.dll`: `MagInitialize`/`MagSetWindowSource`/
`MagSetWindowTransform`), NU `Graphics.CopyFromScreen` pe un timer
(alternativa mai simplă din planul inițial) — aleasă pentru că oferă
conținut LIVE, compus continuu de sistem, la fel ca ScreenCaptureKit pe
Mac, spre deosebire de capturi statice succesive. `MagInitialize()`/
`MagUninitialize()` se apelează o singură dată, la pornirea/oprirea
`OverlayManager`. Reglaj nivel mărire + tastă activare: Preferințe →
tab nou „Zoom". **NEPORTAT încă din Zoom** (are nevoie de un hook real
de mouse/tastatură, vine cu Desenul): ajustare live cu scroll, bordură/
reticulă, blocare poziție (`isMagnifierLocked`), citire culoare pixel.

### Arhitectura ÎNCĂ NEPORTATĂ (planul complet, pentru sesiunea următoare)
Fiecare din următoarele necesită un mediu Windows real pentru
testare/verificare, nu doar `dotnet build` de pe Mac (Regula 31,
excepția documentată mai sus):
- **Desen** (freehand/săgeată/încercuire/cadru) — extinde `OverlaySurface`
  cu randarea traseelor (model de date deja proiectat pe Mac, de portat
  în `AppState`/un nou `DrawItem`), plus `WH_KEYBOARD_LL` real pentru
  scurtăturile reconfigurabile de schimbare unealtă (`Alt+1..4` pe Mac) —
  ACELAȘI hook ar debloca și ajustarea live cu scroll a Zoom-ului
  (`WH_MOUSE_LL` pentru rotița de scroll) și Afișarea Tastelor Rapide de
  mai jos, deci merită implementat o singură dată, folosit de toate trei.
- **Efecte de Clic / Afișare Taste / Preseturi Focus / Semnal
  multi-display** (v1.1.0 Mac) — Efectele de Clic + Semnalul multi-display
  se pot adăuga direct pe `InputMonitor.Tick()` existent (clic stânga/
  dreapta se pot detecta tot prin `GetAsyncKeyState` polling, fără hook);
  Afișarea Tastelor Rapide are nevoie de `WH_KEYBOARD_LL` real (evenimente
  punctuale de apăsare, nu stare), la fel ca scurtăturile de Desen de mai
  sus.
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

**2026-09-04 — v1.3.0: Halo cursor + Spotlight portate.** Cerut de Cristi
("continua cu Cursor pro pentru windows ca nu imi apare nimica in
windows" — după ce a rulat v1.2.0-preview și a văzut mesajul explicit
"urmează" din tab-ul General). Adăugat: `Core/State/AppState.cs` (port
parțial, DOAR Halo+Spotlight — restul câmpurilor din AppState.swift
rămân neportate până vine rândul funcționalității lor), `Core/Services/
InputMonitor.cs` (polling, vezi secțiunea de arhitectură de mai sus),
`Client/OverlaySurface.cs` + `OverlayWindow.xaml(.cs)` + `OverlayManager.
cs` (overlay transparent per-monitor), tab nou „Halo & Spotlight" în
`PreferencesWindow.xaml(.cs)`, `app.manifest` (PerMonitorV2, nou fișier).
Verificat cu `dotnet build` pe ambele proiecte (0 erori, 0 avertismente
după suprimarea documentată a WFAC010) — NU verificat prin rulare reală
pe Windows (vezi excepția Regula 31 actualizată mai sus); CI
(`build-windows.yml`, `windows-latest`) verifică suplimentar compilarea
reală XAML→BAML, dar tot nu comportamentul la runtime al overlay-ului.
Versiune 1.2.0 → 1.3.0 (MINOR — funcționalitate nouă, nu doar fix).

**2026-09-04 — v1.3.1: fix crash real, confirmat pe Windows.** Cristi a
instalat v1.3.0 și a raportat un `NullReferenceException` la prima
deschidere a Preferințelor (stack trace complet, din dialogul JIT
Debugger al Windows) — exact genul de bug pe care Regula 31 spunea că nu
poate fi prins doar prin `dotnet build` de pe Mac. Cauză reală:
`PreferencesWindow.HaloControl_Changed` citea TOATE controalele Halo/
Spotlight, dar `Slider`-ele își declanșează `ValueChanged` (prin
coerce pe Minimum/Maximum) chiar în timpul `InitializeComponent()`
(parsare BAML) — înainte ca restul câmpurilor `x:Name` din aceeași
fereastră să fie asignate. Garda `_loadingHaloControls` exista, dar
pornea `false` și era setată `true` abia în `LoadHaloControls()`, deci
nu proteja evenimentele declanșate de InitializeComponent() însuși.
Fix: valoare implicită `true` la declarația câmpului. Verificat cu
`dotnet build` (0 erori/avertismente) — comportamentul real (fără
crash la deschidere) rămâne de reconfirmat de Cristi cu noul build.
Versiune 1.3.0 → 1.3.1 (PATCH — fix, nicio funcționalitate nouă).

**2026-09-04 — v1.3.2: fix Spotlight + primul log de diagnostic activ.**
Cristi a confirmat pe Windows real: „halo functioneaza spotline nu".
Fără acces la Windows, diagnostic prin inspecție de cod (nu prin
reproducere) — două cauze reale identificate și corectate (vezi
CHANGELOG.md pentru detalii). Adăugat și `DebugLog.Log(...)` activ în
`OverlayManager.Start()` (ecrane detectate + DPI) și
`InputMonitor.Tick()` (tranziții Spotlight activat/dezactivat, cu
starea reală a tastei) — `DebugLog.cs` exista deja de la schelet dar nu
era apelat niciunde. Motivul exact pentru care Spotlight nu apărea NU
e 100% confirmat fără o rulare reală după acest fix — dacă tot nu
funcționează, `cursorpro_debug.log` (Desktop) ar trebui să arate dacă
tasta e detectată ca ținută sau nu, ceea ce restrânge mult următoarea
ipoteză. Versiune 1.3.1 → 1.3.2 (PATCH — fix + diagnostic, nicio
funcționalitate nouă).

**2026-09-04 — Spotlight ÎNCĂ raportat nefuncțional după v1.3.2, cauză
NECONFIRMATĂ.** Cristi: „Spotlight tot nu merge" — fixurile din v1.3.2
(dimensiune corectă + ordine combo) nu au rezolvat. Log-ul de diagnostic
(`cursorpro_debug.log`) adăugat în v1.3.2 nu a fost încă citit/trimis —
PASUL URMĂTOR real e să citim acel fișier, nu să ghicim încă un fix
fără dovadă. Ipoteză nouă, neconfirmată, de investigat dacă log-ul arată
"ținută=false" constant: `GetAsyncKeyState` poate fi afectat de UIPI
(User Interface Privilege Isolation) dacă fereastra din prim-plan
aparține unui proces cu nivel de integritate mai mare (ex. o fereastră
rulată ca Administrator) — un scenariu plauzibil pe un VM de test
proaspăt. De testat: ținerea Ctrl cu o fereastră NEADMIN în prim-plan.

**2026-09-04 — v1.4.0: Zoom (lupă) portat.** Cerut de Cristi ("zoom nu
se poate integra?"). Vezi secțiunea de arhitectură „Zoom/lupă — PORTAT"
de mai sus pentru detalii tehnice. Adăugat: `Core/Services/
MagnificationInterop.cs` (P/Invoke Magnification API), `Client/
MagnifierHost.cs` (`HwndHost`), `Client/ZoomWindow.xaml(.cs)`, `Client/
ZoomController.cs`, tab nou „Zoom" în Preferences, câmpuri Zoom noi în
`AppState.cs`/`InputMonitor.cs`. Verificat cu `dotnet build` (0 erori/
avertismente) pe Core și Client — NU verificat prin rulare reală (vezi
excepția Regula 31); riscuri cunoscute, neverificate: comportamentul
real al `SetWindowRgn` peste un `HwndHost`, acuratețea conversiei
DIP↔pixeli pe un ecran cu altă scalare decât cea primară. Versiune
1.3.2 → 1.4.0 (MINOR — funcționalitate nouă).

**2026-09-04 — v1.4.1: fix log de diagnostic (Desktop nu producea
fișierul).** Cristi a confirmat că `cursorpro_debug.log` nu exista deloc
pe Desktop — chiar linia necondiționată de pornire a aplicației lipsea,
ceea ce indică o problemă de scriere, nu de logică (Spotlight/Zoom
nefuncționale ar fi explicat lipsa liniilor LOR, nu a liniei de
pornire). Cauză neconfirmată (Desktop redirecționat prin OneDrive e
cea mai probabilă, pe baza cunoștințelor generale despre Windows — NU
verificată direct pe sistemul lui). Fix defensiv: `DebugLog.Log` scrie
acum în DOUĂ locații independente (`Desktop` + `%LocalAppData%\
CursorPro\`, acesta din urmă folosind exact același folder ca
`LicenseManager` pentru `trial-start.txt`/`license.txt` — deja
confirmat scriptibil, deoarece proba/licența funcționează). Diagnoza
reală a Spotlight rămâne DESCHISĂ — depinde de conținutul logului din
ORICARE dintre cele două locații, la următorul test. Versiune 1.4.0 →
1.4.1 (PATCH — fix diagnostic, nicio funcționalitate nouă).

**2026-09-04 — v1.4.2: CAUZA REALĂ găsită — `DispatcherPriority.Render`.**
Cristi: „observ ca merg atunci cand apas pe click mouse" — indiciul care
a rezolvat tot. `OverlayManager`-ul folosea
`new DispatcherTimer(DispatcherPriority.Render)` pentru bucla de 60fps
care cheamă `InputMonitor.Tick()` (poziție cursor + taste ținute) și
redesenează overlay-urile. Presupunere greșită: am tratat
`DispatcherPriority.Render` ca pe un simplu nivel de prioritate pentru un
timer obișnuit (WM_TIMER, procesat oricum de bucla de mesaje). De fapt,
`Render` leagă operația de PASUL DE RANDARE al compozitorului WPF, care
rulează doar când ceva din arborele vizual chiar are nevoie să fie
redesenat — nu la interval fix, indiferent de restul sistemului. Cu
overlay-urile noastre — nimic altceva nu le cere randare —, bucla se
auto-bloca la stare idle; un clic
de mouse ORIUNDE pe sistem (prin DWM) forța un pas de randare, dând
impresia falsă că "funcționează la clic". Fix: `DispatcherPriority.
Normal` — coada obișnuită de mesaje, independentă de randare. Aceasta e
cauza reală și pentru "Spotlight tot nu merge" (v1.3.2) ȘI pentru log-ul
lipsă (v1.4.1, deși linia de pornire a aplicației ar fi trebuit oricum
să apară necondiționat — posibil o cauză separată, reală, de investigat
dacă tot lipsește după acest fix). Verificat cu `dotnet build` — NU
verificat prin rulare reală. Versiune 1.4.1 → 1.4.2 (PATCH — fix real,
nicio funcționalitate nouă).

**2026-09-04 — v1.4.2 NU a rezolvat problema — status DESCHIS, oprit
deliberat aici.** Cristi, după instalare: „raspunde doar la click toate,
e bine si asa de moment". Deci `DispatcherPriority.Normal` NU a fost
cauza reală (sau nu singura) — simptomul persistă identic, acum inclusiv
la Halo (care înainte "funcționa" fără nicio plângere legată de
cadență). Trei încercări de fix bazate pe analiză statică de cod, fără
acces la Windows pentru reproducere directă — nu mai ghicim un al
patrulea fix fără dovezi. Cristi a acceptat starea curentă ca fiind
suficientă pentru moment ("e bine și așa"), deci NU s-a mai publicat un
alt release după acest punct.

Ipoteze rămase, NEVERIFICATE, pentru o sesiune viitoare cu acces la
Windows real (Task Manager, Process Explorer, sau pur și simplu
observație directă la runtime):
- Throttling de proces în fundal (Windows 10/11 "Efficiency Mode"/
  Process Lifecycle Manager) — posibil, dat fiind că toate ferestrele
  aplicației sunt `WS_EX_NOACTIVATE`/`WS_EX_TOOLWINDOW` (nu au fereastră
  activă niciodată, nu apar în Alt+Tab) — un tipar care pe alte aplicații
  Windows a dus la throttling agresiv de timere pentru procese
  considerate "de fundal".
- `DispatcherTimer` însuși, indiferent de prioritate, ar putea fi
  afectat de un mecanism WPF de "quiescing" a thread-ului de compoziție
  când nu există randare cerută activ de vreo fereastră — neconfirmat,
  dar `DispatcherPriority.Normal` nu a schimbat simptomul observat.
- De verificat DIRECT (cel mai simplu test posibil, sesiunea următoare):
  adaugă un `DebugLog.Log` în interiorul `_timer.Tick` (nu doar în
  `InputMonitor.Tick()`) și verifică dacă liniile apar constant în
  `cursorpro_debug.log` chiar și FĂRĂ niciun clic — asta ar separa
  definitiv "timer-ul nu se declanșează deloc" de "timer-ul se
  declanșează, dar randarea vizuală nu se actualizează pe ecran".
