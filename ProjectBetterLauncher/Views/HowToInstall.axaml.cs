using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ProjectBetterLauncher.Helpers.Json;

namespace ProjectBetterLauncher.Views;

public partial class HowToInstall : UserControl
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");
    
    private readonly string SettingsFile = Path.Combine("Data", "Settings.json");
    private AppSettings _currentSettings;
    
    public HowToInstall()
    {
        InitializeComponent();
        
        _currentSettings = JsonStorageService.Load(SettingsFile, new AppSettings());
    }

    private void Understood_OnClick(object? sender, RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);

        _currentSettings.AcceptedInstalled = true;

        MainWindow.Instance.HowToInstall.IsVisible = false;
        
        JsonStorageService.Save(SettingsFile, _currentSettings);
    }

    private void Understood_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }

    private void HowToInstall_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_currentSettings.AcceptedInstalled == false)
        {

        }
        else
        {
            MainWindow.Instance.HowToInstall.IsVisible = false;

        }

    }
}