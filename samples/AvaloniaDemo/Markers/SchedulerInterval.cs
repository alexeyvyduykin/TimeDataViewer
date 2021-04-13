#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using TimeDataViewer.Spatial;

namespace AvaloniaDemo.Markers
{
    public class SchedulerInterval : SchedulerMarker
    {
        private SchedulerString _string;
        private Guid _id;

        public SchedulerInterval(double left, double right) : base()
        {
            Left = left;
            Right = right;

            Name = string.Format("Interval_{0}_{1}", (int)left, (int)right);

            _id = Guid.NewGuid();
        }
        
        public SchedulerString String
        {
            get
            {
                return _string;
            }
            set
            {
                _string = value;


                base.SetLocalPosition(Left + (Right - Left) / 2.0, _string.LocalPosition.Y);

                //   base.PositionX = Left + (Right - Left) / 2.0;
                //   base.PositionY = _string.PositionY;

                //  base.Position = new SCSchedulerPoint(Left + (Right - Left) / 2.0, _string.Position.Y);
            }
        }

        public Point2I Center
        {
            get
            {
                int x = base.AbsolutePositionX;
                int y = base.AbsolutePositionY;

                return new Point2I(x, y);
            }
        }
     
        public string Id => _id.ToString();

        public string Line => Id;

        public double Left { get; set; }

        public double Right { get; set; }

        public double Length => Right - Left;
    }

}
