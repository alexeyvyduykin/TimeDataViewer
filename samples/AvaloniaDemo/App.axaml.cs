using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.Models;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo
{
    public class App : Application
    {
        static App()
        {
            if (Design.IsDesignMode)
            {
                DesignerContext.InitializeContext();
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var data = new DataPool()
                {
                    NumSatellites = 3,
                    TimePeriod = ETimePeriod.Week
                };

                data.GenerateIntervals();

                var baseModelView = new BaseModelView()
                {
                    Epoch = data.Epoch,
                    Interval1 = new ObservableCollection<TimeInterval>(data.Intervals[0].ToArray()),
                    Interval2 = new ObservableCollection<TimeInterval>(data.Intervals[1].ToArray()),
                    Interval3 = new ObservableCollection<TimeInterval>(data.Intervals[2].ToArray()),
                };

                desktop.MainWindow = new MainWindow()
                {
                    DataContext = baseModelView
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
