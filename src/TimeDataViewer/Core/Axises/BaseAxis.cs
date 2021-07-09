using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.Spatial;
using Timeline.ViewModels;
using System.Diagnostics;

namespace Timeline.Core
{
    public record AxisLabelPosition
    {
        public string? Label { get; init; }
        public double Value { get; init; }
    }

    public abstract class BaseAxis : IAxis
    {
    //    public bool HasInversion { get; set; }

    //    public bool IsDynamicLabelEnable { get; set; }

    //    public string Header { get; set; } = "Header";

        public double MinValue { get; protected set; }

        public double MaxValue { get; protected set; }

        public double MinClientValue { get; protected set; }

        public double MaxClientValue { get; protected set; }

        public int MinPixel { get; protected set; }

        public int MaxPixel { get; protected set; }
                         
        public abstract double FromAbsoluteToLocal(int pixel);

        public abstract int FromLocalToAbsolute(double value);
      
        public abstract void UpdateWindow(RectI window);

        public abstract void UpdateViewport(RectD viewport);

        public abstract void UpdateClientViewport(RectD clientViewport);
    }
}
