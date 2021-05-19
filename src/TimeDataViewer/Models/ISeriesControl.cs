using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.ViewModels;

namespace TimeDataViewer.Models
{
    public interface ISeriesControl
    {
        IShape CreateIntervalShape(IntervalViewModel interval);

        IShape CreateSeriesShape();
    }
}
