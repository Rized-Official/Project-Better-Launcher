using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.IO;

namespace ProjectBetterLauncher;

public partial class App : Application
{
    public static float Volume { get; set; } = 1.0f;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void PlaySound(string filePath)
    {
        if (Volume <= 0.001f || !File.Exists(filePath))
            return;

        try
        {
            byte[] wavBytes = File.ReadAllBytes(filePath);
            float vol = Math.Clamp(Volume, 0f, 1f);
            
            if (vol < 0.99f && wavBytes.Length > 44)
            {
                for (int i = 44; i + 1 < wavBytes.Length; i += 2)
                {
                    short sample = BitConverter.ToInt16(wavBytes, i);
                    short scaled = (short)Math.Clamp(sample * vol, short.MinValue, short.MaxValue);
                    wavBytes[i] = (byte)(scaled & 0xFF);
                    wavBytes[i + 1] = (byte)((scaled >> 8) & 0xFF);
                }
            }

            using var ms = new MemoryStream(wavBytes);
            using var player = new System.Media.SoundPlayer(ms);
            player.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing sound: {ex.Message}");
        }
    }
}