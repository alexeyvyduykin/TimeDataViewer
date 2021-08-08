using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class TimelineSeries : Series
    {
        public IList<TimelineItem> Items { get; private set; }

        public string CategoryField { get; set; }

        public string BeginField { get; set; }

        public string EndField { get; set; }
    }
}
