using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TimeDataViewer.Core;
using System.Collections.ObjectModel;

namespace TimeDataViewer.UnitTests
{
    public class SchedulerControlTests
    {
        public record Rotation(double BeginTime, double EndTime, double Angle);
        
        private SchedulerControl CreateScheduler()
        {
            return new SchedulerControl();
        }

        [Fact]
        public void Series_SetElementWithoutIntervals_ItemsHaveString()
        {
            var scheduler = CreateScheduler();
            var series = new Series();
            series.Items = new ObservableCollection<Interval>();

            scheduler.Series = new ObservableCollection<Series>() { series };

            Assert.Equal(1, scheduler.ItemCount);
        }

        [Fact]
        public void Series_SetElementWithCustomIntervals_ItemsHaveStringAndInterval()
        {
            var scheduler = CreateScheduler();
            var rotation = new Rotation(0.0, 1.0, 10.0);
            var series = new Series();
            series.LeftBindingPath = "BeginTime";
            series.RightBindingPath = "EndTime";
            series.Items = new ObservableCollection<Rotation>() { rotation };
          
            scheduler.Series = new ObservableCollection<Series>() { series };
      
            Assert.Equal(2, scheduler.ItemCount);
        }

        [Fact]
        public void Series_SetElementWithIntervals_ItemsHaveStringAndInterval()
        {
            var scheduler = CreateScheduler();
            var series = new Series();
            var ival = new Interval(0.0, 1.0);
            series.Items = new ObservableCollection<Interval>() { ival };
                   
            scheduler.Series = new ObservableCollection<Series>() { series };

            Assert.Equal(2, scheduler.ItemCount);
        }
    }
}
