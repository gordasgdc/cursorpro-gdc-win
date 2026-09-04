using System.Runtime.InteropServices;

namespace CursorPro.Core.Services;

/// Structuri + funcții native pentru Windows Magnification API
/// (`Magnification.dll`) — echivalentul funcțional al ScreenCaptureKit
/// (Mac): livrează conținut mărit LIVE, compus direct de sistem, nu
/// capturi statice pe un timer. E API-ul folosit chiar de Lupa (Magnifier)
/// nativă din Windows (accesibilitate), deci e mecanismul "corect" pentru
/// exact acest caz de folosire, nu doar o alegere convenabilă.
[StructLayout(LayoutKind.Sequential)]
public struct MagRect
{
    public int Left, Top, Right, Bottom;
}

/// Matrice de transformare 3x3 (row-major) cerută de
/// `MagSetWindowTransform` — pentru un simplu factor de scalare uniform,
/// doar M11/M22 (X/Y) și M33 (identitate) contează.
[StructLayout(LayoutKind.Sequential)]
public struct MagTransform
{
    public float M11, M12, M13;
    public float M21, M22, M23;
    public float M31, M32, M33;

    public static MagTransform Scale(float factor) => new()
    {
        M11 = factor,
        M22 = factor,
        M33 = 1f,
    };
}

public static class MagnificationInterop
{
    /// Numele clasei de fereastră nativă pe care Magnification API o
    /// înregistrează după `MagInitialize()` — se creează o fereastră
    /// copil din această clasă (vezi MagnifierHost.cs, Client).
    public const string WindowClassName = "Magnifier";

    [DllImport("Magnification.dll", SetLastError = true)]
    public static extern bool MagInitialize();

    [DllImport("Magnification.dll", SetLastError = true)]
    public static extern bool MagUninitialize();

    [DllImport("Magnification.dll", SetLastError = true)]
    public static extern bool MagSetWindowSource(IntPtr hwnd, MagRect rect);

    [DllImport("Magnification.dll", SetLastError = true)]
    public static extern bool MagSetWindowTransform(IntPtr hwnd, ref MagTransform transform);
}
