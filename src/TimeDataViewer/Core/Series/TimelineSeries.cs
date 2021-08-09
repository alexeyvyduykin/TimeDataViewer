using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class TimelineSeries : Series
    {
        public IList<TimelineItem> Items { get; private set; } = new List<TimelineItem>();

        public TimeDataViewer.Series SeriesControl{get;set;}

        public string CategoryField { get; set; }

        public string BeginField { get; set; }

        public string EndField { get; set; }

        public double MinTime() => (Items.Count == 0) ? 0.0 : Items.Min(s => s.Begin);

        public double MaxTime() => (Items.Count == 0) ? 0.0 : Items.Max(s => s.End);

        protected internal override void UpdateData()
        {
            if (ItemsSource != null)
            {
                if (ItemsSource is IEnumerable<TimelineItem> ivals)
                {
                    Items = new List<TimelineItem>(ivals);
                }
                else
                {
                    Items = UpdateItems();
                }
            }
        }

        private IList<TimelineItem> UpdateItems()
        {
            if (string.IsNullOrWhiteSpace(BeginField) == false && string.IsNullOrWhiteSpace(EndField) == false)
            {
                var list = new List<TimelineItem>();
             
                foreach (var item in ItemsSource)
                {
                    var propertyInfoLeft = item.GetType().GetProperty(BeginField);
                    var propertyInfoRight = item.GetType().GetProperty(EndField);

                    var valueLeft = propertyInfoLeft?.GetValue(item, null);
                    var valueRight = propertyInfoRight?.GetValue(item, null);

                    if (valueLeft is not null && valueRight is not null && valueLeft is double left && valueRight is double right)
                    {
                        list.Add(new TimelineItem(left, right)
                        {
                            ZIndex = 100,
                            SeriesControl = SeriesControl
                        });
                    }
                }
                return list;
            }

            return new List<TimelineItem>();
        }
    }
}
