﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteDemo.Models
{
    public class Transmission
    {
        public double BeginTime { get; set; }

        public double EndTime { get; set; }
        
        public int Type { get; set; }

        public int IndexTarget { get; set; }
    }
}
