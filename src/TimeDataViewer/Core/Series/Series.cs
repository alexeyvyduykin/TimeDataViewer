using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace TimeDataViewer.Core
{
    public abstract class Series
    {
        public PlotModel Parent { get; set; }

        public IEnumerable ItemsSource { get; set; }

        protected internal abstract void UpdateData(); 
        
        public EventHandler MyRender;

        public abstract void MyOnRender(); 
        
        public abstract void Render(/*IRenderContext rc*/);
    }
}
