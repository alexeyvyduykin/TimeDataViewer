using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using AvaloniaDemo.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaDemo.Models;

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
                    Interval1 = new ObservableCollection<Interval>(data.Intervals[0].ToArray()),
                    Interval2 = new ObservableCollection<Interval>(data.Intervals[1].ToArray()),
                    Interval3 = new ObservableCollection<Interval>(data.Intervals[2].ToArray()),
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
