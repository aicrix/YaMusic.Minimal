using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using System;
using YaMusic.Minimal.ViewModels;

namespace YaMusic.Minimal.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly double margin = 10;

        public MainWindow()
        {
            InitializeComponent();
            PositionChanged += MainWindow_PositionChanged;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            ViewModel.outputDevice.Stop();
            ViewModel.outputDevice.Dispose();
        }

        private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (Position.X != Screens.Primary.Bounds.Width - margin - Width)
            {
                Position = new PixelPoint((int)(Screens.Primary.Bounds.Width - ((Width + margin) * Screens.Primary.Scaling)), Position.Y);
            }
        }

        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            //Position = new PixelPoint((int)(Screens.Primary.Bounds.Width - margin - Width), Screens.Primary.Bounds.Height / 2);
            Position = new PixelPoint((int)(Screens.Primary.Bounds.Width - ((Width + margin) * Screens.Primary.Scaling)), Screens.Primary.Bounds.Height / 2);
        }

        private void DragWindow(object? sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void Button_Clicked(object? sender, RoutedEventArgs e)
        {
            if (Position.X != Screens.Primary.Bounds.Width - margin - Width)
            {
                Position = new PixelPoint((int)(Screens.Primary.Bounds.Width - ((Width + margin) * Screens.Primary.Scaling)), Position.Y);
            }
        }
    }
}