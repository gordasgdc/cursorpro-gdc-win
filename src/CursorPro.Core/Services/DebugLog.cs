namespace CursorPro.Core.Services;

/// Port al DebugLog.swift (Mac) — log de diagnostic PERMANENT (Regula 25,
/// CLAUDE.md). Niciodată folosit pentru date sensibile (parole, taste
/// tastate) — vezi InputMonitor.cs pentru aceeași restricție ca pe Mac.
///
/// BUG REAL (raportat de Cristi, 2026-09-04): calea inițială,
/// `%USERPROFILE%\Desktop\cursorpro_debug.log`, nu producea NICIUN
/// fișier vizibil pe sistemul lui de test — nici măcar linia de pornire
/// a aplicației (necondiționată, la fiecare lansare). Cauză neconfirmată
/// (Desktop redirecționat prin OneDrive e cea mai probabilă explicație
/// pe Windows, dar rămâne o ipoteză) — nu am reprodus-o local, doar am
/// făcut scrierea mai robustă: acum scrie în DOUĂ locații independente,
/// astfel încât cel puțin una să fie găsibilă indiferent de cauza reală.
public static class DebugLog
{
    private static readonly string[] LogPaths =
    {
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "cursorpro_debug.log"),
        // A doua locație — NU depinde de Desktop (redirecționare OneDrive
        // etc.): același folder unde LicenseManager ține deja
        // trial-start.txt/license.txt, deci garantat scriptibil.
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CursorPro", "cursorpro_debug.log"),
    };

    private static readonly object Lock = new();

    public static void Log(string message)
    {
        var line = $"{DateTime.Now:HH:mm:ss.fff} {message}\n";
        lock (Lock)
        {
            foreach (var path in LogPaths)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    File.AppendAllText(path, line);
                }
                catch
                {
                    // Diagnostic best-effort — o locație care eșuează nu
                    // trebuie să blocheze scrierea în cealaltă.
                }
            }
        }
    }
}
