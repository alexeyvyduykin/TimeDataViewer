using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core = TimeDataViewer.Core;

namespace SatelliteDemo.Models
{
    public class Observation : Core.TimelineItem
    {
        public string Category => "Observation";

        public double BeginTime { get; set; }

        public double EndTime { get; set; }

        public double Gam1 { get; set; }

        public double Gam2 { get; set; }

        public double Range1 { get; set; }

        public double Range2 { get; set; }
    }
}
