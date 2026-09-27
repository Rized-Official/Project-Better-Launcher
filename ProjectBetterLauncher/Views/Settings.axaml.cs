using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ProjectBetterLauncher.Helpers.Json;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Controls.ApplicationLifetimes;

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
        if (_currentSettings.DiscordRPC)
        {
            DiscordRpcCheckBox.IsChecked = true;
        }
        else
        {
            DiscordRpcCheckBox.IsChecked = false;
        }
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TextChangeEffect.IsVisible = false;
        TopBar.SetState(MainWindow.StateAtTheMoment);
        
        App.PlaySound(_clickSoundPath);
        
        MainWindow.Instance?.Settings.IsVisible = false;
        MainWindow.Instance?.OpenSettingsBorder.IsVisible = true;
    }

    private void BackButton_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }

    private void DiscordRPC_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        bool isChecked = DiscordRpcCheckBox.IsChecked ?? false;

        if (isChecked)
        {
            TextChangeEffect.IsVisible = true;
            _currentSettings.DiscordRPC = true;
            
            JsonStorageService.Save(_settingsFile, _currentSettings);
        }
        else
        {
            TextChangeEffect.IsVisible = true;
            _currentSettings.DiscordRPC = false;
            
            JsonStorageService.Save(_settingsFile, _currentSettings);

        }
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

    private async void DefaultSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        _currentSettings.DiscordRPC = true;
        _currentSettings.BackgroundName = "defaultbackground.jpg";
        
        JsonStorageService.Save(_settingsFile, _currentSettings);
        
        TopBar.SetState("Closing");
        BackToDefault.IsVisible = true;

        await Task.Delay(2000);
        
        MainWindow.Instance?.Settings.IsVisible = false;
        MainWindow.Instance?.LoadBackground();
        
        MainWindow.Instance?.Close();
    }
}