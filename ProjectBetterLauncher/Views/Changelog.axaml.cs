using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ProjectBetterLauncher.Views;

public partial class Changelog : UserControl
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");

    public Changelog()
    {
        InitializeComponent();
    }

    public void SetChangelog(string? title, List<string>? items)
    {
        ChangelogTitleText.Text = string.IsNullOrWhiteSpace(title) 
            ? "LATEST CHANGES" 
            : title;

        ChangelogItemsPanel.Children.Clear();

        if (items == null || items.Count == 0)
        {
            ChangelogItemsPanel.Children.Add(new TextBlock
            {
                Text = "No changelog entries available for this version.",
                Foreground = Brushes.Gray,
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });
            return;
        }

        foreach (string item in items)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;

            string trimmed = item.Trim();

            if (trimmed.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(trimmed);
                    using var assetStream = AssetLoader.Open(uri);
                    var bitmap = new Bitmap(assetStream);

                    var imageControl = new Image
                    {
                        Source = bitmap,
                        Stretch = Stretch.Uniform,
                        MaxWidth = 720,
                        MaxHeight = 340,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var imageBorder = new Border
                    {
                        BorderBrush = Brushes.White,
                        BorderThickness = new Thickness(2),
                        Background = new SolidColorBrush(Color.Parse("#141414")),
                        Padding = new Thickness(6),
                        Margin = new Thickness(0, 6, 0, 6),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Child = imageControl,
                        Effect = new DropShadowEffect
                        {
                            BlurRadius = 10,
                            Color = Colors.Black,
                            Opacity = 0.8
                        }
                    };

                    ChangelogItemsPanel.Children.Add(imageBorder);
                }
                catch (Exception ex)
                {
                    ChangelogItemsPanel.Children.Add(new TextBlock
                    {
                        Text = $"[Could not load image: {trimmed}]",
                        Foreground = Brushes.Red,
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap
                    });
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                var rowGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,*")
                };

                var bullet = new TextBlock
                {
                    Text = ">>",
                    Foreground = Brushes.Yellow,
                    FontSize = 15,
                    Margin = new Thickness(0, 0, 12, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };

                var textBlock = new TextBlock
                {
                    Text = trimmed,
                    Foreground = Brushes.White,
                    FontSize = 15,
                    LineHeight = 22,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(bullet, 0);
                Grid.SetColumn(textBlock, 1);

                rowGrid.Children.Add(bullet);
                rowGrid.Children.Add(textBlock);

                var entryCard = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#141414")),
                    BorderBrush = Brushes.Yellow,
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    Padding = new Thickness(14, 12),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Child = rowGrid
                };

                ChangelogItemsPanel.Children.Add(entryCard);
            }
        }
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TopBar.SetState(MainWindow.StateAtTheMoment);
        App.PlaySound(_clickSoundPath);

        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.Changelog.IsVisible = false;
            MainWindow.Instance.BottomLeftButtonsPanel.IsVisible = true;
        }
    }

    private void BackButton_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        App.PlaySound(_hoverSoundPath);
    }
}