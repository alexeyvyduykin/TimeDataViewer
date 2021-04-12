using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.Views;
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

            Scheduler = this.FindControl<SchedulerControl>("Scheduler");

            Init();
        }

        SchedulerControl Scheduler;

        void Init()
        {
            BaseModelView model = new BaseModelView(BaseModelView.EDataType.New/*Old*/, RealDataPool.ETimePeriod.Week/*Day*/);

            foreach (var str in model.Strings)
            {
                var markerStr = new SchedulerString(str.Name);
                {
                    var shape = new StringVisual(markerStr);

                    //       shape.Tooltip.SetValues(new StringData() { Name = str.Name, Description = str.Description });

                    markerStr.Shape = shape;
                }

                List<SchedulerInterval> ivalMarkers = new List<SchedulerInterval>();

                foreach (var itemIval in str.Intervals)
                {
                    var markerIval = new SchedulerInterval(itemIval.Left, itemIval.Right);
                    {
                        markerIval.String = markerStr;
                        var shape = new IntervalVisual(markerIval);
                        //shape.Tooltip.SetValues(
                        //    new IntervalData()
                        //    {
                        //        Name = itemIval.Name,
                        //        Id = itemIval.Id,
                        //        Begin = itemIval.Left,
                        //        End = itemIval.Right,
                        //        StringName = itemIval.Group.Name
                        //    });

                        markerIval.Shape = shape;
                    }
                    ivalMarkers.Add(markerIval);
                }

                Scheduler.AddIntervals(ivalMarkers, markerStr);
            }

        }
    }
}
