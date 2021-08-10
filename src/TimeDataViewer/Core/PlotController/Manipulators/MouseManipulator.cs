using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public abstract class MouseManipulator : PlotManipulator<OxyMouseEventArgs>
    {
        protected MouseManipulator(IPlotView plotView)
            : base(plotView)
        {
        }

        public Point2D StartPosition { get; protected set; }

        public override void Started(OxyMouseEventArgs e)
        {
            AssignAxes(e.Position);
            base.Started(e);
            StartPosition = e.Position;
        }
    }
}
