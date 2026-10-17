using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ProjectBetterLauncher.Views;

public partial class Hibernation : UserControl
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");

    public Hibernation()
    {
        InitializeComponent();
    }

    private void StopHibernation_OnClick(object? sender, RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);
        MainWindow.Instance?.ExitHibernation();
    }

    private void CloseGame_OnClick(object? sender, RoutedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);

        if (MainWindow.Instance != null)
        {
            MainWindow.StateAtTheMoment = "In Launcher";
            MainWindow.Instance.CloseGame();
            TopBar.SetState(MainWindow.StateAtTheMoment);
            MainWindow.Instance.StopButton.IsVisible = false;
            MainWindow.Instance.PlayButton.IsVisible = true;
        }
    }

    private void StopHibernation_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }
}