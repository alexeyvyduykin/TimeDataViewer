using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public class InputEventArgs : EventArgs
    {   
        public bool Handled { get; set; }

        public Point2D Position { get; set; }
    }

    public partial class PlotModel
    {     
        // The mouse hit tolerance.    
        private const double MouseHitTolerance = 10;
       
        public event EventHandler<InputEventArgs> MouseDown;

        public event EventHandler<InputEventArgs> MouseMove;

        public event EventHandler<InputEventArgs> MouseUp;

        public event EventHandler<InputEventArgs> MouseEnter;

        public event EventHandler<InputEventArgs> MouseLeave;

        public virtual void HandleMouseDown(object sender, InputEventArgs e)
        {
            if (!e.Handled)
            {
                OnMouseDown(sender, e);
            }
        }

        public virtual void HandleMouseMove(object sender, InputEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnMouseMove(sender, e);
            }
        }

        public virtual void HandleMouseUp(object sender, InputEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnMouseUp(sender, e);
            }
        }

        public virtual void HandleMouseEnter(object sender, InputEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnMouseEnter(sender, e);
            }
        }

        public virtual void HandleMouseLeave(object sender, InputEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnMouseLeave(sender, e);
            }
        }

        protected virtual void OnMouseDown(object sender, InputEventArgs e)
        {
            var handler = this.MouseDown;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        protected virtual void OnMouseMove(object sender, InputEventArgs e)
        {
            var handler = this.MouseMove;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        protected virtual void OnMouseUp(object sender, InputEventArgs e)
        {
            var handler = this.MouseUp;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        protected virtual void OnMouseEnter(object sender, InputEventArgs e)
        {
            MouseEnter?.Invoke(sender, e);
        }

        protected virtual void OnMouseLeave(object sender, InputEventArgs e)
        {
            MouseLeave?.Invoke(sender, e);
        }
    }
}
