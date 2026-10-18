namespace ProjectBetterLauncher.Helpers.Json;

public class AppSettings
{
    public bool DiscordRPC { get; set; } = true;
    
    public bool AcceptedInstalled { get; set; } = false;
    
    public string BackgroundName { get; set; } = "defaultbackground.jpg";

    public string GameLocation { get; set; } = "";
    
    public bool SharedOpinion { get; set; } = false;

    public string InstalledVersion { get; set; } = "NONE";

    public bool BackgroundAnimation { get; set; } = true;

    public bool HibernationMode { get; set; } = true;

    public int SoundVolume { get; set; } = 100;

    public string LastSeenChangelogVersion { get; set; } = "NONE";
}