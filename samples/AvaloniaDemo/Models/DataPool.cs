using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Models
{
    public record TimeInterval(double Begin, double End);

    public enum ETimePeriod { Day, Week, Month, Year }

    public class DataPool
    {    
        private readonly Dictionary<ETimePeriod, Tuple<int, int>> _minSizeIntervals = new()
        {
            { ETimePeriod.Day, Tuple.Create(30 * 60/*30min*/, 9 * 60 * 60/*9h*/) },
            { ETimePeriod.Week, Tuple.Create(2 * 60 * 60/*2h*/, 10 * 60 * 60/*10h*/) },
            { ETimePeriod.Month, Tuple.Create(6 * 60 * 60/*6h*/, 20 * 60 * 60/*20h*/) },
            { ETimePeriod.Year, Tuple.Create(12 * 60 * 60/*12h*/, 7 * 24 * 60 * 60/*7day*/) },
        };
        private readonly Dictionary<ETimePeriod, Tuple<int, int>> _maxSizeIntervals = new()
        {
            { ETimePeriod.Day, Tuple.Create(2 * 60 * 60/*2h*/, 12 * 60 * 60/*12h*/) },
            { ETimePeriod.Week, Tuple.Create(6 * 60 * 60/*6h*/, 12 * 60 * 60/*12h*/) },
            { ETimePeriod.Month, Tuple.Create(12 * 60 * 60/*12h*/, 24 * 60 * 60/*24h*/) },
            { ETimePeriod.Year, Tuple.Create(24 * 60 * 60/*24h*/, 20 * 24 * 60 * 60/*20day*/) },
        };
        private readonly Dictionary<ETimePeriod, Tuple<int, int>> _numIntervals = new()
        {
            { ETimePeriod.Day, Tuple.Create(10, 2) },
            { ETimePeriod.Week, Tuple.Create(25, 10) },
            { ETimePeriod.Month, Tuple.Create(60, 25) },
            { ETimePeriod.Year, Tuple.Create(200, 25) },
        };
        private ETimePeriod _timePeriod;
        private int _begin, _end;
        private int _lastValue;       
        private readonly Dictionary<int, bool> _isFull = new();
        private readonly Random _random = new();
        private readonly int _specialDelta = 0;// для тестирования отступа от MinScreenValue=0
        private DateTime _epoch;

        public DataPool()
        {
            TimePeriod = ETimePeriod.Day;

            Intervals = new Dictionary<int, IList<TimeInterval>>();
            BackIntervals = new List<TimeInterval>();

            NumSatellites = 5;
        }

        public IDictionary<int, IList<TimeInterval>> Intervals { get; }

        public IList<TimeInterval> BackIntervals { get; }

        public int NumSatellites { get; set; }

        public ETimePeriod TimePeriod
        {
            get
            {
                return _timePeriod;
            }
            set
            {
                _timePeriod = value;

                _begin = 0;

                switch (_timePeriod)
                {
                    case ETimePeriod.Day:
                        _end = 24 * 60 * 60;
                        break;
                    case ETimePeriod.Week:
                        _end = 7 * 24 * 60 * 60;
                        break;
                    case ETimePeriod.Month:
                        _end = 30 * 24 * 60 * 60;
                        break;
                    case ETimePeriod.Year:
                        _end = 12 * 30 * 24 * 60 * 60;
                        break;
                    default:
                        break;
                }
            }
        }

        public DateTime Epoch => _epoch;

        private int AllSeconds => _end - _begin;

        public double MinValue
        {
            get
            {
                double min = double.MaxValue;
                foreach (var item in Intervals.Values)
                {                                
                    min = Math.Min(item.Min(s => s.Begin), min);                    
                }

                return min;
            }
        }

        public double MaxValue
        {
            get
            {
                double max = double.MinValue;
                foreach (var item in Intervals.Values)
                {
                    max = Math.Max(item.Max(s => s.End), max);
                }

                return max;
            }
        }

        public void GenerateIntervals()
        {
            GenerateEpoch();

            Intervals.Clear();
            BackIntervals.Clear();

            for (int i = 0; i < NumSatellites; i++)
            {
                for (int j = 0; j < _numIntervals[TimePeriod].Item1; j++)
                {
                    GenerateIvals(i);
                }
            }

            GenerateBacks();
        }

        private void GenerateEpoch()
        {
            DateTime epoch = new DateTime(1990, 1, 1);
            
            epoch = epoch.AddYears(_random.Next(0, 20));

            epoch = epoch.AddMonths(_random.Next(0, 11));

            epoch = epoch.AddDays(_random.Next(0, 30));

            _epoch = epoch.AddSeconds(_random.Next(0, 86400));
        }

        private void GenerateIvals(int stringIndex)
        {
            if (_isFull.ContainsKey(stringIndex) == false)
            {
                _isFull.Add(stringIndex, false);
            }

            int MinValue = 0 + _specialDelta;
            int MaxValue = AllSeconds;

            while (_isFull[stringIndex] == false)
            {
                var value = _random.Next(MinValue, MaxValue);

                if (IsValid(stringIndex, value) == true)
                {
                    CreateInterval(stringIndex, value, MinValue, MaxValue, out double left, out double right);

                    Intervals[stringIndex].Add(new TimeInterval(left, right));

                    return;
                }
            }
        }

        private bool IsInner(TimeInterval ival, double value)
        {
            if (value <= ival.End && value >= ival.Begin)
                return true;

            return false;
        }

        private bool IsValid(int stringIndex, double value)
        {
            if (Intervals.ContainsKey(stringIndex) == true)
            {
                double temp = 0.0;

                foreach (var ival in Intervals[stringIndex])
                {
                    temp += (ival.End - ival.Begin);

                    if (IsInner(ival, value) == true)
                        return false;
                }

                if (temp >= AllSeconds)
                {
                    _isFull[stringIndex] = true;
                    return false;
                }

                return true;
            }

            Intervals.Add(stringIndex, new List<TimeInterval>());

            return true;
        }

        private void CreateInterval(int stringIndex, int value, int MinValue, int MaxValue, out double left, out double right)
        {
            // int left;
            int leftDir = _maxSizeIntervals[TimePeriod].Item1 / 2;

            do
            {
                leftDir = _random.Next(0, leftDir);

                left = value - leftDir;
                if (left < MinValue)
                    left = MinValue;

            } while (IsValid(stringIndex, left) == false);

            // int right;
            int rightDir = _maxSizeIntervals[TimePeriod].Item1 / 2;

            do
            {
                rightDir = _random.Next(0, rightDir);

                right = value + rightDir;
                if (right > MaxValue)
                    right = MaxValue;

            } while (IsValid(stringIndex, right) == false);


        }

        private void GenerateBacks()
        {
            _lastValue = 0 + _specialDelta;

            for (int i = 0; i < _numIntervals[TimePeriod].Item2; i++)
            {
                CreateBack(i);
            }
        }

        private void CreateBack(int index)
        {
            int max = _maxSizeIntervals[TimePeriod].Item2;

            int need = _numIntervals[TimePeriod].Item2 - (index + 1);

            // i = 0
            var pos = _random.Next(_lastValue, AllSeconds - need * max - _maxSizeIntervals[TimePeriod].Item2/*current*/);
            var len = _random.Next(_minSizeIntervals[TimePeriod].Item2, _maxSizeIntervals[TimePeriod].Item2);


            BackIntervals.Add(new TimeInterval(pos, pos + len));

            _lastValue = pos + len;
        }
    }
}
