using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ProjectBetterLauncher.Helpers.Discord.RPC;
using ProjectBetterLauncher.Helpers.Json;

namespace ProjectBetterLauncher.Views;

public partial class Opinion : UserControl
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");
    
    private readonly string SettingsFile = Path.Combine("Data", "Settings.json");
    private AppSettings _currentSettings;
    
    public Opinion()
    {
        InitializeComponent();
        
        _currentSettings = JsonStorageService.Load(SettingsFile, new AppSettings());
    }

    private void ShareWithDevelopersButton_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }

    private void ShareWithDevelopersButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.Opinion.IsVisible = false;

        string HowLongPJBTUsed = HowLongPJBTUse.Text;

        _currentSettings.SharedOpinion = true;
        
        JsonStorageService.Save(SettingsFile, _currentSettings);
        
        OpinionManager.SendOpinion("New opinion send: " + HowLongPJBTUsed);
    }
}