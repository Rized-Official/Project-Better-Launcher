using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
    private bool _canUnderstood = false;
    private string _videoLink = "https://www.youtube.com/watch?v=_yQcnxqHIh8";
    
    private readonly string SettingsFile = Path.Combine("Data", "Settings.json");
    private AppSettings _currentSettings;
    
    public HowToInstall()
    {
        InitializeComponent();
        CountTo10();
        
        _currentSettings = JsonStorageService.Load(SettingsFile, new AppSettings());
    }

    private void Understood_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_canUnderstood == true)
        {
            App.PlaySound(_clickSoundPath);

            _currentSettings.AcceptedInstalled = true;

            MainWindow.Instance?.HowToInstall.IsVisible = false;
        
            JsonStorageService.Save(SettingsFile, _currentSettings);
        }
        else
        {
            
        }
    }

    private void Understood_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }

    private async void CountTo10()
    {
        int countdown = 11;

        while (countdown > 0)
        {
            await Task.Delay(1000);
            countdown--;
            UnderstoodButton.Content = "Watch the video... " + countdown + "s";
        }
        _canUnderstood = true;

        UnderstoodButton.Content = "Whatched it!";
    }

    private void OpenInBrowser_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process videoBrowserProcess = new Process();
            videoBrowserProcess.StartInfo.UseShellExecute = true;
            videoBrowserProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            videoBrowserProcess.StartInfo.FileName = _videoLink;
            videoBrowserProcess.Start();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}