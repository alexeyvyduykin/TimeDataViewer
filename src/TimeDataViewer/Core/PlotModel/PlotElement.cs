using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public abstract class PlotElement : UIElement
    {
        protected PlotElement() { }

        public PlotModel PlotModel => (PlotModel)Parent;
    }
}
