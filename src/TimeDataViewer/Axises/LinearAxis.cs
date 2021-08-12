using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer
{
    public class LinearAxis : Axis
    {
        public LinearAxis()
        {
            InternalAxis = new Core.LinearAxis();
        }

        public override Core.Axis CreateModel()
        {
            SynchronizeProperties();
            return InternalAxis;
        }
    }
}
