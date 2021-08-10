using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;
using System.Diagnostics;

namespace TimeDataViewer.Core
{
    public enum AxisPosition
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }

    public record AxisLabelPosition
    {
        public string? Label { get; init; }
        public double Value { get; init; }
    }

    public abstract class Axis
    {
        private double _scale;
        private double _offset;        

        private AxisPosition _position;

        public event EventHandler? OnAxisChanged;

        public bool IsPanEnabled { get; set; }

        public bool IsZoomEnabled { get; set; }

        public bool IsDynamicLabelEnable { get; set; }

        public string Header { get; set; } = "Header";

        public AxisPosition Position 
        {
            get => _position;
            set => _position = value;
        }

        public double MinValue { get; protected set; }

        public double MaxValue { get; protected set; }

        public double MinClientValue { get; protected set; }

        public double MaxClientValue { get; protected set; }

        public int MinPixel { get; protected set; }

        public int MaxPixel { get; protected set; }

        public abstract AxisInfo AxisInfo { get; }
                         
        public double FromAbsoluteToLocal(double pixel)
        {
            double scale = (MaxPixel - MinPixel) / (MaxValue - MinValue);

            return pixel / scale + MinValue;
            
            //return Math.Clamp(value, MinValue, MaxValue);
        }

        public double FromLocalToAbsolute(double value)
        {
            double scale = (MaxPixel - MinPixel) / (MaxValue - MinValue);

            return (value - MinValue) * scale;
         
            //return Math.Clamp(pixel, MinPixel, MaxPixel);
        }

        public Point2I FromLocalToScreen(double x, double y, Axis axisy)
        {
            return new Point2I((int)FromLocalToAbsolute(x), (int)axisy.FromLocalToAbsolute(y));     
        }

        public Point2I FromLocalToAbsolute(double x, double y, Axis axisy)
        {
            return new Point2I((int)FromLocalToAbsolute(x), (int)axisy.FromLocalToAbsolute(y));
        }

        public Point2D InverseTransform(double x, double y, Axis yaxis)
        {
            throw new Exception();
            //return new Point2D(InverseTransform(x), yaxis != null ? yaxis.InverseTransform(y) : 0);
        }

        public double InverseTransform(double sx)
        {
            throw new Exception();
            //return (sx / _scale) + offset;
        }

        public virtual void ZoomAt(double factor, double x)
        {
            throw new Exception();
            //if (!this.IsZoomEnabled)
            //{
            //    return;
            //}

            //var oldMinimum = this.ActualMinimum;
            //var oldMaximum = this.ActualMaximum;

            //double dx0 = (this.ActualMinimum - x) * _scale;
            //double dx1 = (this.ActualMaximum - x) * _scale;
            //_scale *= factor;

            //double newMinimum = (dx0 / _scale) + x;
            //double newMaximum = (dx1 / _scale) + x;

            //if (newMaximum - newMinimum > this.MaximumRange)
            //{
            //    var mid = (newMinimum + newMaximum) * 0.5;
            //    newMaximum = mid + (this.MaximumRange * 0.5);
            //    newMinimum = mid - (this.MaximumRange * 0.5);
            //}

            //if (newMaximum - newMinimum < this.MinimumRange)
            //{
            //    var mid = (newMinimum + newMaximum) * 0.5;
            //    newMaximum = mid + (this.MinimumRange * 0.5);
            //    newMinimum = mid - (this.MinimumRange * 0.5);
            //}

            //newMinimum = Math.Max(newMinimum, this.AbsoluteMinimum);
            //newMaximum = Math.Min(newMaximum, this.AbsoluteMaximum);

            //ViewMinimum = newMinimum;
            //ViewMaximum = newMaximum;
            //UpdateActualMaxMin();

            //var deltaMinimum = this.ActualMinimum - oldMinimum;
            //var deltaMaximum = this.ActualMaximum - oldMaximum;

            //this.OnAxisChanged(new AxisChangedEventArgs(AxisChangeTypes.Zoom, deltaMinimum, deltaMaximum));
        }

        public virtual void Pan(Point2D ppt, Point2D cpt)
        {
            throw new Exception();
            //if (!this.IsPanEnabled)
            //{
            //    return;
            //}

            //bool isHorizontal = this.IsHorizontal();

            //double dsx = isHorizontal ? cpt.X - ppt.X : cpt.Y - ppt.Y;
            //this.Pan(dsx);
        }

        public virtual void Zoom(double x0, double x1)
        {
            throw new Exception();
            //if (!this.IsZoomEnabled)
            //{
            //    return;
            //}

            //var oldMinimum = this.ActualMinimum;
            //var oldMaximum = this.ActualMaximum;

            //double newMinimum = Math.Max(Math.Min(x0, x1), this.AbsoluteMinimum);
            //double newMaximum = Math.Min(Math.Max(x0, x1), this.AbsoluteMaximum);

            //ViewMinimum = newMinimum;
            //ViewMaximum = newMaximum;
            //UpdateActualMaxMin();

            //var deltaMinimum = this.ActualMinimum - oldMinimum;
            //var deltaMaximum = this.ActualMaximum - oldMaximum;

            //this.OnAxisChanged(new AxisChangedEventArgs(AxisChangeTypes.Zoom, deltaMinimum, deltaMaximum));
        }

        public bool IsHorizontal()
        {
            return _position == AxisPosition.Top || _position == AxisPosition.Bottom;
        }

        public bool IsVertical()
        {
            return _position == AxisPosition.Left || _position == AxisPosition.Right;
        }

        protected virtual void Invalidate()
        {
            OnAxisChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateWindow(RectI window)
        {
            if(IsHorizontal() == true)
            {
                MinPixel = 0;
                MaxPixel = window.Width;
            }
            else
            {
                MinPixel = 0;
                MaxPixel = window.Height;
            }

            OnAxisChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateViewport(RectD viewport)
        {
            if (IsHorizontal() == true)
            {
                MinValue = viewport.Left;
                MaxValue = viewport.Right;
            }
            else
            {
                MinValue = viewport.Bottom;
                MaxValue = viewport.Top;
            }

            OnAxisChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateClientViewport(RectD clientViewport)
        {
            if (IsHorizontal() == true)
            {
                MinClientValue = clientViewport.Left;
                MaxClientValue = clientViewport.Right;
            }
            else
            {
                MinClientValue = clientViewport.Bottom;                    
                MaxClientValue = clientViewport.Top;
            }

            OnAxisChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
