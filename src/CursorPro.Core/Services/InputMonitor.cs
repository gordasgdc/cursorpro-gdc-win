using System.Drawing;
using System.Runtime.InteropServices;
using CursorPro.Core.State;

namespace CursorPro.Core.Services;

/// Urmărește poziția cursorului și starea tastelor modificator, la nivel
/// de sistem (peste toate aplicațiile) — echivalentul InputMonitor.swift
/// (Mac), dar cu o arhitectură deliberat diferită:
///
/// Mac foloseşte `NSEvent.addGlobalMonitorForEvents` (abonare la
/// evenimente). Windows nu are un echivalent managed direct pentru asta —
/// alternativa nativă ar fi `SetWindowsHookEx(WH_MOUSE_LL/WH_KEYBOARD_LL)`
/// (vezi CLAUDE.md, planul inițial), dar necesită păstrarea unui delegate
/// de hook în viață + o buclă de mesaje pe thread-ul care instalează
/// hook-ul, cu multe capcane greu de verificat fără Windows real.
///
/// În schimb, `GetCursorPos`/`GetAsyncKeyState` sunt interogări DIRECTE
/// de stare globală (nu evenimente) — nu necesită niciun hook, funcţionează
/// din orice thread, şi ajung la exact acelaşi rezultat pentru cazul de
/// folosire de-aici (poziţie + „e ţinută apăsată acum tasta X?"). Această
/// alegere e adaptare deliberată, documentată — vezi Regula 31 (CLAUDE.md)
/// pentru portările reale/testate pe Windows.
///
/// `Tick()` e chemat o singură dată pe cadru, de acelaşi timer UI care
/// redesenează overlay-ul (`OverlayManager`), la fel cum pe Mac atât
/// `InputMonitor`, cât şi bucla de redraw a `OverlayView`, rulează pe
/// thread-ul principal.
public static class InputMonitor
{
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12;   // Alt
    private const int VK_SHIFT = 0x10;
    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;

    private static bool IsHeld(ModifierKey key) => key switch
    {
        ModifierKey.Control => (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0,
        ModifierKey.Alt => (GetAsyncKeyState(VK_MENU) & 0x8000) != 0,
        ModifierKey.Shift => (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0,
        ModifierKey.Windows => (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 || (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0,
        _ => false,
    };

    public static void Tick()
    {
        var state = AppState.Shared;

        if (GetCursorPos(out var p))
        {
            state.MouseLocation = new PointF(p.X, p.Y);
        }

        // Probă expirată, fără licenţă activă: modurile reale rămân
        // oprite indiferent ce tastă e ţinută — la fel ca pe Mac
        // (InputMonitor.swift, cazul .flagsChanged). Fereastra de
        // Preferinţe → Licenţă funcţionează oricând, neschimbat.
        var unlocked = LicenseManager.Shared.IsUnlocked;
        var wasActive = state.IsSpotlightActive;
        var keyHeld = IsHeld(state.SpotlightKey);
        state.IsSpotlightActive = unlocked && keyHeld;

        // Log DOAR la tranziții (nu la fiecare cadru, ~60/sec — ar inunda
        // fișierul) — ajută să diagnosticăm dacă tasta chiar e detectată
        // ca ținută (GetAsyncKeyState) sau dacă licența blochează modul.
        if (state.IsSpotlightActive != wasActive)
        {
            DebugLog.Log($"Spotlight {(state.IsSpotlightActive ? "ACTIVAT" : "dezactivat")} — tastă={state.SpotlightKey}, ținută={keyHeld}, deblocat={unlocked}");
        }

        var zoomWasActive = state.IsZoomActive;
        var zoomKeyHeld = IsHeld(state.ZoomKey);
        state.IsZoomActive = unlocked && zoomKeyHeld;
        if (state.IsZoomActive != zoomWasActive)
        {
            DebugLog.Log($"Zoom {(state.IsZoomActive ? "ACTIVAT" : "dezactivat")} — tastă={state.ZoomKey}, ținută={zoomKeyHeld}, deblocat={unlocked}");
        }
    }
}
