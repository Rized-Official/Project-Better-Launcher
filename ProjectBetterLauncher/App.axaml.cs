using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjectBetterLauncher.ViewModels;
using ProjectBetterLauncher.Views;
using DiscordRPC;
using System.Diagnostics;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using ProjectBetterLauncher.Helpers.Discord.RPC;

namespace ProjectBetterLauncher;

public partial class App : Application
{
    public string HoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    public string ClickSound = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    // On started basicly
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    public static void PlaySound(string path)
    {
        Task.Run(() =>
        {
            try
            {
                using (var audioFile = new AudioFileReader(path))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        });
    }
}