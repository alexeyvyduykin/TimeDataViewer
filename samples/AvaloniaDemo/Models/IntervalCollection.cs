using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace AvaloniaDemo.Models
{
    //public class IntervalCollection : IDisposable
    //{
    //    static Random r = new Random();

    //    public IntervalCollection(string name)
    //    {
    //        this.Name = name;

    //        Intervals = new ObservableCollection<BaseInterval>();

    //        string[] descr = new string[]
    //        {
    //            "Satellites times vision",
    //            "Sunlight satellite subpoint",
    //            "Satellites angle rotation",
    //            "Satellite received",
    //            "Sensor daylight",
    //            "GroundStation work",
    //            "Satellite orbit correction"
    //        };

    //        var index = r.Next(0, descr.Length - 1);

    //        Description = descr[index];
    //    }

    //    public void AddInterval(BaseInterval ival)
    //    {
    //        ival.Collection = this;

    //        Intervals.Add(ival);
    //    }

    //    public void Dispose()
    //    {
    //        Intervals.Clear();
    //    }

    //    public ObservableCollection<BaseInterval> Intervals { get; }

    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //}
}
