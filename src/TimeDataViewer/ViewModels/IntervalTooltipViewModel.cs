using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Markers;

namespace TimeDataViewer.ViewModels
{
    public class IntervalTooltipViewModel : ViewModelBase
    {
        private readonly SchedulerInterval _marker;
        private string _category;
        private string _date;
        private string _begin;
        private string _end;

        public IntervalTooltipViewModel(SchedulerInterval marker)
        {
            _marker = marker;

            var left = marker.Left;
            var right = marker.Right;

            var strng = marker.String;
            // var epoch = DateTime.Now.Date;

            var epoch = marker.String.Map.Epoch;

            _category = strng.Name;
            _date = epoch.AddSeconds(left).ToShortDateString();
            _begin = epoch.AddSeconds(left).ToLongTimeString();
            _end = epoch.AddSeconds(right).ToLongTimeString();
        }

        public string Category
        {
            get => _category;
            set => RaiseAndSetIfChanged(ref _category, value);
        }

        public string Date
        {
            get => _date;
            set => RaiseAndSetIfChanged(ref _date, value);
        }

        public string Begin
        {
            get => _begin;
            set => RaiseAndSetIfChanged(ref _begin, value);
        }

        public string End
        {
            get => _end;
            set => RaiseAndSetIfChanged(ref _end, value);
        }
    }
}
