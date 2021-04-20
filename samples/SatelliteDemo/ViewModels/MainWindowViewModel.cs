using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SatelliteDemo.Models;
using System.Collections.ObjectModel;

namespace SatelliteDemo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private DateTime _epoch;
        private ObservableCollection<Satellite> _satellites;
        private Satellite _selected;

        public MainWindowViewModel()
        {          
            var path1 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite1.json");
            var path2 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite2.json");
            var path3 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite3.json");
            var path4 = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\Data", "satellite4.json");

            var json1 = GetJson(path1);
            var json2 = GetJson(path2);
            var json3 = GetJson(path3);
            var json4 = GetJson(path4);

            Serializer serializer = new();

            var sat1 = serializer.Deserialize<Satellite>(json1);
            var sat2 = serializer.Deserialize<Satellite>(json2);
            var sat3 = serializer.Deserialize<Satellite>(json3);
            var sat4 = serializer.Deserialize<Satellite>(json4);

            Satellites = new ObservableCollection<Satellite>(new(){ sat1, sat2, sat3, sat4 });

            Selected = sat1;

            Epoch = DateTime.Now;
        }

        public DateTime Epoch
        {
            get => _epoch;
            set => RaiseAndSetIfChanged(ref _epoch, value);
        }

        public ObservableCollection<Satellite> Satellites
        {
            get => _satellites;
            set => RaiseAndSetIfChanged(ref _satellites, value);
        }

        public Satellite Selected
        {
            get => _selected;
            set => RaiseAndSetIfChanged(ref _selected, value);
        }

        private string GetJson(string path)
        {
            using var fs = File.OpenRead(path);
            using var sr = new StreamReader(fs, Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }
}
