using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;
using A = TimeDataViewer;

namespace TimeDataViewer.Core
{
    public class TimelineItem 
    {
        public TimelineItem()
        {
            CategoryIndex = -1;
        }

        public TimelineItem(double begin, double end) : this()
        {
            Begin = begin;
            End = end;
        }

        public double End { get; set; }

        public double Begin { get; set; }

        // Gets or sets the index of the category.
        public int CategoryIndex { get; set; }

        // Gets the index of the category.
        public int GetCategoryIndex(int defaultIndex)
        {
            if (CategoryIndex < 0)
            {
                return defaultIndex;
            }

            return CategoryIndex;
        }

        public A.Series? SeriesControl { get; set; }

        public Point2D LocalPosition { get; set; }

        //public int AbsolutePositionX { get; set; }

        //public int AbsolutePositionY { get; set; }

        public int ZIndex { get; set; }
    }
}
