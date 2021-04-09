using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Models
{
    public class BaseInterval
    {
        public BaseInterval(double left, double right)
        {
            this.Left = left;
            this.Right = right;

            this.Name = string.Format("Interval_{0:HH:mm:ss}_{1:HH:mm:ss}", left, right);
            this.Id = Guid.NewGuid().ToString();
        }

        public IntervalCollection Collection { get; set; }

        public double Left { get; private set; }
        public double Right { get; private set; }

        public string Name { get; set; }
        public string Id { get; }
    }
}
