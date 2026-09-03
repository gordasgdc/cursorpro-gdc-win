using System.Management;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace CursorPro.Core.Services;

/// Port al MachineID.swift (Mac) pentru Windows — același principiu (un ID
/// hardware stabil, SHA-512, primii 6 octeți, Base32 fără liniuțe) dar
/// sursa ID-ului e diferită: pe Mac e IOPlatformUUID (IOKit), pe Windows e
/// UUID-ul din Win32_ComputerSystemProduct (expus de BIOS/placa de bază
/// prin WMI) — la fel de stabil între reporniri/reinstalări OS. NU produce
/// același hash ca pe Mac pentru aceeași mașină fizică (surse diferite) —
/// fiecare platformă își are propriul spațiu de coduri machine-locked,
/// generate separat din Furnizor (vezi GenerateSerialView.swift).
[SupportedOSPlatform("windows")]
public static class MachineID
{
    /// UUID-ul hardware raportat de Windows — stabil între reporniri.
    private static string RawPlatformUuid()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
            foreach (var obj in searcher.Get())
            {
                var uuid = obj["UUID"]?.ToString();
                if (!string.IsNullOrWhiteSpace(uuid) && uuid != "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                {
                    return uuid;
                }
            }
        }
        catch
        {
            // WMI indisponibil (rulare fără privilegii, VM restricționată, etc.)
        }
        return "win-machine-id-unavailable";
    }

    /// Hash-ul de 6 octeți folosit atât pentru afișare cât și pentru
    /// machine-locking-ul codurilor de licență.
    public static byte[] HashBytes =>
        SHA512.HashData(Encoding.UTF8.GetBytes(RawPlatformUuid()))[..6];

    /// String Base32 scurt, lizibil (fără liniuțe) — ce copiază userul din
    /// Preferințe -> Licență și trimite prin WhatsApp înainte de activare.
    public static string Display => LicenseCore.Base32Encode(HashBytes);
}
