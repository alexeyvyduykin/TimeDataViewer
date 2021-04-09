#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Spatial;

namespace TimeDataViewer
{
    public interface ITargetMarker
    {
        public Point2D LocalPosition { get; }

        public string Name { get; set; }
    }
}
