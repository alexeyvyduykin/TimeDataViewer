using System;

namespace TimeDataViewer.Core
{
    public abstract class UIElement : SelectableElement
    {
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

        protected internal virtual void OnMouseDown(OxyMouseDownEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        protected internal virtual void OnMouseMove(OxyMouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        protected internal virtual void OnMouseUp(OxyMouseEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        public HitTestResult HitTest(HitTestArguments args)
        {
            return this.HitTestOverride(args);
        }
        protected virtual HitTestResult HitTestOverride(HitTestArguments args)
        {
            return null;
        }
    }
}
