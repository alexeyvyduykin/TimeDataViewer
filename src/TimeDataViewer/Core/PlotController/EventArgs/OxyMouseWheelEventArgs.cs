using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class OxyMouseWheelEventArgs : OxyMouseEventArgs
    {
        // Gets or sets the change.      
        public int Delta { get; set; }
    }
}
