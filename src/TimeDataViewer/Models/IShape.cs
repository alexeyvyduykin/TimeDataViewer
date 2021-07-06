using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timeline.Models
{
    public interface IShape
    {
        ITimeline? Scheduler { get; }

        IMarker? Marker { get; }
    }
}
