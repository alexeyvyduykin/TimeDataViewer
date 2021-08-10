using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public partial class Model
    {
        /// <summary>
        /// The mouse hit tolerance.
        /// </summary>
        private const double MouseHitTolerance = 10;

        /// <summary>
        /// The element that receives mouse move events.
        /// </summary>
        private UIElement currentMouseEventElement;

        /// <summary>
        /// Occurs when a mouse button is pressed down on the model.
        /// </summary>
        public event EventHandler<OxyMouseDownEventArgs> MouseDown;

        /// <summary>
        /// Occurs when the mouse is moved on the plot element (only occurs after MouseDown).
        /// </summary>
        public event EventHandler<OxyMouseEventArgs> MouseMove;

        /// <summary>
        /// Occurs when the mouse button is released on the plot element.
        /// </summary>
        public event EventHandler<OxyMouseEventArgs> MouseUp;

        /// <summary>
        /// Occurs when the mouse cursor enters the plot area.
        /// </summary>
        public event EventHandler<OxyMouseEventArgs> MouseEnter;

        /// <summary>
        /// Occurs when the mouse cursor leaves the plot area.
        /// </summary>
        public event EventHandler<OxyMouseEventArgs> MouseLeave;

        /// <summary>
        /// Handles the mouse down event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public virtual void HandleMouseDown(object sender, OxyMouseDownEventArgs e)
        {
            var args = new HitTestArguments(e.Position, MouseHitTolerance);
            foreach (var result in this.HitTest(args))
            {
                e.HitTestResult = result;
                result.Element.OnMouseDown(e);
                if (e.Handled)
                {
                    this.currentMouseEventElement = result.Element;
                    return;
                }
            }

            if (!e.Handled)
            {
                this.OnMouseDown(sender, e);
            }
        }

        /// <summary>
        /// Handles the mouse move event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public virtual void HandleMouseMove(object sender, OxyMouseEventArgs e)
        {
            if (this.currentMouseEventElement != null)
            {
                this.currentMouseEventElement.OnMouseMove(e);
            }

            if (!e.Handled)
            {
                this.OnMouseMove(sender, e);
            }
        }

        /// <summary>
        /// Handles the mouse up event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public virtual void HandleMouseUp(object sender, OxyMouseEventArgs e)
        {
            if (this.currentMouseEventElement != null)
            {
                this.currentMouseEventElement.OnMouseUp(e);
                this.currentMouseEventElement = null;
            }

            if (!e.Handled)
            {
                this.OnMouseUp(sender, e);
            }
        }

        /// <summary>
        /// Handles the mouse enter event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public virtual void HandleMouseEnter(object sender, OxyMouseEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnMouseEnter(sender, e);
            }
        }

        /// <summary>
        /// Handles the mouse leave event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public virtual void HandleMouseLeave(object sender, OxyMouseEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnMouseLeave(sender, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyMouseEventArgs" /> instance containing the event data.</param>
        protected virtual void OnMouseDown(object sender, OxyMouseDownEventArgs e)
        {
            var handler = this.MouseDown;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseMove" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyMouseEventArgs" /> instance containing the event data.</param>
        protected virtual void OnMouseMove(object sender, OxyMouseEventArgs e)
        {
            var handler = this.MouseMove;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyMouseEventArgs" /> instance containing the event data.</param>
        protected virtual void OnMouseUp(object sender, OxyMouseEventArgs e)
        {
            var handler = this.MouseUp;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseEnter" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyMouseEventArgs" /> instance containing the event data.</param>
        protected virtual void OnMouseEnter(object sender, OxyMouseEventArgs e)
        {
            var handler = this.MouseEnter;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseLeave" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OxyMouseEventArgs" /> instance containing the event data.</param>
        protected virtual void OnMouseLeave(object sender, OxyMouseEventArgs e)
        {
            var handler = this.MouseLeave;
            if (handler != null)
            {
                handler(sender, e);
            }
        }
    }
}
