using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public interface IPlotView : IView
    {
        new PlotModel ActualModel { get; }

        // Invalidates the plot (not blocking the UI thread)
        void InvalidatePlot(bool updateData = true);

        // Stores text on the clipboard.
        void SetClipboardText(string text);
    }
}
