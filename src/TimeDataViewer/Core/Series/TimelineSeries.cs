﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public class TimelineSeries : CategorizedSeries, IStackableSeries
    {
        public new const string DefaultTrackerFormatString = "{0}\n{1}: {2}\n{3}: {4}";

        public TimelineSeries()
        {
            Items = new List<TimelineItem>();
            this.TrackerFormatString = DefaultTrackerFormatString;
            BarWidth = 1;
        }

        public double BarWidth { get; set; }

        public bool IsStacked => true;

        public string StackGroup => string.Empty;

        public IList<TimelineItem> Items { get; private set; }

        public string CategoryField { get; set; }

        public string EndField { get; set; }

        public string BeginField { get; set; }

        protected internal IList<OxyRect> ActualBarRectangles { get; set; }

        protected internal IList<TimelineItem> ValidItems { get; set; }

        protected internal Dictionary<int, int> ValidItemsIndexInversion { get; set; }

        /// <summary>
        /// Gets the point in the dataset that is nearest the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="interpolate">The interpolate.</param>
        /// <returns>A TrackerHitResult for the current hit.</returns>
        public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
        {
            for (int i = 0; i < this.ActualBarRectangles.Count; i++)
            {
                var r = this.ActualBarRectangles[i];
                if (r.Contains(point))
                {
                    var item = (TimelineItem)this.GetItem(this.ValidItemsIndexInversion[i]);
                    var categoryIndex = item.GetCategoryIndex(i);
                    double value = (this.ValidItems[i].Begin + this.ValidItems[i].End) / 2;
                    var dp = new DataPoint(categoryIndex, value);
                    var categoryAxis = this.GetCategoryAxis();
                    var valueAxis = this.GetValueAxis();
                    return new TrackerHitResult
                    {
                        Series = this,
                        DataPoint = dp,
                        Position = point,
                        Item = item,
                        Index = i,
                        Text = StringHelper.Format(
                        System.Globalization.CultureInfo.CurrentCulture,//this.ActualCulture,
                        this.TrackerFormatString,
                        item,
                        "this.Title",//this.Title,
                        /*categoryAxis.Title ??*/ DefaultCategoryAxisTitle,
                        categoryAxis.FormatValue(categoryIndex),
                        /*valueAxis.Title ??*/ DefaultValueAxisTitle,
                        valueAxis.GetValue(this.Items[i].Begin),
                        valueAxis.GetValue(this.Items[i].End),
                        "this.Items[i].Title")//this.Items[i].Title)
                    };
                }
            }

            return null;
        }

        // Checks if the specified value is valid.
        public virtual bool IsValidPoint(double v, Axis yaxis)
        {
            return !double.IsNaN(v) && !double.IsInfinity(v);
        }

        public override void Render()
        {
            ActualBarRectangles = new List<OxyRect>();

            if (ValidItems.Count == 0)
            {
                return;
            }

            var clippingRect = GetClippingRect();
            var categoryAxis = GetCategoryAxis();

            var actualBarWidth = GetActualBarWidth();
            var stackIndex = categoryAxis.GetStackIndex(StackGroup);

            _rectList.Clear();
            _clippingRect = clippingRect;

            for (var i = 0; i < ValidItems.Count; i++)
            {
                var item = ValidItems[i];

                var categoryIndex = item.GetCategoryIndex(i);

                double categoryValue = categoryAxis.GetCategoryValue(categoryIndex, stackIndex, actualBarWidth);

                var p0 = Transform(item.Begin, categoryValue);
                var p1 = Transform(item.End, categoryValue + actualBarWidth);
                
                var rectangle = OxyRect.Create(p0.X, p0.Y, p1.X, p1.Y);

                ActualBarRectangles.Add(rectangle);

                _rectList.Add(rectangle);

                //rc.DrawClippedRectangleAsPolygon(
                //    clippingRect,
                //    rectangle,
                //    item.Color.GetActualColor(ActualFillColor),
                //    StrokeColor);
            }
        }
        OxyRect _clippingRect;
        List<OxyRect> _rectList = new List<OxyRect>();

        public OxyRect MyClippingRect => _clippingRect;
        public List<OxyRect> MyRectList => _rectList;

        public override void MyOnRender()
        {
            MyRender?.Invoke(this, EventArgs.Empty);
        }

        // Gets or sets the width/height of the columns/bars (as a fraction of the available space).
        internal override double GetBarWidth()
        {
            return BarWidth;
        }

        // Gets the items of this series.
        protected internal override IList<CategorizedItem> GetItems()
        {
            return Items.Cast<CategorizedItem>().ToList();
        }

        // Check if the data series is using the specified axis.
        protected internal override bool IsUsing(Axis axis)
        {
            return XAxis == axis || YAxis == axis;
        }


        // Updates the axis maximum and minimum values.     
        protected internal override void UpdateAxisMaxMin()
        {
            XAxis.Include(MinX);
            XAxis.Include(MaxX);
        }

        protected internal override void UpdateData()
        {
            if (ItemsSource != null)
            {
                var list = new List<TimelineItem>();

                Items.Clear();

                var categoryAxis = GetCategoryAxis();
        
                if (string.IsNullOrWhiteSpace(BeginField) == false &&
                    string.IsNullOrWhiteSpace(EndField) == false &&
                    string.IsNullOrWhiteSpace(CategoryField) == false)
                {
                    foreach (var item in ItemsSource)
                    {
                        var propertyInfoLeft = item.GetType().GetProperty(BeginField);
                        var propertyInfoRight = item.GetType().GetProperty(EndField);
                        var propertyInfoCategory = item.GetType().GetProperty(CategoryField);

                        var left = propertyInfoLeft?.GetValue(item, null);
                        var right = propertyInfoRight?.GetValue(item, null);
                        var valueCategory = propertyInfoCategory?.GetValue(item, null);

                        if (left is not null &&
                            right is not null &&                                               
                            valueCategory is string category)
                        {                                              
                            list.Add(new TimelineItem()
                            {
                                Begin = Axis.ToDouble(left),
                                End = Axis.ToDouble(right),
                                CategoryIndex = categoryAxis.ActualLabels.IndexOf(category)
                            });
                        }
                    }
                }
            
                Items = new List<TimelineItem>(list);
            }
        }

        // Updates the maximum and minimum values of the series.  
        protected internal override void UpdateMaxMin()
        {
            base.UpdateMaxMin();

            if (ValidItems == null || ValidItems.Count == 0)
            {
                return;
            }

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            foreach (var item in ValidItems)
            {
                minValue = Math.Min(minValue, item.Begin);
                minValue = Math.Min(minValue, item.End);
                maxValue = Math.Max(maxValue, item.Begin);
                maxValue = Math.Max(maxValue, item.End);
            }

            MinX = minValue;
            MaxX = maxValue;
        }

        protected internal override void UpdateValidData()
        {
            ValidItems = new List<TimelineItem>();
            ValidItemsIndexInversion = new Dictionary<int, int>();
            var valueAxis = GetValueAxis();

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (valueAxis.IsValidValue(item.Begin) && valueAxis.IsValidValue(item.End))
                {
                    ValidItemsIndexInversion.Add(ValidItems.Count, i);
                    ValidItems.Add(item);
                }
            }
        }

        // Gets the actual width/height of the items of this series.
        public override double GetActualBarWidth()
        {
            var categoryAxis = GetCategoryAxis();
            return BarWidth / (1 + categoryAxis.GapWidth) / categoryAxis.GetMaxWidth();
        }

        public override CategoryAxis GetCategoryAxis()
        {
            var categoryAxis = YAxis as CategoryAxis;
            if (categoryAxis == null)
            {
                throw new InvalidOperationException("No category axis defined.");
            }

            return categoryAxis;
        }

        // Gets the item at the specified index.
        protected override object GetItem(int i)
        {
            if (ItemsSource != null || Items == null || Items.Count == 0)
            {
                return base.GetItem(i);
            }

            return Items[i];
        }

        private Axis GetValueAxis()
        {
            return XAxis;
        }
    }
}
