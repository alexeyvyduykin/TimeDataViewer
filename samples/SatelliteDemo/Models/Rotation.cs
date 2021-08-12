using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core = TimeDataViewer.Core;

namespace SatelliteDemo.Models
{
    public class Rotation : Core.TimelineItem
    {
        public string Category => "Rotation";

        public double BeginTime { get; set; }
           
        public double EndTime { get; set; }
     
        public double Angle { get; set; }    
    }
}
