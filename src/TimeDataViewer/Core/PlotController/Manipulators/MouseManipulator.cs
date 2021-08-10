﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    /// <summary>
    /// Provides an abstract base class for manipulators that handles mouse events.
    /// </summary>
    public abstract class MouseManipulator : PlotManipulator<OxyMouseEventArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseManipulator" /> class.
        /// </summary>
        /// <param name="plotView">The plot view.</param>
        protected MouseManipulator(IPlotView plotView)
            : base(plotView)
        {
        }

        /// <summary>
        /// Gets or sets the first position of the manipulation.
        /// </summary>
        public Point2D StartPosition { get; protected set; }

        /// <summary>
        /// Occurs when an input device begins a manipulation on the plot.
        /// </summary>
        /// <param name="e">The <see cref="OxyInputEventArgs" /> instance containing the event data.</param>
        public override void Started(OxyMouseEventArgs e)
        {
            this.AssignAxes(e.Position);
            base.Started(e);
            this.StartPosition = e.Position;
        }
    }
}
