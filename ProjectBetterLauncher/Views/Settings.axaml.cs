using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ProjectBetterLauncher.Helpers.Json;
using Avalonia.Platform.Storage;

namespace ProjectBetterLauncher.Views;

public partial class Settings : UserControl
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");
    
    private readonly string _settingsFile = Path.Combine("Data", "Settings.json");
    private AppSettings _currentSettings;
    
    public Settings()
    {
        InitializeComponent();
        
        _currentSettings = JsonStorageService.Load(_settingsFile, new AppSettings());
        DiscordRpcCheckBox.IsChecked = _currentSettings.DiscordRPC;
        BackgroundAnimationCheckBox.IsChecked = _currentSettings.BackgroundAnimation;
        
        GameLocationText.Text = string.IsNullOrEmpty(_currentSettings.GameLocation)
            ? "Game not found"
            : _currentSettings.GameLocation;
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TextChangeEffect.IsVisible = false;
        TopBar.SetState(MainWindow.StateAtTheMoment);
        
        App.PlaySound(_clickSoundPath);
        
        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.Settings.IsVisible = false;
            MainWindow.Instance.BottomLeftButtonsPanel.IsVisible = true;
        }
    }

    private void BackButton_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }

    private void DiscordRPC_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        bool isChecked = DiscordRpcCheckBox.IsChecked ?? false;

        TextChangeEffect.IsVisible = true;
        _currentSettings.DiscordRPC = isChecked;
        JsonStorageService.Save(_settingsFile, _currentSettings);
    }

    private void BackgroundAnimation_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        bool isChecked = BackgroundAnimationCheckBox.IsChecked ?? false;

        _currentSettings.BackgroundAnimation = isChecked;
        JsonStorageService.Save(_settingsFile, _currentSettings);
        
        MainWindow.Instance?.ApplyBackgroundAnimationState();
    }

    private async void ChnageBackground_OnClick(object? sender, RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null)
            return;
        
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Background Image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg" }
                }
            }
        });
        
        if (files.Count == 0) return;
        
        _currentSettings.BackgroundName = "background.jpg";
            
        var selectedFile = files[0];
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string targetPath = Path.Combine(baseDir, "Assets", "background.jpg");
        string? targetDir = Path.GetDirectoryName(targetPath);

        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }
            
        await using (Stream sourceStream = await selectedFile.OpenReadAsync())
        await using (FileStream destinationStream = File.Create(targetPath))
        {
            await sourceStream.CopyToAsync(destinationStream);
        }
        
        JsonStorageService.Save(_settingsFile, _currentSettings);
        MainWindow.Instance?.LoadBackground("background.jpg");
    }

    private async void DeleteMod_OnClick(object? sender, RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);

        _currentSettings = JsonStorageService.Load(_settingsFile, new AppSettings());

        if (!string.IsNullOrEmpty(_currentSettings.GameLocation))
        {
            string pakPath = Path.Combine(_currentSettings.GameLocation, "Playtime_Multiplayer", "Content", "Paks", "ProjectBetter_P.pak");

            try
            {
                if (File.Exists(pakPath))
                {
                    File.Delete(pakPath);
                }

                _currentSettings.InstalledVersion = "NONE";
                JsonStorageService.Save(_settingsFile, _currentSettings);

                DeleteModButton.Content = "Deleted!";
                MainWindow.Instance?.OnModDeleted();

                await Task.Delay(1500);
                DeleteModButton.Content = "Delete Project: Better";
            }
            catch (Exception ex)
            {
                DeleteModButton.Content = "Error (Close Game)";
                Console.WriteLine(ex.Message);
                await Task.Delay(2000);
                DeleteModButton.Content = "Delete Project: Better";
            }
        }
    }

    private async void DefaultSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);

        _currentSettings.DiscordRPC = true;
        _currentSettings.BackgroundName = "defaultbackground.jpg";
        _currentSettings.AcceptedInstalled = false;
        _currentSettings.BackgroundAnimation = true;
        
        JsonStorageService.Save(_settingsFile, _currentSettings);
        
        TopBar.SetState("Restarting...");
        BackToDefault.IsVisible = true;

        await Task.Delay(2000);
        
        string? exePath = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(exePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true
            });
        }
        
        MainWindow.Instance?.Close();
        Environment.Exit(0);
    }
}