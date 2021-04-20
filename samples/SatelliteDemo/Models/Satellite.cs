using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteDemo.Models
{
    public class Satellite
    {
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<Rotation> Rotations { get; set; } = new ObservableCollection<Rotation>();

        public ObservableCollection<Observation> Observations { get; set; } = new ObservableCollection<Observation>();

        public ObservableCollection<Transmission> Transmissions { get; set; } = new ObservableCollection<Transmission>();
    }
}
