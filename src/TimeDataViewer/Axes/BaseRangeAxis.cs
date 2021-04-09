#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Spatial;

namespace TimeDataViewer
{
    public abstract class BaseRangeAxis : BaseAxis
    {
        public abstract void UpdateDynamicLabelPosition(Point2D point);
    }
}
