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
        private AxisPosition _position;

        public event EventHandler? OnAxisChanged;

        public bool HasInversion { get; set; }

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
                         
        public abstract double FromAbsoluteToLocal(int pixel);

        public abstract int FromLocalToAbsolute(double value);

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
            //Debug.WriteLine($"BaseAxis -> OnAxisChanged -> Count = {OnAxisChanged?.GetInvocationList().Length}");
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
