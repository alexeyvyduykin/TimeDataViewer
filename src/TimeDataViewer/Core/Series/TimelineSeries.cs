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

        public string CategoryField { get; set; }

        public string BeginField { get; set; }

        public string EndField { get; set; }

        public double MinTime() => (Items.Count == 0) ? 0.0 : Items.Min(s => s.Begin);

        public double MaxTime() => (Items.Count == 0) ? 0.0 : Items.Max(s => s.End);
    }
}
