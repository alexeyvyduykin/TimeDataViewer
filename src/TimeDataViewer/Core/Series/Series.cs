using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace TimeDataViewer.Core
{
    public abstract class Series : PlotElement
    {
        public EventHandler MyRender;

        public abstract void MyOnRender();

        protected Series()
        {
            IsVisible = true;
        }

        public bool IsVisible { get; set; }

        // Renders the series on the specified render context.
        public abstract void Render();

        // Checks if this data series requires X/Y axes. (e.g. Pie series do not require axes)
        protected internal abstract bool AreAxesRequired();

        // Ensures that the axes of the series are defined.   
        protected internal abstract void EnsureAxes();

        // Checks if the data series is using the specified axis.
        protected internal abstract bool IsUsing(Axis axis);

        // Sets the default values (colors, line style etc.) from the plot model.     
        protected internal abstract void SetDefaultValues();

        // Updates the maximum and minimum values of the axes used by this series.    
        protected internal abstract void UpdateAxisMaxMin();

        // Updates the data of the series.    
        protected internal abstract void UpdateData();

        // Updates the valid data of the series.      
        protected internal abstract void UpdateValidData();

        // Updates the maximum and minimum values of the series.      
        protected internal abstract void UpdateMaxMin();
    }
}
