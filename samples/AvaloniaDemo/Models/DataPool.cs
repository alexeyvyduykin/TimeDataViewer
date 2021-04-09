using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Models
{
    public class DataPool
    {
        public static int MinValue { get; set; } = 0;
        public static int MaxValue { get; set; } = 86400;
        public static int MaxLength { get; set; } = 3600;

        static int NumIvalsPerString = 10;
        static int NumBacks = 3;

        IDictionary<int, bool> IsFull { get; set; } = new Dictionary<int, bool>();

        Random random = new Random();

        public DataPool()
        {
            Pool = new Dictionary<int, IList<Interval>>();
            Backs = new List<Interval>();
        }

        public void Generate(int numIval)
        {
            Pool.Clear();

            for (int i = 0; i < numIval; i++)
            {
                for (int j = 0; j < NumIvalsPerString; j++)
                {
                    GenerateIvals(i);
                }
            }
        }

        public static int MinBackLength { get; set; } = 3 * 3600;
        public static int MaxBackLength { get; set; } = 6 * 3600;

        private int LastValue = 0;

        public void GenerateBacks()
        {
            Backs.Clear();

            LastValue = MinValue;

            for (int i = 0; i < NumBacks; i++)
            {
                CreateBack(i);
            }
        }

        private void CreateBack(int index)
        {
            int max = MaxBackLength;

            int need = NumBacks - (index + 1);

            // i = 0
            var pos = random.Next(LastValue, MaxValue - need * max - MaxBackLength/*current*/);
            var len = random.Next(MinBackLength, MaxBackLength);


            Backs.Add(new Interval(pos, pos + len));

            LastValue = pos + len;
        }

        private void GenerateIvals(int stringIndex)
        {
            if (IsFull.ContainsKey(stringIndex) == false)
                IsFull.Add(stringIndex, false);

            while (IsFull[stringIndex] == false)
            {
                var value = random.Next(MinValue, MaxValue);

                if (IsValid(stringIndex, value) == true)
                {
                    CreateInterval(stringIndex, value, out double left, out double right);

                    Pool[stringIndex].Add(new Interval(left, right));

                    return;
                }
            }
        }

        private bool IsInner(Interval ival, double value)
        {
            if (value <= ival.Right && value >= ival.Left)
                return true;

            return false;
        }

        private bool IsValid(int stringIndex, double value)
        {
            if (Pool.ContainsKey(stringIndex) == true)
            {
                double temp = 0.0;

                foreach (var ival in Pool[stringIndex])
                {
                    temp += (ival.Right - ival.Left);

                    if (IsInner(ival, value) == true)
                        return false;
                }

                if (temp >= MaxValue - MinValue)
                {
                    IsFull[stringIndex] = true;
                    return false;
                }

                return true;
            }

            Pool.Add(stringIndex, new List<Interval>());

            return true;
        }

        private void CreateInterval(int stringIndex, int value, out double left, out double right)
        {
            // int left;
            int leftDir = MaxLength / 2;

            do
            {
                leftDir = random.Next(0, leftDir);

                left = value - leftDir;
                if (left < MinValue)
                    left = MinValue;

            } while (IsValid(stringIndex, left) == false);

            // int right;
            int rightDir = MaxLength / 2;

            do
            {
                rightDir = random.Next(0, rightDir);

                right = value + rightDir;
                if (right > MaxValue)
                    right = MaxValue;

            } while (IsValid(stringIndex, right) == false);


        }


        public IDictionary<int, IList<Interval>> Pool { get; private set; }
        public IList<Interval> Backs { get; private set; }
    }

}
