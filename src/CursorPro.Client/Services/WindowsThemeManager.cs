using System;
using System.Windows;
using Microsoft.Win32;
using WpfApplication = System.Windows.Application;

namespace CursorPro.Client.Services;

/// Urmează tema Windows (Dark/Light) și schimbă paleta aplicației.
///
/// DE CE E NEVOIE DE COD, spre deosebire de Mac: WPF clasic NU urmărește Dark
/// Mode-ul din Windows. O fereastră fără fundal setat rămâne albă cu text
/// negru chiar dacă tot sistemul e pe temă închisă — exact simptomul găsit la
/// fereastra de Preferințe. Paleta e a noastră (Styles/Theme.Dark.xaml și
/// Theme.Light.xaml), iar aici se alege care dintre ele e activă.
///
/// Indexul e fixat intenționat în App.xaml: 0 = paleta de culori (cea
/// schimbată aici), 1 = stilurile care o citesc. Dacă se adaugă un dicționar
/// nou, ordinea aceea trebuie păstrată.
public static class WindowsThemeManager
{
    private const int ColorsDictIndex = 0;
    private const string DarkPath = "Styles/Theme.Dark.xaml";
    private const string LightPath = "Styles/Theme.Light.xaml";

    /// Cheia din registru pe care o scrie chiar Windows când comuți tema.
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public static bool SystemUsesLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            // `AppsUseLightTheme` (nu `SystemUsesLightTheme`): prima e tema
            // APLICAȚIILOR, a doua e a barei de activități. Pot fi setate
            // diferit, iar pe noi ne interesează prima.
            return key?.GetValue("AppsUseLightTheme") is int v && v != 0;
        }
        catch
        {
            // Cheia poate lipsi pe ediții vechi de Windows. Tema închisă e
            // implicitul aplicației, deci nu e un caz de eroare.
            return false;
        }
    }

    public static void ApplyNow() => Apply(SystemUsesLightTheme());

    public static void Apply(bool light)
    {
        var merged = WpfApplication.Current?.Resources.MergedDictionaries;
        if (merged is null) return;

        var dict = new ResourceDictionary
        {
            Source = new Uri(light ? LightPath : DarkPath, UriKind.Relative)
        };

        if (merged.Count > ColorsDictIndex) merged[ColorsDictIndex] = dict;
        else merged.Insert(ColorsDictIndex, dict);
    }

    /// Ascultă schimbarea temei din Windows cât timp aplicația rulează.
    /// CursorPro stă în tray ore întregi — o comutare de temă la apus nu are
    /// voie să-l lase pe paleta veche până la repornire.
    public static void StartFollowingSystem()
    {
        SystemEvents.UserPreferenceChanged += (_, e) =>
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                WpfApplication.Current?.Dispatcher.Invoke(ApplyNow);
            }
        };
    }
}
