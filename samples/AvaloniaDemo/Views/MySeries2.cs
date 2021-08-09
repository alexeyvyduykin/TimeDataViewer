using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.ViewModels;
using TimeDataViewer;
using AvaloniaDemo.ViewModels;
using System.Collections;

namespace AvaloniaDemo.Views
{
    public class MySeries2 : Series
    {
        public override TimeDataViewer.Core.Series CreateModel()
        {
            throw new NotImplementedException();
        }

        public override IntervalTooltipViewModel CreateTooltip(IntervalViewModel marker)
        {
            return new IntervalTooltipViewModel2(marker);
        }

        protected override void UpdateData(IEnumerable items)
        {
            throw new NotImplementedException();
        }
    }
}
