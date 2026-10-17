using Avalonia.Controls;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ProjectBetterLauncher.Helpers.Discord.RPC;
using ProjectBetterLauncher.Helpers.Json;
using ProjectBetterLauncher.Helpers.Steam;

namespace ProjectBetterLauncher.Views;

public class SupabaseModInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; } = "";

    [JsonPropertyName("pak_name")]
    public string? PakName { get; set; }

    [JsonPropertyName("changelog_title")]
    public string? ChangelogTitle { get; set; }

    [JsonPropertyName("changelog_data")]
    public List<string>? ChangelogData { get; set; }
}

public partial class MainWindow : Window
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");
    private DispatcherTimer? _gameWatcher;
    public string gameLocation;

    private const string SupabaseUrl = "https://bpxpcrdmgeriiyfuizdw.supabase.co";
    private const string SupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJweHBjcmRtZ2VyaWl5ZnVpemR3Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTE5NzM0NzcsImV4cCI6MjEwNzU0OTQ3N30.1CXgSM-v5MlWaW6XguewJtAz2vcTWNFiYMYedAIw9-Y";
    private const string SupabaseTableName = "mod_info";
    private const string DefaultPakFileName = "ProjectBetter_P.pak";

    private static readonly HttpClient _httpClient = new HttpClient();
    private string _latestVersion = "";
    private string _downloadUrl = "";
    private string _pakFileName = DefaultPakFileName;
    private bool _needsDownload = false;
    private bool _isDownloading = false;
    private bool _isHibernating = false;

    public static bool IsProcessRunning(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        bool running = processes.Length > 0;

        foreach (Process process in processes)
            process.Dispose();

        return running;
    }

    public static string StateAtTheMoment = "In Launcher";
    
    private readonly string SettingsFile = Path.Combine("Data", "Settings.json");
    private AppSettings _currentSettings;
    
    public static MainWindow? Instance { get; private set; }
    
    private const string GameProcessName = "Playtime_Multiplayer-Win64-Shipping";
    
    public MainWindow()
    {
        _currentSettings = JsonStorageService.Load(SettingsFile, new AppSettings());
        
        if (string.IsNullOrEmpty(_currentSettings.GameLocation) || !Directory.Exists(_currentSettings.GameLocation))
        {
            string? foundLocation = SteamHelper.FindGame();

            if (foundLocation != null)
            {
                _currentSettings.GameLocation = foundLocation;
                JsonStorageService.Save(SettingsFile, _currentSettings);
            }
        }

        gameLocation = _currentSettings.GameLocation;
        
        InitializeComponent();
        
        Instance = this;
        
        ToastNotificationManagerCompat.OnActivated -= OnToastActivated;
        ToastNotificationManagerCompat.OnActivated += OnToastActivated;
        
        StateAtTheMoment = "In Launcher";
    }

    private async void MainWindow_OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HowToInstall.IsVisible = !_currentSettings.AcceptedInstalled;
        
        DiscordRPCManager.Initialize("1550895344668254299");
        
        if (_currentSettings.DiscordRPC)
        {
            DiscordRPCManager.SetDiscordRPC(
                details: "In Launcher",
                state: "Ready to play",
                largeImageKey: "image_large",
                largeImageText: "Project: Better Launcher",
                smallImageKey: "image_small");
        }
        else
        {
            Debug.WriteLine("Discord RPC is off.");
        }
        
        TopBar.SetState(StateAtTheMoment);
        
        LoadBackground();
        ApplyBackgroundAnimationState();

        await CheckForUpdatesAsync();
    }

    public void EnterHibernation()
    {
        if (_isHibernating) return;
        _isHibernating = true;

        BackgroundImage.Classes.Remove("bg");

        if (BackgroundImage.Source is IDisposable oldBitmap)
        {
            BackgroundImage.Source = null;
            oldBitmap.Dispose();
        }

        Settings.IsVisible = false;
        Changelog.IsVisible = false;
        MainContentPanel.IsVisible = false;
        Hibernation.IsVisible = true;

        TopBar.SetState("Hibernating");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    public void ExitHibernation()
    {
        if (!_isHibernating) return;
        _isHibernating = false;

        Hibernation.IsVisible = false;
        MainContentPanel.IsVisible = true;
        BottomLeftButtonsPanel.IsVisible = true;

        LoadBackground();
        ApplyBackgroundAnimationState();

        TopBar.SetState(StateAtTheMoment);
    }

    public void ApplyBackgroundAnimationState()
    {
        _currentSettings = JsonStorageService.Load(SettingsFile, _currentSettings);
        if (_currentSettings.BackgroundAnimation && !_isHibernating)
        {
            if (!BackgroundImage.Classes.Contains("bg"))
                BackgroundImage.Classes.Add("bg");
        }
        else
        {
            BackgroundImage.Classes.Remove("bg");
        }
    }

    public void OnModDeleted()
    {
        _currentSettings = JsonStorageService.Load(SettingsFile, _currentSettings);
        _needsDownload = true;
        PlayButtonAction.Content = "Download";
        StatusText.Text = "Mod is not installed. Press \"Download\" to install!";
    }

    private string GetPaksFolderPath()
    {
        if (string.IsNullOrEmpty(gameLocation))
            return string.Empty;

        return Path.Combine(gameLocation, "Playtime_Multiplayer", "Content", "Paks");
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            PlayButtonAction.IsEnabled = false;
            PlayButtonAction.Content = "Checking...";
            StatusText.Text = "Checking for updates...";
            VersionText.Text = "Newest version: LOADING";

            string requestUrl = $"{SupabaseUrl}/rest/v1/{SupabaseTableName}?select=*&limit=1";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("apikey", SupabaseAnonKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseAnonKey}");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var modInfoList = JsonSerializer.Deserialize<List<SupabaseModInfo>>(json);

            if (modInfoList != null && modInfoList.Count > 0)
            {
                var modInfo = modInfoList[0];
                _latestVersion = modInfo.Version;
                _downloadUrl = modInfo.DownloadUrl.Replace("dl=0", "dl=1");
                
                if (!string.IsNullOrWhiteSpace(modInfo.PakName))
                {
                    _pakFileName = modInfo.PakName;
                }

                VersionText.Text = $"Newest version: {_latestVersion}";

                string paksFolder = GetPaksFolderPath();
                string pakFilePath = Path.Combine(paksFolder, _pakFileName);

                bool pakExists = !string.IsNullOrEmpty(paksFolder) && File.Exists(pakFilePath);
                bool isUpToDate = _currentSettings.InstalledVersion == _latestVersion;

                bool isAlreadySetup = _currentSettings.AcceptedInstalled;
                bool isNewChangelogVersion = _currentSettings.LastSeenChangelogVersion != _latestVersion;

                if (!isAlreadySetup)
                {
                    _currentSettings.LastSeenChangelogVersion = _latestVersion;
                    JsonStorageService.Save(SettingsFile, _currentSettings);
                    Changelog.SetChangelog(modInfo.ChangelogTitle, modInfo.ChangelogData, _latestVersion, false);
                }
                else if (isNewChangelogVersion || !isUpToDate)
                {
                    _currentSettings.LastSeenChangelogVersion = _latestVersion;
                    JsonStorageService.Save(SettingsFile, _currentSettings);

                    Changelog.SetChangelog(modInfo.ChangelogTitle, modInfo.ChangelogData, _latestVersion, true);
                    BottomLeftButtonsPanel.IsVisible = false;
                    Changelog.IsVisible = true;
                    TopBar.SetState("New Update!");
                }
                else
                {
                    Changelog.SetChangelog(modInfo.ChangelogTitle, modInfo.ChangelogData, _latestVersion, false);
                }

                if (!pakExists || !isUpToDate)
                {
                    _needsDownload = true;
                    PlayButtonAction.Content = "Download";
                    StatusText.Text = !pakExists
                        ? "Mod is not installed. Press \"Download\" to install!"
                        : $"Update available ({_latestVersion})! Press \"Download\" to update.";
                }
                else
                {
                    _needsDownload = false;
                    PlayButtonAction.Content = "Play";
                    StatusText.Text = "Press \"Play\" to launch the game!";
                }
            }
            else
            {
                VersionText.Text = "Newest version: UNKNOWN";
                StatusText.Text = "Could not find version info on server.";
                PlayButtonAction.Content = "Play";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Supabase Error: {ex.Message}");
            VersionText.Text = "Newest version: OFFLINE";
            StatusText.Text = "Failed to check updates. You can still launch the game.";
            PlayButtonAction.Content = "Play";
            _needsDownload = false;
        }
        finally
        {
            PlayButtonAction.IsEnabled = true;
        }
    }

    private async Task DownloadModAsync()
    {
        if (_isDownloading) return;

        if (string.IsNullOrEmpty(gameLocation) || !Directory.Exists(gameLocation))
        {
            StatusText.Text = "Game location not found! Please install Project: Playtime on Steam.";
            return;
        }

        if (string.IsNullOrEmpty(_downloadUrl))
        {
            StatusText.Text = "Download URL is missing from server!";
            return;
        }

        bool downloadSuccess = false;

        try
        {
            _isDownloading = true;
            PlayButtonAction.IsEnabled = false;
            PlayButtonAction.Content = "Downloading...";
            
            DownloadProgressBorder.IsVisible = true;
            DownloadProgressBar.Value = 0;
            DownloadProgressText.Text = "0%";
            StatusText.Text = "Downloading mod files...";
            TopBar.SetState("Downloading...");

            string paksFolder = GetPaksFolderPath();
            if (!Directory.Exists(paksFolder))
            {
                Directory.CreateDirectory(paksFolder);
            }

            string destinationPakPath = Path.Combine(paksFolder, _pakFileName);

            using var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(destinationPakPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalBytes.HasValue && totalBytes.Value > 0)
                {
                    double progressPercentage = (double)totalRead / totalBytes.Value * 100;
                    double downloadedMb = totalRead / 1024d / 1024d;
                    double totalMb = totalBytes.Value / 1024d / 1024d;

                    DownloadProgressBar.Value = progressPercentage;
                    DownloadProgressText.Text = $"{progressPercentage:0}% ({downloadedMb:0.0} / {totalMb:0.0} MB)";
                }
                else
                {
                    double downloadedMb = totalRead / 1024d / 1024d;
                    DownloadProgressText.Text = $"{downloadedMb:0.1} MB";
                }
            }

            _currentSettings = JsonStorageService.Load(SettingsFile, _currentSettings);
            _currentSettings.InstalledVersion = _latestVersion;
            _currentSettings.LastSeenChangelogVersion = _latestVersion;
            JsonStorageService.Save(SettingsFile, _currentSettings);

            _needsDownload = false;
            PlayButtonAction.Content = "Play";
            StatusText.Text = "Download complete! Launching the game...";
            downloadSuccess = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Download error: {ex.Message}");
            StatusText.Text = $"Download failed: {ex.Message}";
            PlayButtonAction.Content = "Download";
            TopBar.SetState("In Launcher");
        }
        finally
        {
            _isDownloading = false;
            PlayButtonAction.IsEnabled = true;
            DownloadProgressBorder.IsVisible = false;
        }

        if (downloadSuccess)
        {
            OpenGame();
        }
    }
    
    public void LoadBackground(string? newBackgroundName = null)
    {
        if (!string.IsNullOrEmpty(newBackgroundName))
        {
            _currentSettings.BackgroundName = newBackgroundName;
        }
        
        if (string.IsNullOrWhiteSpace(_currentSettings.BackgroundName))
            return;

        string imagePath = Path.Combine(AppContext.BaseDirectory, "Assets", _currentSettings.BackgroundName);

        if (File.Exists(imagePath))
        {
            try
            {
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                using var memoryStream = new MemoryStream(imageBytes);
                
                if (BackgroundImage.Source is IDisposable oldBitmap)
                {
                    BackgroundImage.Source = null;
                    oldBitmap.Dispose();
                }

                BackgroundImage.Source = new Bitmap(memoryStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        else
        {
            Debug.WriteLine("Not existing");
        }
    }

    private void OnToastActivated(ToastNotificationActivatedEventArgsCompat toastArgs)
    {
        ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

        if (args.TryGetValue("action", out string? action) && action == "cancel")
        {
            CloseGame();
        }
    }

    private async void PlayButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);

        if (_needsDownload)
        {
            await DownloadModAsync();
        }
        else
        {
            OpenGame();
        }
    }

    private async void OpenGame()
    {
        PlayButton.IsVisible = false;
        StopButton.IsVisible = true;
        
        StatusText.Text = "Launching...";
        int appId = 1961460;
        string steamUrl = $"steam://rungameid/{appId}";

        Process.Start(new ProcessStartInfo
        {
            FileName = steamUrl,
            UseShellExecute = true
        });

        StateAtTheMoment = "In Game";
        
        TopBar.SetState(StateAtTheMoment);
        StartWatchingGame();
        
        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PJBTLogo.png");
        
        new ToastContentBuilder()
            .AddArgument("action", "viewUpdates")
            .AddHeroImage(new Uri(logoPath))
            .AddText("Project: Better is launching")
            .AddText("Please wait")
            .AddButton(new ToastButton()
                .SetContent("Okay")
            )
            .AddButton(new ToastButton()
                .SetContent("Close The Game")
                .AddArgument("action", "cancel")
                .SetBackgroundActivation()
            )
            .Show();

        this.WindowState = WindowState.Minimized;
        
        await Task.Delay(5000);
        
        Process[] processes = Process.GetProcessesByName(GameProcessName);

        if (processes.Length == 0)
        {
            StatusText.Text = "The game didn't launch properly. Please contact support.";
        }
        else
        {
            StatusText.Text = "Launched!";
            EnterHibernation();
        }
    }
    
    public void CloseGame()
    {
        try
        {
            ExitHibernation();

            Process[] processes = Process.GetProcessesByName(GameProcessName);

            foreach (var process in processes)
            {
                process.Kill();
            }

            StatusText.Text = "Press \"Play\" to launch the game!";
        } 
        catch (Exception ex)
        {
            StatusText.Text = $"There was an error while closing the game: {ex.Message}. Please contact support.";
            Debug.WriteLine(ex.Message);
        }
    }

    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }

    private void CloseTheGame_OnClick(object? sender, RoutedEventArgs e)
    {
        StateAtTheMoment = "In Launcher";
        
        TopBar.SetState(StateAtTheMoment);
        
        App.PlaySound(_clickSoundPath);
        StopButton.IsVisible = false;
        PlayButton.IsVisible = true;
        CloseGame();
    }

    private void OpenSettings_Click(object? sender, RoutedEventArgs e)
    {
        BottomLeftButtonsPanel.IsVisible = false;
        
        App.PlaySound(_clickSoundPath);
        
        TopBar.SetState("In Settings");
        
        Settings.IsVisible = true;
    }

    private void OpenChangelog_Click(object? sender, RoutedEventArgs e)
    {
        BottomLeftButtonsPanel.IsVisible = false;
        
        App.PlaySound(_clickSoundPath);
        
        TopBar.SetState("In Changelog");
        
        Changelog.IsVisible = true;
    }
    
    private void StartWatchingGame()
    {
        _gameWatcher = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };

        _gameWatcher.Tick += (s, e) =>
        {
            if (!IsProcessRunning(GameProcessName))
            {
                _gameWatcher.Stop();

                StateAtTheMoment = "In Launcher";
                ExitHibernation();
                TopBar.SetState(StateAtTheMoment);
                StopButton.IsVisible = false;
                PlayButton.IsVisible = true;
                StatusText.Text = "Press \"Play\" to launch the game!";
                WindowState = WindowState.Normal;
            }
        };

        _gameWatcher.Start();
    }
}