﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.Spatial;
using Timeline.ViewModels;

namespace Timeline.Core
{
    public interface IAxis
    {
        //event EventHandler OnAxisChanged;

        AxisType Type { get; set; }

        string Header { get; set; }

        bool HasInversion { get; set; }
  
        bool IsDynamicLabelEnable { get; set; }

        double MinValue { get; }

        double MaxValue { get; }

        double MinClientValue { get; }

        double MaxClientValue { get; }

        int MinPixel { get; }

        int MaxPixel { get; }

        void UpdateViewport(RectD viewport);

        void UpdateClientViewport(RectD clientViewport);

        void UpdateWindow(RectI window);

        double FromAbsoluteToLocal(int pixel);

        int FromLocalToAbsolute(double value);
    }
}
