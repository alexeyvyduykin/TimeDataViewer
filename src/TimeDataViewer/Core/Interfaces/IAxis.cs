using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;

namespace TimeDataViewer.Core
{
    public interface IAxis
    {
        event EventHandler OnAxisChanged;

        AxisType Type { get; set; }
        string Header { get; set; }
        bool HasInversion { get; set; }
        AxisInfo AxisInfo { get; }
        bool IsDynamicLabelEnable { get; set; }

        double MinValue { get; }

        double MaxValue { get; }

        double MinScreenValue { get; }

        double MaxScreenValue { get; }

        int MinPixel { get; }

        int MaxPixel { get; }

        void UpdateViewport(RectD viewport);
        void UpdateScreen(RectD screen);
        void UpdateWindow(RectI window);
        double FromAbsoluteToLocal(int pixel);
        int FromLocalToAbsolute(double value);
    }
}
