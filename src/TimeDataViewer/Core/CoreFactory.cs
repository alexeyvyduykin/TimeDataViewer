using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class CoreFactory
    {
        public PlotModel CreateArea()
        {
            return new PlotModel()
            {
                MinZoom = 0,
                MaxZoom = 100,
                ZoomScaleX = 1.0, // 30 %        
                ZoomScaleY = 0.0,
                CanDragMap = true,
                MouseWheelZoomEnabled = true,       
            };
        }
    }
}
