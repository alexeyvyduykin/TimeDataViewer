﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.Spatial;
using Avalonia.Controls;
using Timeline.Core;

namespace Timeline.Models
{
    public interface ITimeline : IControl
    {
        Point2D FromAbsoluteToLocal(int x, int y);

        Point2I FromLocalToAbsolute(Point2D point);

        void ShowTooltip(Control placementTarget, Control tooltip);

        void HideTooltip();

        RectI AbsoluteWindow { get; }

        DateTime Epoch { get; }
        
        ITimeAxis AxisX { get; }

        ICategoryAxis AxisY { get; }

        event EventHandler OnZoomChanged;

        event EventHandler OnSizeChanged;
    }
}