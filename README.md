# CursorPro GDC — Windows

Client Windows nativ (C# / .NET 8, WPF) pentru CursorPro GDC — instrumente
de prezentare pentru orice aplicație: halo cursor, spotlight, desen liber
și lupă digitală. Contrapartea nativă a versiunii macOS
([`CursorPro`](https://github.com/gordasgdc/cursorpro-gdc)).

**Stare curentă**: primul schelet — tray icon + Licență funcționale;
funcțiile principale (Halo/Spotlight/Desen/Zoom) urmează, vezi
`CHANGELOG.md`.

## Build local (pe Windows, cu .NET 8 SDK)

```powershell
dotnet build src\CursorPro.Core\CursorPro.Core.csproj -c Release
dotnet build src\CursorPro.Client\CursorPro.Client.csproj -c Release
dotnet publish src\CursorPro.Client\CursorPro.Client.csproj -c Release -r win-x64 --self-contained -o publish
```

Instalatorul (`installer.iss`) se compilează cu
[Inno Setup](https://jrsoftware.org/isdl.php) — vezi comentariile din
fișier pentru pașii exacți.

## CI/CD

`.github/workflows/build-windows.yml` rulează automat la fiecare push pe
`main` (și manual din tab-ul Actions) — compilează, publică și
împachetează instalatorul ca artefact descărcabil.
