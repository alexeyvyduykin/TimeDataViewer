﻿#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System;
using Avalonia.Media;
using Avalonia.Visuals;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia;
using TimeDataViewer.Spatial;
using TimeDataViewer;
using TimeDataViewer.Core;

namespace TimeDataViewer.ViewModels
{
    public class MarkerViewModel : ViewModelBase
    {
        internal MarkerViewModel() { }

        public Point2D LocalPosition { get; set; }
         
        public virtual int AbsolutePositionX { get; set; }

        public virtual int AbsolutePositionY { get; set; }

        public int ZIndex { get; set; }
    }
}
