using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
//using AvaloniaDemo.Views;
using AvaloniaDemo.ViewModels;
using System.Collections.Generic;
using AvaloniaDemo.Markers;
using AvaloniaDemo.Models;

namespace AvaloniaDemo
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
