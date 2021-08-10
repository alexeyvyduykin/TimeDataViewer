using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public abstract class OxyInputEventArgs : EventArgs
    {
        // Gets or sets a value indicating whether the event was handled.      
        public bool Handled { get; set; }
    }
}
