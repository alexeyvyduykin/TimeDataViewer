using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;

namespace TimeDataViewer.Core
{
    public abstract class Series
    {
        public IEnumerable ItemsSource { get; set; }


    }
}
