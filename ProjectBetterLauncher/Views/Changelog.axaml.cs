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
using Avalonia.Media.Transformation;
using Avalonia.Platform;

namespace ProjectBetterLauncher.Views;

public partial class Changelog : UserControl
{
    private string _hoverSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Hover.wav");
    private string _clickSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "UI_Button_Press.wav");

    private bool _isImageZoomed = false;

    public Changelog()
    {
        InitializeComponent();
    }

    public void SetChangelog(string? title, List<string>? items, string? version = null, bool isNewUpdateAlert = false)
    {
        ChangelogBadgeText.Text = isNewUpdateAlert
            ? $"NEW UPDATE DETECTED ({version})"
            : (!string.IsNullOrWhiteSpace(version) ? $"VERSION {version}" : "OFFICIAL UPDATE");

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
                        MaxWidth = 750,
                        MaxHeight = 350,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var imageBorder = new Border
                    {
                        Classes = { "changelogImg" },
                        BorderBrush = Brushes.White,
                        BorderThickness = new Thickness(2),
                        Background = new SolidColorBrush(Color.Parse("#141414")),
                        Padding = new Thickness(6),
                        Margin = new Thickness(0, 8, 0, 8),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Cursor = new Cursor(StandardCursorType.Hand),
                        Child = imageControl,
                        Effect = new DropShadowEffect
                        {
                            BlurRadius = 12,
                            Color = Colors.Black,
                            Opacity = 0.85
                        }
                    };

                    imageBorder.PointerEntered += (_, _) => App.PlaySound(_hoverSoundPath);
                    imageBorder.PointerPressed += (_, _) => OpenImageViewer(bitmap);

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
            else if (trimmed.StartsWith("[1]"))
            {
                string content = trimmed.Substring(3).Trim();

                var headerStack = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 4,
                    Margin = new Thickness(10, 12, 10, 4)
                };

                var centeredText = new TextBlock
                {
                    Text = content,
                    Foreground = Brushes.Yellow,
                    FontSize = 18,
                    LineHeight = 24,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var underline = new Border
                {
                    Height = 2,
                    Width = 140,
                    Background = new SolidColorBrush(Color.FromArgb(90, 255, 255, 0)),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                headerStack.Children.Add(centeredText);
                headerStack.Children.Add(underline);

                ChangelogItemsPanel.Children.Add(headerStack);
            }
            else if (trimmed.StartsWith("[2]"))
            {
                string content = trimmed.Substring(3).Trim();

                var centeredSubText = new TextBlock
                {
                    Text = content,
                    Foreground = Brushes.Gray,
                    FontSize = 12,
                    LineHeight = 18,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(10, 0, 10, 6)
                };

                ChangelogItemsPanel.Children.Add(centeredSubText);
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
                    Background = new SolidColorBrush(Color.Parse("#131313")),
                    BorderBrush = Brushes.Yellow,
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    Padding = new Thickness(16, 12),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Child = rowGrid
                };

                ChangelogItemsPanel.Children.Add(entryCard);
            }
        }
    }

    private void OpenImageViewer(Bitmap bitmap)
    {
        App.PlaySound(_clickSoundPath);
        FullscreenImage.Source = bitmap;
        ResetImageZoom();
        ImageViewerOverlay.IsVisible = true;
    }

    private void ResetImageZoom()
    {
        _isImageZoomed = false;
        FullscreenImage.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        FullscreenImage.RenderTransform = TransformOperations.Parse("scale(1)");
        ImageViewport.Cursor = new Cursor(StandardCursorType.Hand);
        ZoomHintText.Text = "CLICK ON ANY SPOT TO ZOOM IN  |  CLICK OUTSIDE OR 'CLOSE' TO EXIT";
    }

    private void ImageViewport_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        App.PlaySound(_clickSoundPath);

        if (!_isImageZoomed)
        {
            double width = ImageViewport.Bounds.Width;
            double height = ImageViewport.Bounds.Height;

            if (width <= 0 || height <= 0) return;

            Point clickPos = e.GetPosition(ImageViewport);
            double relX = Math.Clamp(clickPos.X / width, 0.0, 1.0);
            double relY = Math.Clamp(clickPos.Y / height, 0.0, 1.0);

            _isImageZoomed = true;
            FullscreenImage.RenderTransformOrigin = new RelativePoint(relX, relY, RelativeUnit.Relative);
            FullscreenImage.RenderTransform = TransformOperations.Parse("scale(2.2)");
            ImageViewport.Cursor = new Cursor(StandardCursorType.Cross);
            ZoomHintText.Text = "CLICK AGAIN TO ZOOM OUT  |  CLICK OUTSIDE OR 'CLOSE' TO EXIT";
        }
        else
        {
            ResetImageZoom();
        }

        e.Handled = true;
    }

    private void OverlayBackground_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CloseImageViewer();
    }

    private void CloseImageViewer_OnClick(object? sender, RoutedEventArgs e)
    {
        CloseImageViewer();
    }

    private void CloseImageViewer()
    {
        App.PlaySound(_clickSoundPath);
        ResetImageZoom();
        ImageViewerOverlay.IsVisible = false;
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ImageViewerOverlay.IsVisible)
        {
            CloseImageViewer();
            return;
        }

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