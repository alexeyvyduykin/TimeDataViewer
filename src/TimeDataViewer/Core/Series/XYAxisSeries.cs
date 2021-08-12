﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public abstract class XYAxisSeries : ItemsSeries
    {
        protected XYAxisSeries()
        {
        }
      
        // Gets or sets the maximum x-coordinate of the dataset.
        public double MaxX { get; protected set; }

        // Gets or sets the maximum y-coordinate of the dataset.
        public double MaxY { get; protected set; }

        // Gets or sets the minimum x-coordinate of the dataset.
        public double MinX { get; protected set; }
  
        // Gets or sets the minimum y-coordinate of the dataset.
        public double MinY { get; protected set; }

        public Axis XAxis { get; private set; }

        public string XAxisKey { get; set; }

        public Axis YAxis { get; private set; }

        public string YAxisKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the X coordinate of all data point increases monotonically.
        /// </summary>
        public bool IsXMonotonic { get; set; }

        /// <summary>
        /// Gets or sets the last visible window start position in the data points collection.
        /// </summary>
        public int WindowStartIndex { get; set; }
      
        // Gets the rectangle the series uses on the screen (screen coordinates).
        public OxyRect GetScreenRectangle()
        {
            return this.GetClippingRect();
        }

        /// <summary>
        /// Transforms from a screen point to a data point by the axes of this series.
        /// </summary>
        /// <param name="p">The screen point.</param>
        /// <returns>A data point.</returns>
        public DataPoint InverseTransform(ScreenPoint p)
        {
            return this.XAxis.InverseTransform(p.X, p.Y, this.YAxis);
        }

        /// <summary>
        /// Transforms the specified coordinates to a screen point by the axes of this series.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>A screen point.</returns>
        public ScreenPoint Transform(double x, double y)
        {
            return this.XAxis.Transform(x, y, this.YAxis);
        }

        /// <summary>
        /// Transforms the specified data point to a screen point by the axes of this series.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>A screen point.</returns>
        public ScreenPoint Transform(DataPoint p)
        {
            return this.XAxis.Transform(p.X, p.Y, this.YAxis);
        }

        /// <summary>
        /// Check if this data series requires X/Y axes. (e.g. Pie series do not require axes)
        /// </summary>
        /// <returns>The are axes required.</returns>
        protected internal override bool AreAxesRequired()
        {
            return true;
        }

        /// <summary>
        /// Ensures that the axes of the series is defined.
        /// </summary>
        protected internal override void EnsureAxes()
        {
            this.XAxis = this.XAxisKey != null ?
                         this.PlotModel.GetAxis(this.XAxisKey) :
                         this.PlotModel.DefaultXAxis;

            this.YAxis = this.YAxisKey != null ?
                         this.PlotModel.GetAxis(this.YAxisKey) :
                         this.PlotModel.DefaultYAxis;
        }

        /// <summary>
        /// Check if the data series is using the specified axis.
        /// </summary>
        /// <param name="axis">An axis.</param>
        /// <returns>True if the axis is in use.</returns>
        protected internal override bool IsUsing(Axis axis)
        {
            return false;
        }

        /// <summary>
        /// Sets default values from the plot model.
        /// </summary>
        protected internal override void SetDefaultValues()
        {
        }

        /// <summary>
        /// Updates the axes to include the max and min of this series.
        /// </summary>
        protected internal override void UpdateAxisMaxMin()
        {
            this.XAxis.Include(this.MinX);
            this.XAxis.Include(this.MaxX);
            this.YAxis.Include(this.MinY);
            this.YAxis.Include(this.MaxY);
        }

        /// <summary>
        /// Updates the data.
        /// </summary>
        protected internal override void UpdateData()
        {
            this.WindowStartIndex = 0;
        }

        /// <summary>
        /// Updates the maximum and minimum values of the series.
        /// </summary>
        protected internal override void UpdateMaxMin()
        {
            this.MinX = this.MinY = this.MaxX = this.MaxY = double.NaN;
        }

        /// <summary>
        /// Gets the clipping rectangle.
        /// </summary>
        /// <returns>The clipping rectangle.</returns>
        public OxyRect GetClippingRect()
        {
            double minX = Math.Min(XAxis.ScreenMin.X, XAxis.ScreenMax.X);
            double minY = Math.Min(YAxis.ScreenMin.Y, YAxis.ScreenMax.Y);
            double maxX = Math.Max(XAxis.ScreenMin.X, XAxis.ScreenMax.X);
            double maxY = Math.Max(YAxis.ScreenMin.Y, YAxis.ScreenMax.Y);

            return new OxyRect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Determines whether the specified point is valid.
        /// </summary>
        /// <param name="pt">The point.</param>
        /// <returns><c>true</c> if the point is valid; otherwise, <c>false</c> .</returns>
        protected virtual bool IsValidPoint(DataPoint pt)
        {
            return
                this.XAxis != null && this.XAxis.IsValidValue(pt.X) &&
                this.YAxis != null && this.YAxis.IsValidValue(pt.Y);
        }

        /// <summary>
        /// Determines whether the specified point is valid.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns><c>true</c> if the point is valid; otherwise, <c>false</c> . </returns>
        protected bool IsValidPoint(double x, double y)
        {
            return
                this.XAxis != null && this.XAxis.IsValidValue(x) &&
                this.YAxis != null && this.YAxis.IsValidValue(y);
        }

        /// <summary>
        /// Updates the Max/Min limits from the specified <see cref="DataPoint" /> list.
        /// </summary>
        /// <param name="points">The list of points.</param>
        protected void InternalUpdateMaxMin(List<DataPoint> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            this.IsXMonotonic = true;

            if (points.Count == 0)
            {
                return;
            }

            double minx = this.MinX;
            double miny = this.MinY;
            double maxx = this.MaxX;
            double maxy = this.MaxY;

            if (double.IsNaN(minx))
            {
                minx = double.MaxValue;
            }

            if (double.IsNaN(miny))
            {
                miny = double.MaxValue;
            }

            if (double.IsNaN(maxx))
            {
                maxx = double.MinValue;
            }

            if (double.IsNaN(maxy))
            {
                maxy = double.MinValue;
            }

            double lastX = double.MinValue;
            foreach (var pt in points)
            {
                double x = pt.X;
                double y = pt.Y;

                // Check if the point is valid
                if (!this.IsValidPoint(pt))
                {
                    continue;
                }

                if (x < lastX)
                {
                    this.IsXMonotonic = false;
                }

                if (x < minx)
                {
                    minx = x;
                }

                if (x > maxx)
                {
                    maxx = x;
                }

                if (y < miny)
                {
                    miny = y;
                }

                if (y > maxy)
                {
                    maxy = y;
                }

                lastX = x;
            }

            if (minx < double.MaxValue)
            {
                //if (minx < this.XAxis.FilterMinValue)
                //{
                //    minx = this.XAxis.FilterMinValue;
                //}

                this.MinX = minx;
            }

            if (miny < double.MaxValue)
            {
                //if (miny < this.YAxis.FilterMinValue)
                //{
                //    miny = this.YAxis.FilterMinValue;
                //}

                this.MinY = miny;
            }

            if (maxx > double.MinValue)
            {
                //if (maxx > this.XAxis.FilterMaxValue)
                //{
                //    maxx = this.XAxis.FilterMaxValue;
                //}

                this.MaxX = maxx;
            }

            if (maxy > double.MinValue)
            {
                //if (maxy > this.YAxis.FilterMaxValue)
                //{
                //    maxy = this.YAxis.FilterMaxValue;
                //}

                this.MaxY = maxy;
            }
        }

        /// <summary>
        /// Updates the Max/Min limits from the specified list.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="xf">A function that provides the x value for each item.</param>
        /// <param name="yf">A function that provides the y value for each item.</param>
        /// <exception cref="System.ArgumentNullException">The items argument cannot be null.</exception>
        protected void InternalUpdateMaxMin<T>(List<T> items, Func<T, double> xf, Func<T, double> yf)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.IsXMonotonic = true;

            if (items.Count == 0)
            {
                return;
            }

            double minx = this.MinX;
            double miny = this.MinY;
            double maxx = this.MaxX;
            double maxy = this.MaxY;

            if (double.IsNaN(minx))
            {
                minx = double.MaxValue;
            }

            if (double.IsNaN(miny))
            {
                miny = double.MaxValue;
            }

            if (double.IsNaN(maxx))
            {
                maxx = double.MinValue;
            }

            if (double.IsNaN(maxy))
            {
                maxy = double.MinValue;
            }

            double lastX = double.MinValue;
            foreach (var item in items)
            {
                double x = xf(item);
                double y = yf(item);

                // Check if the point is valid
                if (!this.IsValidPoint(x, y))
                {
                    continue;
                }

                if (x < lastX)
                {
                    this.IsXMonotonic = false;
                }

                if (x < minx)
                {
                    minx = x;
                }

                if (x > maxx)
                {
                    maxx = x;
                }

                if (y < miny)
                {
                    miny = y;
                }

                if (y > maxy)
                {
                    maxy = y;
                }

                lastX = x;
            }

            if (minx < double.MaxValue)
            {
                this.MinX = minx;
            }

            if (miny < double.MaxValue)
            {
                this.MinY = miny;
            }

            if (maxx > double.MinValue)
            {
                this.MaxX = maxx;
            }

            if (maxy > double.MinValue)
            {
                this.MaxY = maxy;
            }
        }

        /// <summary>
        /// Updates the Max/Min limits from the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="xmin">A function that provides the x minimum for each item.</param>
        /// <param name="xmax">A function that provides the x maximum for each item.</param>
        /// <param name="ymin">A function that provides the y minimum for each item.</param>
        /// <param name="ymax">A function that provides the y maximum for each item.</param>
        /// <exception cref="System.ArgumentNullException">The items argument cannot be null.</exception>
        protected void InternalUpdateMaxMin<T>(List<T> items, Func<T, double> xmin, Func<T, double> xmax, Func<T, double> ymin, Func<T, double> ymax)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.IsXMonotonic = true;

            if (items.Count == 0)
            {
                return;
            }

            double minx = this.MinX;
            double miny = this.MinY;
            double maxx = this.MaxX;
            double maxy = this.MaxY;

            if (double.IsNaN(minx))
            {
                minx = double.MaxValue;
            }

            if (double.IsNaN(miny))
            {
                miny = double.MaxValue;
            }

            if (double.IsNaN(maxx))
            {
                maxx = double.MinValue;
            }

            if (double.IsNaN(maxy))
            {
                maxy = double.MinValue;
            }

            double lastX0 = double.MinValue;
            double lastX1 = double.MinValue;
            foreach (var item in items)
            {
                double x0 = xmin(item);
                double x1 = xmax(item);
                double y0 = ymin(item);
                double y1 = ymax(item);

                if (!this.IsValidPoint(x0, y0) || !this.IsValidPoint(x1, y1))
                {
                    continue;
                }

                if (x0 < lastX0 || x1 < lastX1)
                {
                    this.IsXMonotonic = false;
                }

                if (x0 < minx)
                {
                    minx = x0;
                }

                if (x1 > maxx)
                {
                    maxx = x1;
                }

                if (y0 < miny)
                {
                    miny = y0;
                }

                if (y1 > maxy)
                {
                    maxy = y1;
                }

                lastX0 = x0;
                lastX1 = x1;
            }

            if (minx < double.MaxValue)
            {
                this.MinX = minx;
            }

            if (miny < double.MaxValue)
            {
                this.MinY = miny;
            }

            if (maxx > double.MinValue)
            {
                this.MaxX = maxx;
            }

            if (maxy > double.MinValue)
            {
                this.MaxY = maxy;
            }
        }

        /// <summary>
        /// Verifies that both axes are defined.
        /// </summary>
        protected void VerifyAxes()
        {
            if (this.XAxis == null)
            {
                throw new InvalidOperationException("XAxis not defined.");
            }

            if (this.YAxis == null)
            {
                throw new InvalidOperationException("YAxis not defined.");
            }
        }

        /// <summary>
        /// Updates visible window start index.
        /// </summary>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="items">Data points.</param>
        /// <param name="xgetter">Function that gets data point X coordinate.</param>
        /// <param name="targetX">X coordinate of visible window start.</param>
        /// <param name="lastIndex">Last window index.</param>
        /// <returns>The new window start index.</returns>
        public int UpdateWindowStartIndex<T>(IList<T> items, Func<T, double> xgetter, double targetX, int lastIndex)
        {
            lastIndex = this.FindWindowStartIndex(items, xgetter, targetX, lastIndex);
            if (lastIndex > 0)
            {
                lastIndex--;
            }

            return lastIndex;
        }

        /// <summary>
        /// Finds the index of max(x) &lt;= target x in a list of data points
        /// </summary>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="items">vector of data points</param>
        /// <param name="xgetter">Function that gets data point X coordinate.</param>
        /// <param name="targetX">target x.</param>
        /// <param name="initialGuess">initial guess index.</param>
        /// <returns>
        /// index of x with max(x) &lt;= target x or -1 if cannot find
        /// </returns>
        protected int FindWindowStartIndex<T>(IList<T> items, Func<T, double> xgetter, double targetX, int initialGuess)
        {
            int lastguess = 0;
            int start = 0;
            int end = items.Count - 1;
            int curGuess = initialGuess;

            while (start <= end)
            {
                if (curGuess < start)
                {
                    return lastguess;
                }
                else if (curGuess > end)
                {
                    return end;
                }

                double guessX = xgetter(items[curGuess]);
                if (guessX.Equals(targetX))
                {
                    return curGuess;
                }
                else if (guessX > targetX)
                {
                    end = curGuess - 1;
                    if (end < start)
                    {
                        return lastguess;
                    }
                    else if (end == start)
                    {
                        return end;
                    }
                }
                else
                {
                    start = curGuess + 1;
                    lastguess = curGuess;
                }

                if (start >= end)
                {
                    return lastguess;
                }

                double endX = xgetter(items[end]);
                double startX = xgetter(items[start]);

                var m = (end - start + 1) / (endX - startX);
                curGuess = start + (int)((targetX - startX) * m);
            }

            return lastguess;
        }
    }
}
