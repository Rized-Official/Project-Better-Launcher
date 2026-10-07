using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ProjectBetterLauncher.Helpers.Steam;

[SupportedOSPlatform("windows")]
public static class SteamHelper
{
    public static string? FindGame(string folderName = "Poppy Playtime - Multiplayer")
    {
        string? steamPath = GetSteamPath();
        if (steamPath == null) return null;

        List<string> libraries = new List<string> { steamPath };

        string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (File.Exists(vdfPath))
        {
            string text = File.ReadAllText(vdfPath);
            foreach (Match match in Regex.Matches(text, "\"path\"\\s+\"(.+?)\""))
            {
                libraries.Add(match.Groups[1].Value.Replace(@"\\", @"\"));
            }
        }

        foreach (string library in libraries)
        {
            string gamePath = Path.Combine(library, "steamapps", "common", folderName);
            if (Directory.Exists(gamePath))
                return gamePath;
        }

        return null;
    }

    private static string? GetSteamPath()
    {
        string? path = Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam",
            "InstallPath", null) as string;

        path ??= Registry.GetValue(
            @"HKEY_CURRENT_USER\Software\Valve\Steam",
            "SteamPath", null) as string;

        return path?.Replace('/', '\\');
    }
}