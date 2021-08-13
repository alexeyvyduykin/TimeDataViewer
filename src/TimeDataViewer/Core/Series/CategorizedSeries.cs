using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public abstract class CategorizedSeries : XYAxisSeries
    {
        internal abstract double GetBarWidth();

        protected internal abstract IList<CategorizedItem> GetItems();

        public abstract double GetActualBarWidth();

        public abstract CategoryAxis GetCategoryAxis();
    }
}
