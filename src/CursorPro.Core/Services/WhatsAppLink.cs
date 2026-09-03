namespace CursorPro.Core.Services;

/// Port 1:1 al WhatsAppLink.swift (Mac). Numărul de contact nu stă ca
/// literal simplu în sursă — repo-ul e public pe GitHub, iar un număr de
/// telefon scris direct ca text e ușor de găsit de crawlere automate care
/// adună numere pentru spam. Reconstruit la rulare din bucăți.
public static class WhatsAppLink
{
    private static readonly string[] Parts = ["34", "643", "109", "970"];
    private static string Number => string.Concat(Parts);

    public static Uri Url(string? text = null)
    {
        var baseUrl = $"https://wa.me/{Number}";
        return string.IsNullOrEmpty(text) ? new Uri(baseUrl) : new Uri($"{baseUrl}?text={Uri.EscapeDataString(text)}");
    }
}
