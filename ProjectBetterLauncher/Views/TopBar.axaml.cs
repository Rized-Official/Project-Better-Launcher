using Avalonia;
using Avalonia.Controls;
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
}