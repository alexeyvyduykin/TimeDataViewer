using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Models
{
    public class RealDataPool
    {
        public int NumSatellites { get; set; } = 5;

        ETimePeriod _timePeriod;
        public ETimePeriod TimePeriod
        {
            get
            {
                return _timePeriod;
            }
            set
            {
                _timePeriod = value;

                Epoch0 = DateTime.UtcNow;

                Begin__ = 0;

                switch (_timePeriod)
                {
                    case ETimePeriod.Day:
                        End__ = 24 * 60 * 60;
                        break;
                    case ETimePeriod.Week:
                        End__ = 7 * 24 * 60 * 60;
                        break;
                    case ETimePeriod.Month:
                        End__ = 30 * 24 * 60 * 60;
                        break;
                    case ETimePeriod.Year:
                        End__ = 12 * 30 * 24 * 60 * 60;
                        break;
                    default:
                        break;
                }
            }
        }

        public enum ETimePeriod
        {
            Day,
            Week,
            Month,
            Year
        }

        // для тестирования отступа от MinScreenValue=0
        int SpecialDelta { get; set; } = 0;//= 3 * 86400;

        DateTime Epoch0 { get; set; }

        int Begin__, End__; // secs
        //DateTime Begin, End;

        int AllSeconds
        {
            get
            {
                return End__ - Begin__;
            }
        }

        public RealDataPool()
        {
            TimePeriod = ETimePeriod.Day;

            Intervals = new Dictionary<int, IList<Interval>>();
            BackIntervals = new List<Interval>();
        }

        Dictionary<ETimePeriod, Tuple<int, int>> MinSizeIntervals = new Dictionary<ETimePeriod, Tuple<int, int>>()
        {
            { ETimePeriod.Day, Tuple.Create(30 * 60/*30min*/, 9 * 60 * 60/*9h*/) },
            { ETimePeriod.Week, Tuple.Create(2 * 60 * 60/*2h*/, 10 * 60 * 60/*10h*/) },
            { ETimePeriod.Month, Tuple.Create(6 * 60 * 60/*6h*/, 20 * 60 * 60/*20h*/) },
            { ETimePeriod.Year, Tuple.Create(12 * 60 * 60/*12h*/, 7 * 24 * 60 * 60/*7day*/) },
        };

        Dictionary<ETimePeriod, Tuple<int, int>> MaxSizeIntervals = new Dictionary<ETimePeriod, Tuple<int, int>>()
        {
            { ETimePeriod.Day, Tuple.Create(2 * 60 * 60/*2h*/, 12 * 60 * 60/*12h*/) },
            { ETimePeriod.Week, Tuple.Create(6 * 60 * 60/*6h*/, 12 * 60 * 60/*12h*/) },
            { ETimePeriod.Month, Tuple.Create(12 * 60 * 60/*12h*/, 24 * 60 * 60/*24h*/) },
            { ETimePeriod.Year, Tuple.Create(24 * 60 * 60/*24h*/, 20 * 24 * 60 * 60/*20day*/) },
        };

        Dictionary<ETimePeriod, Tuple<int, int>> NumIntervals = new Dictionary<ETimePeriod, Tuple<int, int>>()
        {
            { ETimePeriod.Day, Tuple.Create(10, 2) },
            { ETimePeriod.Week, Tuple.Create(25, 10) },
            { ETimePeriod.Month, Tuple.Create(60, 25) },
            { ETimePeriod.Year, Tuple.Create(200, 25) },
        };


        public void GenerateIntervals()
        {
            Intervals.Clear();
            BackIntervals.Clear();

            for (int i = 0; i < NumSatellites; i++)
            {
                for (int j = 0; j < NumIntervals[TimePeriod].Item1; j++)
                {
                    GenerateIvals(i);
                }
            }

            GenerateBacks();
        }

        void GenerateIvals(int stringIndex)
        {
            if (IsFull.ContainsKey(stringIndex) == false)
                IsFull.Add(stringIndex, false);

            int MinValue = 0 + SpecialDelta;
            int MaxValue = AllSeconds;

            while (IsFull[stringIndex] == false)
            {
                var value = random.Next(MinValue, MaxValue);

                if (IsValid(stringIndex, value) == true)
                {
                    CreateInterval(stringIndex, value, MinValue, MaxValue, out double left, out double right);

                    Intervals[stringIndex].Add(new Interval(left, right));

                    return;
                }
            }
        }

        bool IsInner(Interval ival, double value)
        {
            if (value <= ival.Right && value >= ival.Left)
                return true;

            return false;
        }

        bool IsValid(int stringIndex, double value)
        {
            if (Intervals.ContainsKey(stringIndex) == true)
            {
                double temp = 0.0;

                foreach (var ival in Intervals[stringIndex])
                {
                    temp += (ival.Right - ival.Left);

                    if (IsInner(ival, value) == true)
                        return false;
                }

                if (temp >= AllSeconds)
                {
                    IsFull[stringIndex] = true;
                    return false;
                }

                return true;
            }

            Intervals.Add(stringIndex, new List<Interval>());

            return true;
        }

        void CreateInterval(int stringIndex, int value, int MinValue, int MaxValue, out double left, out double right)
        {
            // int left;
            int leftDir = MaxSizeIntervals[TimePeriod].Item1 / 2;

            do
            {
                leftDir = random.Next(0, leftDir);

                left = value - leftDir;
                if (left < MinValue)
                    left = MinValue;

            } while (IsValid(stringIndex, left) == false);

            // int right;
            int rightDir = MaxSizeIntervals[TimePeriod].Item1 / 2;

            do
            {
                rightDir = random.Next(0, rightDir);

                right = value + rightDir;
                if (right > MaxValue)
                    right = MaxValue;

            } while (IsValid(stringIndex, right) == false);


        }

        private int LastValue;

        void GenerateBacks()
        {
            LastValue = 0 + SpecialDelta;

            for (int i = 0; i < NumIntervals[TimePeriod].Item2; i++)
            {
                CreateBack(i);
            }
        }

        void CreateBack(int index)
        {
            int max = MaxSizeIntervals[TimePeriod].Item2;

            int need = NumIntervals[TimePeriod].Item2 - (index + 1);

            // i = 0
            var pos = random.Next(LastValue, AllSeconds - need * max - MaxSizeIntervals[TimePeriod].Item2/*current*/);
            var len = random.Next(MinSizeIntervals[TimePeriod].Item2, MaxSizeIntervals[TimePeriod].Item2);


            BackIntervals.Add(new Interval(pos, pos + len));

            LastValue = pos + len;
        }


        IDictionary<int, bool> IsFull { get; set; } = new Dictionary<int, bool>();
        Random random = new Random();

        public IDictionary<int, IList<Interval>> Intervals { get; private set; }
        public IList<Interval> BackIntervals { get; private set; }


    }
}
