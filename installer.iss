; Instalator Windows pentru CursorPro GDC, cu Inno Setup
; (https://jrsoftware.org/isinfo.php — gratuit) — port 1:1 al tiparului
; installer.iss din GDCVaultWin/GDCPluginManagerWin.
;
; CI-ul (.github/workflows/build-windows.yml) face toti pasii automat.
; Pentru compilare MANUALA, pe Windows, cu Inno Setup Compiler instalat
; (gratuit, https://jrsoftware.org/isdl.php):
;   1. dotnet publish src\CursorPro.Client -c Release -r win-x64 --self-contained -o publish
;   2. Deschide acest fisier (installer.iss) cu Inno Setup Compiler
;   3. Apasa "Compile" (sau F9)
;   4. Rezultatul apare in Output\CursorProGDCSetup.exe

#define MyAppName "CursorPro GDC"
#define MyAppVersion "1.3.1"
#define MyAppPublisher "Cristi Gordas"
#define MyAppExeName "CursorPro.exe"
#define MyAppURL "https://gordas.dev/cursorpro-gdc"

[Setup]
AppId={{B7E1F4A2-3C6D-4A8F-9E1B-CURSORPROWIN1}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\GDC\CursorPro GDC
DefaultGroupName=CursorPro GDC
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=CursorProGDCSetup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
LicenseFile=installer\license.txt
; Nu semnat cu certificat platit — Windows SmartScreen arata un
; avertisment "Unrecognized app" la prima rulare a instalatorului, la fel
; ca la restul aplicatiilor GDC (nesemnate). Creste prioritatea odata cu
; un certificat oficial (vezi nota de transparenta din landing page).
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Dezinstaleaza {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

; REGULA PERMANENTA de Clean Uninstall (gdc-plugin-manager-catalog-vendor/CLAUDE.md,
; Regula 6/18): dezinstalarea trebuie sa stearga TOT ce a scris aplicatia,
; nu doar folderul din Program Files. %LocalAppData%\CursorPro contine
; trial-start.txt si license.txt (LicenseManager.cs). Daca o versiune
; viitoare adauga un fisier persistent nou in alta parte (Registry,
; %AppData%), adauga stergerea lui aici, in acelasi commit.
[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\CursorPro"
