using Windows.UI.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ProjectBetterLauncher.Views;

public partial class TopBar : UserControl
{
    public static TopBar? Instance { get; private set; }

    public TopBar()
    {
        InitializeComponent();
        
        Instance  = this;
    }

    public static void SetState(string state)
    {
        if (Instance != null)
        {
            Instance.LauncherStateText.Text = state;
        }
    }

    private void CloseLauncher_OnClick(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.Close();
    }

    private void Maximize_OnClick(object? sender, RoutedEventArgs e)
    {
        if (MainWindow.Instance?.WindowState == WindowState.Maximized)
        {
            MainWindow.Instance.WindowState = WindowState.Normal;
        }
        else
        {
            MainWindow.Instance.WindowState = WindowState.Maximized;
        }
    }

    private void Minimize_OnClick(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.WindowState = WindowState.Minimized;
    }
}