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
