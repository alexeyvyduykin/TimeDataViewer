using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public class LinearAxis : Axis
    {
        public LinearAxis() { }

        public override bool IsXyAxis()
        {
            return true;
        }
    }
}
