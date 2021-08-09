using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.ViewModels;

namespace AvaloniaDemo.ViewModels
{
    public class IntervalTooltipViewModel2 : IntervalTooltipViewModel
    {
        private int _absX;
        private int _absY;

        public IntervalTooltipViewModel2(IntervalViewModel marker) : base(marker)
        {
            _absX = marker.AbsolutePositionX;
            _absY = marker.AbsolutePositionY;
        }

        public int AbsX
        {
            get => _absX;
            set => RaiseAndSetIfChanged(ref _absX, value);
        }

        public int AbsY
        {
            get => _absY;
            set => RaiseAndSetIfChanged(ref _absY, value);
        }
    }
}
