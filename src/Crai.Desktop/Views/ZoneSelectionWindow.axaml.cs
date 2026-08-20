using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Crai.Application.Contracts.Services;
using Avalonia.VisualTree;

namespace Crai.Desktop.Views;

public partial class ZoneSelectionWindow : Window
{
    private readonly List<Rect> _tempZones = new();
    private Point? _startPoint;
    private Border? _activeDrawingBorder;

    public ZoneSelectionWindow()
    {
        InitializeComponent();

        var screen = Screens.Primary ?? Screens.All[0];
        double scaling = screen.Scaling;

        foreach (var zone in TranslationZoneManager.ActiveZones)
        {
            _tempZones.Add(new Rect(
                zone.X / scaling,
                zone.Y / scaling,
                zone.Width / scaling,
                zone.Height / scaling
            ));
        }

        RedrawZones();
    }

    private void RedrawZones()
    {
        SelectionCanvas.Children.Clear();

        foreach (var rect in _tempZones)
        {
            var zoneBorder = CreateZoneBorder(rect);
            SelectionCanvas.Children.Add(zoneBorder);
        }
    }

    private Border CreateZoneBorder(Rect rect)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#33007ACC")),
            BorderBrush = new SolidColorBrush(Color.Parse("#FF007ACC")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(4),
            Width = rect.Width,
            Height = rect.Height
        };

        Canvas.SetLeft(border, rect.X);
        Canvas.SetTop(border, rect.Y);

        var grid = new Grid();
        var deleteBtn = new Button
        {
            Content = "✕",
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.Parse("#AAFF3B30")),
            Width = 20,
            Height = 20,
            Padding = new Thickness(0),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Thickness(4),
            FontSize = 10,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        deleteBtn.Click += (s, e) =>
        {
            _tempZones.Remove(rect);
            RedrawZones();
        };

        grid.Children.Add(deleteBtn);
        border.Child = grid;

        return border;
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        var point = e.GetPosition(SelectionCanvas);
        var hit = SelectionCanvas.InputHitTest(point);
        if (hit is Button || (hit is Visual visual && visual.FindAncestorOfType<Button>() != null))
        {
            return;
        }

        _startPoint = point;

        _activeDrawingBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#11007ACC")),
            BorderBrush = new SolidColorBrush(Color.Parse("#AA007ACC")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(4)
        };
        SelectionCanvas.Children.Add(_activeDrawingBorder);
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (_startPoint == null || _activeDrawingBorder == null) return;

        var currentPoint = e.GetPosition(SelectionCanvas);

        double x = Math.Min(_startPoint.Value.X, currentPoint.X);
        double y = Math.Min(_startPoint.Value.Y, currentPoint.Y);
        double w = Math.Abs(_startPoint.Value.X - currentPoint.X);
        double h = Math.Abs(_startPoint.Value.Y - currentPoint.Y);

        _activeDrawingBorder.Width = w;
        _activeDrawingBorder.Height = h;
        Canvas.SetLeft(_activeDrawingBorder, x);
        Canvas.SetTop(_activeDrawingBorder, y);
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (_startPoint == null || _activeDrawingBorder == null) return;

        var currentPoint = e.GetPosition(SelectionCanvas);

        double x = Math.Min(_startPoint.Value.X, currentPoint.X);
        double y = Math.Min(_startPoint.Value.Y, currentPoint.Y);
        double w = Math.Abs(_startPoint.Value.X - currentPoint.X);
        double h = Math.Abs(_startPoint.Value.Y - currentPoint.Y);

        SelectionCanvas.Children.Remove(_activeDrawingBorder);
        _activeDrawingBorder = null;
        _startPoint = null;

        if (w > 20 && h > 20)
        {
            var newRect = new Rect(x, y, w, h);
            _tempZones.Add(newRect);
            RedrawZones();
        }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var screen = Screens.Primary ?? Screens.All[0];
        double scaling = screen.Scaling;

        var newZones = _tempZones.Select(rect => new TranslationZone(
            rect.X * scaling,
            rect.Y * scaling,
            rect.Width * scaling,
            rect.Height * scaling
        )).ToList();

        TranslationZoneManager.ActiveZones = newZones;

        Close();
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        _tempZones.Clear();
        RedrawZones();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
