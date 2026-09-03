namespace CursorPro.Core.Services;

/// Oglindă LicenseManager.swift (Mac): probă de 3 zile la prima lansare,
/// apoi activare printr-un cod generat manual din Furnizor
/// (GenerateSerialView.swift, `cursorpro` în gdcStandaloneProducts) —
/// același flux WhatsApp ca toate celelalte unelte GDC, NU plată
/// automatizată. Fără verificare de revocare online (CursorPro Mac nu are
/// una — dacă se adaugă vreodată acolo, portează-o și aici, în aceeași
/// sesiune, per Regula 31).
///
/// Punctul UNIC de gating: <see cref="IsUnlocked"/> — verificat înainte de
/// a porni orice mod real (Halo/Spotlight/Draw/Zoom), la fel ca pe Mac
/// (InputMonitor.swift).
public sealed class LicenseManager
{
    public static readonly LicenseManager Shared = new();
    public const string ProductId = "cursorpro";
    public const int TrialDurationDays = 3;

    public bool IsLicensed { get; private set; }
    public long LicenseExpiresAt { get; private set; } // 0 = perpetuu
    public bool LicenseMachineLocked { get; private set; }
    public string? ActivationError { get; private set; }

    /// Ridicat după orice schimbare de stare (activare/dezactivare) — UI-ul
    /// (meniul din tray, fereastra de Licență) se poate reconstrui la
    /// nevoie; nu e nevoie de INotifyPropertyChanged complet aici, meniul
    /// tray se reconstruiește oricum la fiecare deschidere (ca pe Mac).
    public event Action? Changed;

    private static string AppDataFolder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CursorPro");

    private static string TrialStartFilePath => Path.Combine(AppDataFolder, "trial-start.txt");
    private static string ActivationFilePath => Path.Combine(AppDataFolder, "license.txt");

    private DateTimeOffset _trialStart;

    private LicenseManager()
    {
        EnsureTrialStarted();
        LoadSavedLicense();
    }

    private void EnsureTrialStarted()
    {
        var path = TrialStartFilePath;
        if (File.Exists(path) && long.TryParse(File.ReadAllText(path).Trim(), out var unixSeconds))
        {
            _trialStart = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            return;
        }

        _trialStart = DateTimeOffset.Now;
        Directory.CreateDirectory(AppDataFolder);
        File.WriteAllText(path, _trialStart.ToUnixTimeSeconds().ToString());
    }

    /// Zile întregi rămase din probă, rotunjit în sus — "3" toată prima zi,
    /// până la "0" o dată ce chiar a expirat.
    public int TrialDaysRemaining
    {
        get
        {
            var elapsed = DateTimeOffset.Now - _trialStart;
            var remaining = TimeSpan.FromDays(TrialDurationDays) - elapsed;
            return Math.Max(0, (int)Math.Ceiling(remaining.TotalDays));
        }
    }

    public bool IsTrialActive => TrialDaysRemaining > 0;

    /// Singurul punct de adevăr verificat înainte de a porni orice mod real.
    public bool IsUnlocked => IsLicensed || IsTrialActive;

    public bool Activate(string code)
    {
        ActivationError = null;
        var trimmed = code.Trim();
        try
        {
            var payload = LicenseCore.Validate(trimmed, ProductId);
            SaveLicense(trimmed);
            ApplyLicense(payload.ExpiresAt, payload.MachineLocked);
            Changed?.Invoke();
            return true;
        }
        catch (LicenseCore.ValidationError error)
        {
            ActivationError = MessageFor(error.Kind);
            Changed?.Invoke();
            return false;
        }
    }

    public void Deactivate()
    {
        IsLicensed = false;
        LicenseExpiresAt = 0;
        LicenseMachineLocked = false;
        var path = ActivationFilePath;
        if (File.Exists(path)) File.Delete(path);
        Changed?.Invoke();
    }

    private void LoadSavedLicense()
    {
        var path = ActivationFilePath;
        if (!File.Exists(path)) return;
        var code = File.ReadAllText(path).Trim();
        try
        {
            var payload = LicenseCore.Validate(code, ProductId);
            ApplyLicense(payload.ExpiresAt, payload.MachineLocked);
        }
        catch (LicenseCore.ValidationError)
        {
            // Cod salvat invalid/expirat — rămânem nelicențiați, fără să aruncăm mai departe.
        }
    }

    private void ApplyLicense(long expiresAt, bool machineLocked)
    {
        IsLicensed = true;
        LicenseExpiresAt = expiresAt;
        LicenseMachineLocked = machineLocked;
    }

    private static void SaveLicense(string code)
    {
        Directory.CreateDirectory(AppDataFolder);
        File.WriteAllText(ActivationFilePath, code);
    }

    private static string MessageFor(LicenseCore.ValidationErrorKind kind) => kind switch
    {
        LicenseCore.ValidationErrorKind.MalformedCode => "Cod invalid — verifică să nu lipsească vreun caracter.",
        LicenseCore.ValidationErrorKind.BadSignature => "Semnătura codului nu se potrivește.",
        LicenseCore.ValidationErrorKind.WrongProduct => "Codul e valid, dar pentru alt produs GDC.",
        LicenseCore.ValidationErrorKind.WrongMachine => "Codul e blocat pe alt calculator.",
        LicenseCore.ValidationErrorKind.Expired => "Codul a expirat.",
        _ => "Cod invalid.",
    };
}
