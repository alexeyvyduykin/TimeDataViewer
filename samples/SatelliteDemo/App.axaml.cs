using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SatelliteDemo.ViewModels;
using SatelliteDemo.Views;
using TimeDataViewer;
using TimeDataViewer.Core;
using System.Collections.ObjectModel;

namespace SatelliteDemo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            //var scheduler = new SchedulerControl();
            //var ival = new Interval(0.0, 1.0);
            //var series = new Series();
            //series.Items = new ObservableCollection<Interval>() { ival };

            //scheduler.Series = new ObservableCollection<Series>() { series };
                 
            //var fdfd = scheduler.ItemCount;
       

            //int h = 0;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
