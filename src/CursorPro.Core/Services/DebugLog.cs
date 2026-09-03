namespace CursorPro.Core.Services;

/// Port al DebugLog.swift (Mac) — log de diagnostic PERMANENT (Regula 25,
/// CLAUDE.md), scris pe Desktop, nu print-uri temporare. Un singur fișier,
/// citit direct de Cristi la nevoie — niciodată folosit pentru date
/// sensibile (parole, taste tastate) — vezi InputMonitor.cs (când va fi
/// portat) pentru aceeași restricție ca pe Mac.
public static class DebugLog
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
        "cursorpro_debug.log");

    private static readonly object Lock = new();

    public static void Log(string message)
    {
        try
        {
            lock (Lock)
            {
                var line = $"{DateTime.Now:HH:mm:ss.fff} {message}\n";
                File.AppendAllText(LogPath, line);
            }
        }
        catch
        {
            // Diagnostic best-effort — nu blocăm/aruncăm mai departe dacă
            // Desktop-ul nu e scriptibil dintr-un motiv oarecare.
        }
    }
}
