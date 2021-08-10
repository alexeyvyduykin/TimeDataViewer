﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    //public class ZoomRectangleManipulator : MouseManipulator
    //{
    //    /// <summary>
    //    /// The zoom rectangle.
    //    /// </summary>
    //    private RectD zoomRectangle;

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ZoomRectangleManipulator" /> class.
    //    /// </summary>
    //    /// <param name="plotView">The plot view.</param>
    //    public ZoomRectangleManipulator(IPlotView plotView)
    //        : base(plotView)
    //    {
    //    }

    //    /// <summary>
    //    /// Gets or sets a value indicating whether zooming is enabled.
    //    /// </summary>
    //    private bool IsZoomEnabled { get; set; }

    //    /// <summary>
    //    /// Occurs when a manipulation is complete.
    //    /// </summary>
    //    /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
    //    public override void Completed(OxyMouseEventArgs e)
    //    {
    //        base.Completed(e);
    //        if (!this.IsZoomEnabled)
    //        {
    //            return;
    //        }

    //        this.PlotView.SetCursorType(CursorType.Default);
    //        this.PlotView.HideZoomRectangle();

    //        if (this.zoomRectangle.Width > 10 && this.zoomRectangle.Height > 10)
    //        {
    //            var p0 = this.InverseTransform(this.zoomRectangle.Left, this.zoomRectangle.Top);
    //            var p1 = this.InverseTransform(this.zoomRectangle.Right, this.zoomRectangle.Bottom);

    //            if (this.XAxis != null)
    //            {
    //                this.XAxis.Zoom(p0.X, p1.X);
    //            }

    //            if (this.YAxis != null)
    //            {
    //                this.YAxis.Zoom(p0.Y, p1.Y);
    //            }

    //            this.PlotView.InvalidatePlot();
    //        }

    //        e.Handled = true;
    //    }

    //    /// <summary>
    //    /// Occurs when the input device changes position during a manipulation.
    //    /// </summary>
    //    /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
    //    public override void Delta(OxyMouseEventArgs e)
    //    {
    //        base.Delta(e);
    //        if (!this.IsZoomEnabled)
    //        {
    //            return;
    //        }

    //        var plotArea = this.PlotView.ActualModel.PlotArea;

    //        var x = Math.Min(this.StartPosition.X, e.Position.X);
    //        var w = Math.Abs(this.StartPosition.X - e.Position.X);
    //        var y = Math.Min(this.StartPosition.Y, e.Position.Y);
    //        var h = Math.Abs(this.StartPosition.Y - e.Position.Y);

    //        if (this.XAxis == null || !this.XAxis.IsZoomEnabled)
    //        {
    //            x = plotArea.Left;
    //            w = plotArea.Width;
    //        }

    //        if (this.YAxis == null || !this.YAxis.IsZoomEnabled)
    //        {
    //            y = plotArea.Top;
    //            h = plotArea.Height;
    //        }

    //        this.zoomRectangle = new RectD(x, y, w, h);
    //        this.PlotView.ShowZoomRectangle(this.zoomRectangle);
    //        e.Handled = true;
    //    }

    //    /// <summary>
    //    /// Occurs when an input device begins a manipulation on the plot.
    //    /// </summary>
    //    /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
    //    public override void Started(OxyMouseEventArgs e)
    //    {
    //        base.Started(e);

    //        this.IsZoomEnabled = (this.XAxis != null && this.XAxis.IsZoomEnabled)
    //                 || (this.YAxis != null && this.YAxis.IsZoomEnabled);

    //        if (this.IsZoomEnabled)
    //        {
    //            this.zoomRectangle = new RectD(this.StartPosition.X, this.StartPosition.Y, 0, 0);
    //            this.PlotView.ShowZoomRectangle(this.zoomRectangle);
    //            this.PlotView.SetCursorType(this.GetCursorType());
    //            e.Handled = true;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the cursor for the manipulation.
    //    /// </summary>
    //    /// <returns>The cursor.</returns>
    //    private CursorType GetCursorType()
    //    {
    //        if (this.XAxis == null)
    //        {
    //            return CursorType.ZoomVertical;
    //        }

    //        if (this.YAxis == null)
    //        {
    //            return CursorType.ZoomHorizontal;
    //        }

    //        return CursorType.ZoomRectangle;
    //    }
    //}
}