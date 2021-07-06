using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.Spatial;

namespace Timeline.Models
{
    public interface IMarker
    {
        string Name { get; set; }
        
        Point2D LocalPosition { get; set; }

        int AbsolutePositionX { get; set; }

        int AbsolutePositionY { get; set; }
    }
}
