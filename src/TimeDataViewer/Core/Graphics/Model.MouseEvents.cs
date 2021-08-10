using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public partial class Model
    {
        private const double MouseHitTolerance = 10;

        private UIElement currentMouseEventElement;

        public event EventHandler<OxyMouseDownEventArgs> MouseDown;

        public event EventHandler<OxyMouseEventArgs> MouseMove;

        public event EventHandler<OxyMouseEventArgs> MouseUp;

        public event EventHandler<OxyMouseEventArgs> MouseEnter;

        public event EventHandler<OxyMouseEventArgs> MouseLeave;

        public virtual void HandleMouseDown(object sender, OxyMouseDownEventArgs e)
        {
            //var args = new HitTestArguments(e.Position, MouseHitTolerance);
            //foreach (var result in HitTest(args))
            //{
            //    e.HitTestResult = result;
            //    result.Element.OnMouseDown(e);
            //    if (e.Handled)
            //    {
            //        currentMouseEventElement = result.Element;
            //        return;
            //    }
            //}

            if (!e.Handled)
            {
                OnMouseDown(sender, e);
            }
        }

        public virtual void HandleMouseMove(object sender, OxyMouseEventArgs e)
        {
            if (currentMouseEventElement != null)
            {
                currentMouseEventElement.OnMouseMove(e);
            }

            if (!e.Handled)
            {
                OnMouseMove(sender, e);
            }
        }

        public virtual void HandleMouseUp(object sender, OxyMouseEventArgs e)
        {
            if (currentMouseEventElement != null)
            {
                currentMouseEventElement.OnMouseUp(e);
                currentMouseEventElement = null;
            }

            if (!e.Handled)
            {
                OnMouseUp(sender, e);
            }
        }

        public virtual void HandleMouseEnter(object sender, OxyMouseEventArgs e)
        {
            if (!e.Handled)
            {
                OnMouseEnter(sender, e);
            }
        }

        public virtual void HandleMouseLeave(object sender, OxyMouseEventArgs e)
        {
            if (!e.Handled)
            {
                OnMouseLeave(sender, e);
            }
        }

        protected virtual void OnMouseDown(object sender, OxyMouseDownEventArgs e)
        {                
            MouseDown?.Invoke(sender, e);            
        }

        protected virtual void OnMouseMove(object sender, OxyMouseEventArgs e)
        {
            MouseMove?.Invoke(sender, e);
        }

        protected virtual void OnMouseUp(object sender, OxyMouseEventArgs e)
        {                
            MouseUp?.Invoke(sender, e);            
        }

        protected virtual void OnMouseEnter(object sender, OxyMouseEventArgs e)
        {
            MouseEnter?.Invoke(sender, e);
        }

        protected virtual void OnMouseLeave(object sender, OxyMouseEventArgs e)
        {
            MouseLeave?.Invoke(sender, e);
        }
    }
}
