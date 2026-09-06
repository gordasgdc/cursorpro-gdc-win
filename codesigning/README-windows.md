# codesigning/ — semnare Windows (Self-Signed, testare internă)

Acoperă DOAR semnarea Windows a CursorPro GDC (adăugată 2026-09-06,
CLAUDE.md Regula 34, port direct din `CGConvertor/codesigning/`).

## Certificat COMUN pentru toate aplicațiile GDC

Certificatul (și secretele CI care îl poartă) e **COMUN pentru toate
aplicațiile GDC** (decizie explicită a lui Cristi) — numele secretelor
GitHub Actions sunt IDENTICE în toate repo-urile:
`WIN_SELFSIGN_PFX_BASE64` și `WIN_SELFSIGN_PFX_PASSWORD`. Dacă
certificatul a fost deja generat pentru alt repo (ex. CGConvertor), NU se
regenerează aici — Cristi doar încarcă ACELEAȘI valori ca secret și pe
acest repo (`gordasgdc/cursorpro-gdc-win`), separat, o singură dată.

## De ce Self-Signed, și ce NU rezolvă

Un certificat self-signed **nu elimină avertismentul SmartScreen/"Unknown
publisher"** pentru publicul larg — doar un certificat real de la o CA
publică (cu reputație acumulată) sau un certificat EV fac asta. Self-signed
e util STRICT pentru:
- testare internă (buildurile pe care le rulează Cristi însuși),
- distribuire către un cerc restrâns de colaboratori care importă manual
  certificatul public (`.cer`) în Trusted Root o singură dată.

La lansarea comercială publică, planul e Azure Trusted Signing sau un
certificat EV (HSM cloud) — vezi CLAUDE.md Regula 34 pentru context complet.

## Setup unic (o dată per certificat, făcut DIRECT de Cristi pe Windows real)

Certificatul (privat, cu cheie) nu trece niciodată prin conversația cu
Claude — la fel ca orice altă parolă/cheie din ecosistem.

1. Dacă certificatul comun GDC NU există încă (verifică întâi cu alte
   repo-uri Windows din ecosistem), pe Windows real (Parallels e
   suficient), deschide PowerShell **ca Administrator** și rulează:
   ```powershell
   .\codesigning\generate-self-signed-cert.ps1
   ```
   Scriptul cere o parolă nouă (pentru `.pfx`) și produce două fișiere:
   - `gdc-selfsign.pfx` — **PRIVAT**, nu se distribuie, nu se
     comite în git.
   - `gdc-selfsign.cer` — **PUBLIC**, se distribuie colaboratorilor.

2. Încarcă `.pfx`-ul ca secrete GitHub Actions pe ACEST repo — comenzile
   exacte sunt afișate la finalul scriptului (necesită `gh` CLI
   autentificat pe acea mașină):
   ```powershell
   gh secret set WIN_SELFSIGN_PFX_BASE64 --repo gordasgdc/cursorpro-gdc-win --body $b64
   gh secret set WIN_SELFSIGN_PFX_PASSWORD --repo gordasgdc/cursorpro-gdc-win
   ```
   Dacă certificatul era deja generat pentru alt repo GDC, sar peste
   pasul 1 și rulează DOAR aceste două comenzi (cu valoarea `.pfx`/parola
   deja existente).

3. Șterge `.pfx`-ul local imediat după (`Remove-Item gdc-selfsign.pfx -Force`)
   — rămâne doar în secretele CI, criptate.

4. Distribuie `gdc-selfsign.cer` colaboratorilor (dacă nu l-au primit deja
   de la un alt repo GDC — e ACELAȘI certificat). Pe fiecare mașină a
   lor, o singură dată: dublu-click → **Install Certificate** →
   **Local Machine** → "Place all certificates in the following store" →
   **Trusted Root Certification Authorities**.

Odată făcuți pașii 1-4, **fiecare build viitor din CI** (push pe `main`)
semnează automat exe-ul și installer-ul cu ACELAȘI certificat —
colaboratorii nu mai trebuie să reimporte nimic la versiunile următoare.

## Ce face CI-ul automat (`.github/workflows/build-windows.yml`)

- Dacă secretele NU sunt setate: build-ul continuă **nesemnat**, exact ca
  până acum — nicio eroare, nicio schimbare de comportament.
- Dacă secretele SUNT setate: după ce `CursorPro.exe` (`dotnet publish`,
  în `publish\`) și installer-ul final (Inno Setup,
  `Output\CursorProGDCSetup.exe`) există, ambele sunt semnate cu
  `signtool.exe` (localizat dinamic din Windows Kits, cu timestamp), apoi
  verificate cu `Get-AuthenticodeSignature` — confirmă DOAR că semnătura
  a fost atașată corect, fără să ceară lanț de încredere complet (asta ar
  eșua mereu pe un runner CI proaspăt, care nu are certificatul în
  Trusted Root — normal pentru self-signed, nu un bug). Un eșec real de
  semnare (fișier fără nicio semnătură) tot oprește build-ul (CI roșu).

## Regenerarea certificatului (dacă expiră sau e compromis)

Rulează din nou `generate-self-signed-cert.ps1`, reîncarcă secretele
(pasul 2 de mai sus îi suprascrie pe cei vechi) — **pe FIECARE repo GDC
din ecosistem, nu doar aici** (certificatul e comun) — dar **toți
colaboratorii trebuie să reimporte noul `.cer`**, altfel văd din nou
avertismentul pentru versiunile semnate cu noul certificat. Evită
regenerarea inutilă — de asta scriptul folosește o valabilitate de 5 ani.
