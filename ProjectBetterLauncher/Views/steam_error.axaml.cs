using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ProjectBetterLauncher.Views;

public partial class steam_error : UserControl
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");

    public steam_error()
    {
        InitializeComponent();
    }

    private async void TurnOnSteam_OnClick(object? sender, RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);

        try
        {
            TurnOnSteamButton.IsEnabled = false;
            TurnOnSteamButton.Content = "Starting Steam...";

            Process.Start(new ProcessStartInfo
            {
                FileName = "steam://open/main",
                UseShellExecute = true
            });

            for (int i = 0; i < 12; i++)
            {
                await Task.Delay(500);
                if (MainWindow.IsProcessRunning("steam"))
                {
                    break;
                }
            }

            ClosePopup();
        }
        catch (Exception ex)
        {
            ErrorStatusText.Text = $"Could not start Steam automatically: {ex.Message}";
        }
        finally
        {
            TurnOnSteamButton.IsEnabled = true;
            TurnOnSteamButton.Content = "Turn On Steam";
        }
    }

    
    private void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);
        ClosePopup();
    }

    private void ClosePopup()
    {
        IsVisible = false;

        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.BottomLeftButtonsPanel.IsVisible = true;
            TopBar.SetState(MainWindow.StateAtTheMoment);
        }
    }

    private void Button_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }
}