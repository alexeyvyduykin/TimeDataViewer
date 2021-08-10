﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public class PanManipulator : MouseManipulator
    {
        public PanManipulator(IPlotView plotView)
            : base(plotView)
        {
        }

        private Point2D PreviousPosition { get; set; }

        private bool IsPanEnabled { get; set; }

        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);
            if (!this.IsPanEnabled)
            {
                return;
            }

            this.View.SetCursorType(CursorType.Default);
            e.Handled = true;
        }

        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);
            if (!this.IsPanEnabled)
            {
                return;
            }

            if (this.XAxis != null)
            {
                this.XAxis.Pan(this.PreviousPosition, e.Position);
            }

            if (this.YAxis != null)
            {
                this.YAxis.Pan(this.PreviousPosition, e.Position);
            }

            this.PlotView.InvalidatePlot(false);
            this.PreviousPosition = e.Position;
            e.Handled = true;
        }

        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);
            this.PreviousPosition = e.Position;

            this.IsPanEnabled = (this.XAxis != null && this.XAxis.IsPanEnabled)
                                || (this.YAxis != null && this.YAxis.IsPanEnabled);

            if (this.IsPanEnabled)
            {
                this.View.SetCursorType(CursorType.Pan);
                e.Handled = true;
            }
        }
    }
}
