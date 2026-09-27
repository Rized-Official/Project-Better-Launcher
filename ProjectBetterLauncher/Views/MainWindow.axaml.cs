using Avalonia.Controls;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using NAudio.Wave;
using ProjectBetterLauncher.Helpers.Discord.RPC;
using ProjectBetterLauncher.Helpers.Json;
using Avalonia.Platform.Storage;

namespace ProjectBetterLauncher.Views;

public partial class MainWindow : Window
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");
    private string _currentState;

    public static string StateAtTheMoment;
    
    private readonly string SettingsFile = Path.Combine("Data", "Settings.json");
    private AppSettings _currentSettings;
    
    private string _showingText;
    
    public static MainWindow? Instance { get; private set; }
    
    private const string GameProcessName = "Playtime_Multiplayer-Win64-Shipping";
    
    public MainWindow()
    {
        InitializeComponent();

        Instance = this;
        
        ToastNotificationManagerCompat.OnActivated -= OnToastActivated;
        ToastNotificationManagerCompat.OnActivated += OnToastActivated;
        
        StateAtTheMoment = "In Launcher";
        
        _currentSettings = JsonStorageService.Load(SettingsFile, new AppSettings());
    }

    private void MainWindow_OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DiscordRPCManager.Initialize("1550895344668254299");
        
        if (_currentSettings.DiscordRPC == true)
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
                
                if (background.Source is IDisposable oldBitmap)
                {
                    background.Source = null;
                    oldBitmap.Dispose();
                }

                background.Source = new Bitmap(memoryStream);
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
        // Buttons
        PlayButton.IsVisible = false;
        StopButton.IsVisible = true;

        App.PlaySound(_clickSoundPath);
        
        // Steam AppId
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
        
        // Path to the logo
        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PJBTLogo.png");
        
        // Notification on launch
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
        }
    }
    
    
    // Closing the game
    public void CloseGame()
    {

        try
        {
            Process[] processes = Process.GetProcessesByName(GameProcessName);

            foreach (var process in processes)
            {
                process.Kill();
            }

            StatusText.Text = "Press \"Play\" to launch the game!";
        } catch (Exception ex)
        {
            StatusText.Text = $"There was an error while closing the game: {ex.Message}. Please contact support.";
            Debug.WriteLine(ex.Message);
        }
    }

    
    // Hover sound
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
        OpenSettingsBorder.IsVisible = false;
        
        App.PlaySound(_clickSoundPath);
        
        TopBar.SetState("In Settings");
        
        Settings.IsVisible =  true;
    }
}