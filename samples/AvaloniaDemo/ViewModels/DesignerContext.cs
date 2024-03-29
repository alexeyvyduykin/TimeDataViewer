﻿using System.Collections.ObjectModel;
using AvaloniaDemo.Models;

namespace AvaloniaDemo.ViewModels
{
    public class DesignerContext
    {
        public static BaseModelView? BaseModelView { get; set; }

        public static IntervalTooltipViewModel IntervalTooltip { get; set; }

        public static IntervalTooltipViewModel2 IntervalTooltip2 { get; set; }

        public static void InitializeContext()
        {
            //var begin = DateTime.Now;
            //var duration = TimeSpan.FromDays(1);

            BaseModelView = new BaseModelView()
            {
                Interval1 = DesignerData.Interval1,
                Interval2 = DesignerData.Interval2,
                Interval3 = DesignerData.Interval3
            };

            var strng = new SeriesViewModel(/*"Observation"*/);
            var marker = new IntervalViewModel(43882.0, 48323.0);
            marker.Series = strng;

            IntervalTooltip = new IntervalTooltipViewModel(marker);

            IntervalTooltip2 = new IntervalTooltipViewModel2(marker);
        }
    }

    static class DesignerData
    {
        public static ObservableCollection<TimeInterval> Interval1 = new()
        {
            new TimeInterval(10, 30),
            new TimeInterval(45, 89),
            new TimeInterval(103, 243),
            new TimeInterval(300, 310),
            new TimeInterval(312, 320),
        };

        public static ObservableCollection<TimeInterval> Interval2 = new()
        {
            new TimeInterval(10, 30),
            new TimeInterval(45, 89),
            new TimeInterval(103, 243),
            new TimeInterval(300, 310),
            new TimeInterval(312, 320),
        };

        public static ObservableCollection<TimeInterval> Interval3 = new()
        {
            new TimeInterval(10, 30),
            new TimeInterval(45, 89),
            new TimeInterval(103, 243),
            new TimeInterval(300, 310),
            new TimeInterval(312, 320),
        };
    }
}
