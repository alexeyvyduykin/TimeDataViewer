using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer
{
    public abstract class ItemsSeries : Series
    {
        protected override void SynchronizeProperties(Core.Series series)
        {
            base.SynchronizeProperties(series);
            var s = (Core.ItemsSeries)series;
            s.ItemsSource = Items;
        }
    }
}
