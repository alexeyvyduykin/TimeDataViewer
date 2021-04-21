#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System;
using Avalonia.Media;
using Avalonia.Visuals;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia;
using TimeDataViewer.Spatial;
using TimeDataViewer;

namespace TimeDataViewer.ViewModels
{
    public class MarkerViewModel : ViewModelBase
    {
        private int _absolutePositionX;
        private int _absolutePositionY;
        private int _zIndex;
        private Point2D _offset;
        private SchedulerControl _map;
        private bool _first = false;

        internal MarkerViewModel() { }

        public bool IsFreeze { get; set; } = false;

        public Visual Shape { get; set; }

        public void SetLocalPosition(double localPositionX, double localPositionY)
        {
            LocalPosition = new Point2D(localPositionX, localPositionY);

            UpdateAbsolutePosition();
        }

        public Point2D LocalPosition { get; protected set; }
         
        public SchedulerControl Map
        {
            get
            {
                if (Shape is not null && _map is null)
                {
                    IVisual visual = Shape;
                    while (visual != null && !(visual is SchedulerControl))
                    {
                        visual = visual.VisualParent;// VisualTreeHelper.GetParent(visual);
                    }

                    _map = visual as SchedulerControl;
                }

                return _map;
            }
            internal set
            {
                _map = value;
            }
        }

        public virtual Point2D Offset
        {
            get
            {
                return _offset;
            }
            set
            {
               // if (_offset != value)
              //  {
                    _offset = value;
                    UpdateAbsolutePosition();
                //}
            }
        }
      
        public virtual int AbsolutePositionX
        {
            get => _absolutePositionX;
            set => RaiseAndSetIfChanged(ref _absolutePositionX, value);
        }
  
        public virtual int AbsolutePositionY
        {
            get => _absolutePositionY;            
            set => RaiseAndSetIfChanged(ref _absolutePositionY, value);
        }
       
        public int ZIndex
        {
            get => _zIndex;
            set => RaiseAndSetIfChanged(ref _zIndex, value);
        }

        public virtual void Clear()
        {
            var s = (Shape as IDisposable);
            if (s != null)
            {
                s.Dispose();
                s = null;
            }
            Shape = null;
        }
    
        protected virtual void UpdateAbsolutePosition()
        {
            if (Map != null)
            {
                if (IsFreeze == true && _first == true)
                    return;

                var p = Map.FromLocalToAbsolute(LocalPosition);

                AbsolutePositionX = p.X + (int)Offset.X;
                AbsolutePositionY = p.Y + (int)Offset.Y;

                _first = true;
            }
        }

        public void ResetOffset()
        {
            LocalPosition = Map.FromAbsoluteToLocal(AbsolutePositionX, AbsolutePositionY);

            //  SCSchedulerPoint pos = Map.FromLocalToSchedulerPoint(LocalPositionX, LocalPositionY);

            _offset = new Point2D(0, 0);
        }

        internal void ForceUpdateLocalPosition(SchedulerControl m)
        {
            if (m is not null)
            {
                _map = m;
            }

            UpdateAbsolutePosition();
        }
    }
}
