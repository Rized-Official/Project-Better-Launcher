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
        
        _currentSettings = JsonStorageService.Load(SettingsFile, new AppSettings());

        if (_currentSettings.AcceptedInstalled)
        {
            _canUnderstood = true;
            UnderstoodButton.Content = "Watched it!";
            StopVideo();
        }
        else
        {
            CountTo10();
        }
    }

    public void OpenFromSettings()
    {
        _canUnderstood = true;
        UnderstoodButton.Content = "Watched it!";

        try
        {
            if (NativeWebView != null)
            {
                NativeWebView.IsVisible = true;
                NativeWebView.Source = new Uri(_videoLink);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void StopVideo()
    {
        try
        {
            if (NativeWebView != null)
            {
                NativeWebView.Source = new Uri("about:blank");
                NativeWebView.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void Understood_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_canUnderstood)
        {
            App.PlaySound(_clickSoundPath);

            StopVideo();

            _currentSettings = JsonStorageService.Load(SettingsFile, new AppSettings());
            _currentSettings.AcceptedInstalled = true;
            JsonStorageService.Save(SettingsFile, _currentSettings);

            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.HowToInstall.IsVisible = false;
                MainWindow.Instance.BottomLeftButtonsPanel.IsVisible = true;
                TopBar.SetState(MainWindow.StateAtTheMoment);
            }
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
            if (!_canUnderstood)
            {
                UnderstoodButton.Content = "Watch the video... " + countdown + "s";
            }
        }
        
        _canUnderstood = true;
        UnderstoodButton.Content = "Watched it!";
    }

    private void OpenInBrowser_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            StopVideo();

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