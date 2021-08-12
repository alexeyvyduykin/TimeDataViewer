using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;
using TimeDataViewer.ViewModels;
using System.Diagnostics;

namespace TimeDataViewer.Core
{
    public enum AxisPosition
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }

    public enum AxisChangeTypes { Zoom, Pan, Reset }

    public record AxisLabelPosition
    {
        public string? Label { get; init; }
        public double Value { get; init; }
    }

    public abstract partial class Axis : PlotElement
    {
        protected static readonly Func<double, double> Exponent = x => Math.Floor(ThresholdRound(Math.Log(Math.Abs(x), 10)));
        protected static readonly Func<double, double> Mantissa = x => ThresholdRound(x / Math.Pow(10, Exponent(x)));     
        // Rounds a value if the difference between the rounded value and the original value is less than 1e-6.  
        protected static readonly Func<double, double> ThresholdRound = x => Math.Abs(Math.Round(x) - x) < 1e-6 ? Math.Round(x) : x;

        private double offset;
        private double _scale;
        private AxisPosition position;

        protected Axis()
        {
            Position = AxisPosition.Left;
            IsAxisVisible = true;

            ViewMaximum = double.NaN;
            ViewMinimum = double.NaN;

            AbsoluteMaximum = double.MaxValue;
            AbsoluteMinimum = double.MinValue;

            Minimum = double.NaN;
            Maximum = double.NaN;
            MinorStep = double.NaN;
            MajorStep = double.NaN;
            MinimumMinorStep = 0;
            MinimumMajorStep = 0;

            this.MinimumRange = 0;
            this.MaximumRange = double.PositiveInfinity;

            this.MinorTickSize = 4;
            this.MajorTickSize = 7;

            this.IsZoomEnabled = true;
            this.IsPanEnabled = true;

            this.IntervalLength = 60;

            this.AxisDistance = 0;
            this.AxisTickToLabelDistance = 4;

            this.DataMaximum = double.NaN;
            this.DataMinimum = double.NaN;
        }

        // Gets or sets the current view's maximum. This value is used when the user zooms or pans.
        protected double ViewMaximum { get; set; }

        // Gets or sets the current view's minimum. This value is used when the user zooms or pans.
        protected double ViewMinimum { get; set; }

        // Gets or sets the minimum value of the axis. The default value is <c>double.NaN</c>.  
        public double Minimum { get; set; }

        // Gets or sets the maximum value of the axis. The default value is <c>double.NaN</c>.                                                         
        public double Maximum { get; set; }

        // Gets or sets the absolute maximum. This is only used for the UI control.
        // It will not be possible to zoom/pan beyond this limit. The default value is <c>double.MaxValue</c>.    
        public double AbsoluteMaximum { get; set; }

        // Gets or sets the absolute minimum. This is only used for the UI control.
        // It will not be possible to zoom/pan beyond this limit. The default value is <c>double.MinValue</c>.     
        public double AbsoluteMinimum { get; set; }

        /// <summary>
        /// Occurs when the axis has been changed (by zooming, panning or resetting).
        /// </summary>
        public event EventHandler<AxisChangedEventArgs> AxisChanged;

        /// <summary>
        /// Occurs when the transform changed (size or axis range was changed).
        /// </summary>
        public event EventHandler TransformChanged;

        /// <summary>
        /// Gets or sets the actual major step.
        /// </summary>
        public double ActualMajorStep { get; protected set; }

        /// <summary>
        /// Gets or sets the actual maximum value of the axis.
        /// </summary>
        /// <remarks>If <see cref="ViewMaximum" /> is not <c>NaN</c>, this value will be defined by <see cref="ViewMaximum" />.
        /// Otherwise, if <see cref="Maximum" /> is not <c>NaN</c>, this value will be defined by <see cref="Maximum" />.
        /// Otherwise, this value will be defined by the maximum (+padding) of the data.</remarks>
        public double ActualMaximum { get; protected set; }

        // Gets or sets the actual minimum value of the axis.
        public double ActualMinimum { get; protected set; }

        /// <summary>
        /// Gets or sets the actual minor step.
        /// </summary>
        public double ActualMinorStep { get; protected set; }

        /// <summary>
        /// Gets or sets the actual string format being used.
        /// </summary>
        public string ActualStringFormat { get; protected set; }

        /// <summary>
        /// Gets or sets the distance from the end of the tick lines to the labels. The default value is <c>4</c>.
        /// </summary>
        public double AxisTickToLabelDistance { get; set; }

        /// <summary>
        /// Gets or sets the distance between the plot area and the axis. The default value is <c>0</c>.
        /// </summary>
        public double AxisDistance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to crop gridlines with perpendicular axes Start/EndPositions. The default value is <c>false</c>.
        /// </summary>
        public bool CropGridlines { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of the data displayed on this axis.
        /// </summary>
        public double DataMaximum { get; protected set; }

        /// <summary>
        /// Gets or sets the minimum value of the data displayed on this axis.
        /// </summary>
        public double DataMinimum { get; protected set; }

        /// <summary>
        /// Gets or sets the values for the extra gridlines. The default value is <c>null</c>.
        /// </summary>
        public double[] ExtraGridlines { get; set; }

        /// <summary>
        /// Gets or sets the maximum length (screen space) of the intervals. The available length of the axis will be divided by this length to get the approximate number of major intervals on the axis. The default value is <c>60</c>.
        /// </summary>
        public double IntervalLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this axis is visible. The default value is <c>true</c>.
        /// </summary>
        public bool IsAxisVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether panning is enabled. The default value is <c>true</c>.
        /// </summary>
        public bool IsPanEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether zooming is enabled. The default value is <c>true</c>.
        /// </summary>
        public bool IsZoomEnabled { get; set; }

        /// <summary>
        /// Gets or sets the key of the axis. This can be used to specify an axis if you have defined multiple axes in a plot. The default value is <c>null</c>.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the interval between major ticks. The default value is <c>double.NaN</c>.
        /// </summary>
        public double MajorStep { get; set; }

        /// <summary>
        /// Gets or sets the size of the major ticks. The default value is <c>7</c>.
        /// </summary>
        public double MajorTickSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum range of the axis. Setting this property ensures that <c>ActualMaximum-ActualMinimum &lt; MaximumRange</c>. The default value is <c>double.PositiveInfinity</c>.
        /// </summary>
        public double MaximumRange { get; set; }



        /// <summary>
        /// Gets or sets the minimum value for the interval between major ticks. The default value is <c>0</c>.
        /// </summary>
        public double MinimumMajorStep { get; set; }

        /// <summary>
        /// Gets or sets the minimum value for the interval between minor ticks. The default value is <c>0</c>.
        /// </summary>
        public double MinimumMinorStep { get; set; }

        /// <summary>
        /// Gets or sets the minimum range of the axis. Setting this property ensures that <c>ActualMaximum-ActualMinimum > MinimumRange</c>. The default value is <c>0</c>.
        /// </summary>
        public double MinimumRange { get; set; }

        /// <summary>
        /// Gets or sets the interval between minor ticks. The default value is <c>double.NaN</c>.
        /// </summary>
        public double MinorStep { get; set; }

        /// <summary>
        /// Gets or sets the size of the minor ticks. The default value is <c>4</c>.
        /// </summary>
        public double MinorTickSize { get; set; }

        /// <summary>
        /// Gets the offset. This is used to transform between data and screen coordinates.
        /// </summary>
        public double Offset => offset;

        /// <summary>
        /// Gets or sets the position of the axis. The default value is <see cref="AxisPosition.Left"/>.
        /// </summary>
        public AxisPosition Position
        {
            get
            {
                return this.position;
            }

            set
            {
                this.position = value;
            }
        }

        /// <summary>
        /// Gets the scaling factor of the axis. This is used to transform between data and screen coordinates.
        /// </summary>
        public double Scale => _scale;

        /// <summary>
        /// Gets or sets the screen coordinate of the maximum end of the axis.
        /// </summary>
        public ScreenPoint ScreenMax { get; protected set; }

        /// <summary>
        /// Gets or sets the screen coordinate of the minimum end of the axis.
        /// </summary>
        public ScreenPoint ScreenMin { get; protected set; }

        /// <summary>
        /// Gets or sets the string format used for formatting the axis values. The default value is <c>null</c>.
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// Gets or sets the "desired" size by the renderer such that the axis text &amp; ticks will not be clipped.  This
        /// size is distinct from the margin settings or the size which is actually rendered, as in: ActualWidth / ActualSize.  
        /// Actual rendered size may be smaller or larger than the desired size if the margins are set manually.
        /// </summary>
        public OxySize DesiredSize { get; protected set; }

        /// <summary>
        /// Converts the value of the specified object to a double precision floating point number. DateTime objects are converted using DateTimeAxis.ToDouble and TimeSpan objects are converted using TimeSpanAxis.ToDouble
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The floating point number value.</returns>
        public static double ToDouble(object value)
        {
            if (value is DateTime)
            {
                return DateTimeAxis.ToDouble((DateTime)value);
            }

            return Convert.ToDouble(value);
        }

        // Formats the value to be used on the axis.
        public string FormatValue(double x)
        {
            return FormatValueOverride(x);
        }

        // Gets the coordinates used to draw ticks and tick labels (numbers or category names).
        public virtual void GetTickValues(out IList<double> majorLabelValues, out IList<double> majorTickValues, out IList<double> minorTickValues)
        {
            minorTickValues = CreateTickValues(ActualMinimum, ActualMaximum, ActualMinorStep);
            majorTickValues = CreateTickValues(ActualMinimum, ActualMaximum, ActualMajorStep);
            majorLabelValues = majorTickValues;
        }

        /// <summary>
        /// Gets the value from an axis coordinate, converts from a coordinate <see cref="double" /> value to the actual data type.
        /// </summary>
        /// <param name="x">The coordinate.</param>
        /// <returns>The converted value.</returns>
        /// <remarks>Examples: The <see cref="DateTimeAxis" /> returns the <see cref="DateTime" /> and <see cref="CategoryAxis" /> returns category strings.</remarks>
        public virtual object GetValue(double x)
        {
            return x;
        }

        /// <summary>
        /// Inverse transform the specified screen point.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="yaxis">The y-axis.</param>
        /// <returns>The data point.</returns>
        public DataPoint InverseTransform(double x, double y, Axis yaxis)
        {
            return new DataPoint(InverseTransform(x), yaxis != null ? yaxis.InverseTransform(y) : 0);
        }

        /// <summary>
        /// Inverse transforms the specified screen coordinate. This method can only be used with non-polar coordinate systems.
        /// </summary>
        /// <param name="sx">The screen coordinate.</param>
        /// <returns>The value.</returns>
        public double InverseTransform(double sx)
        {
            return (sx / _scale) + offset;
        }

        /// <summary>
        /// Transforms the specified coordinate to screen coordinates. This method can only be used with non-polar coordinate systems.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>The transformed value (screen coordinate).</returns>
        public double Transform(double x)
        {
            return (x - offset) * _scale;
        }

        /// <summary>
        /// Determines whether the axis is horizontal.
        /// </summary>
        /// <returns><c>true</c> if the axis is horizontal; otherwise, <c>false</c> .</returns>
        public bool IsHorizontal()
        {
            return position == AxisPosition.Top || position == AxisPosition.Bottom;
        }

        /// <summary>
        /// Determines whether the specified value is valid.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the specified value is valid; otherwise, <c>false</c> .</returns>
        public bool IsValidValue(double value)
        {
#pragma warning disable 1718
            // ReSharper disable EqualExpressionComparison
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return value == value && value != 1.0 / 0.0 && value != -1.0 / 0.0;
            // ReSharper restore CompareOfFloatsByEqualityOperator
            // ReSharper restore EqualExpressionComparison
#pragma warning restore 1718
        }

        /// <summary>
        /// Determines whether the axis is vertical.
        /// </summary>
        /// <returns><c>true</c> if the axis is vertical; otherwise, <c>false</c> .</returns>
        public bool IsVertical()
        {
            return this.position == AxisPosition.Left || this.position == AxisPosition.Right;
        }

        /// <summary>
        /// Determines whether the axis is used for X/Y values.
        /// </summary>
        /// <returns><c>true</c> if it is an XY axis; otherwise, <c>false</c> .</returns>
        public abstract bool IsXyAxis();

        /// <summary>
        /// Pans the specified axis.
        /// </summary>
        /// <param name="ppt">The previous point (screen coordinates).</param>
        /// <param name="cpt">The current point (screen coordinates).</param>
        public virtual void Pan(ScreenPoint ppt, ScreenPoint cpt)
        {
            if (!this.IsPanEnabled)
            {
                return;
            }

            bool isHorizontal = this.IsHorizontal();

            double dsx = isHorizontal ? cpt.X - ppt.X : cpt.Y - ppt.Y;
            this.Pan(dsx);
        }

        public virtual void Pan(double delta)
        {
            if (!this.IsPanEnabled)
            {
                return;
            }

            var oldMinimum = this.ActualMinimum;
            var oldMaximum = this.ActualMaximum;

            double dx = delta / this.Scale;

            double newMinimum = this.ActualMinimum - dx;
            double newMaximum = this.ActualMaximum - dx;
            if (newMinimum < AbsoluteMinimum)
            {
                newMinimum = AbsoluteMinimum;
                newMaximum = Math.Min(newMinimum + this.ActualMaximum - this.ActualMinimum, AbsoluteMaximum);
            }

            if (newMaximum > AbsoluteMaximum)
            {
                newMaximum = AbsoluteMaximum;
                newMinimum = Math.Max(newMaximum - (this.ActualMaximum - this.ActualMinimum), AbsoluteMinimum);
            }

            ViewMinimum = newMinimum;
            ViewMaximum = newMaximum;
            UpdateActualMaxMin();

            var deltaMinimum = this.ActualMinimum - oldMinimum;
            var deltaMaximum = this.ActualMaximum - oldMaximum;

            this.OnAxisChanged(new AxisChangedEventArgs(AxisChangeTypes.Pan, deltaMinimum, deltaMaximum));
        }

        /// <summary>
        /// Resets the user's modification (zooming/panning) to minimum and maximum of this axis.
        /// </summary>
        public virtual void Reset()
        {
            var oldMinimum = this.ActualMinimum;
            var oldMaximum = this.ActualMaximum;

            ViewMinimum = double.NaN;
            ViewMaximum = double.NaN;
            UpdateActualMaxMin();

            var deltaMinimum = this.ActualMinimum - oldMinimum;
            var deltaMaximum = this.ActualMaximum - oldMaximum;

            this.OnAxisChanged(new AxisChangedEventArgs(AxisChangeTypes.Reset, deltaMinimum, deltaMaximum));
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture,
                // this.ActualCulture,
                "{0}({1}, {2}, {3}, {4})",
                this.GetType().Name,
                this.Position,
                this.ActualMinimum,
                this.ActualMaximum,
                this.ActualMajorStep);
        }

        /// <summary>
        /// Transforms the specified point to screen coordinates.
        /// </summary>
        /// <param name="x">The x value (for the current axis).</param>
        /// <param name="y">The y value.</param>
        /// <param name="yaxis">The y axis.</param>
        /// <returns>The transformed point.</returns>
        public ScreenPoint Transform(double x, double y, Axis yaxis)
        {
            if (yaxis == null)
            {
                throw new NullReferenceException("Y axis should not be null when transforming.");
            }

            return new ScreenPoint(this.Transform(x), yaxis.Transform(y));
        }

        public virtual void Zoom(double newScale)
        {
            var oldMinimum = this.ActualMinimum;
            var oldMaximum = this.ActualMaximum;

            double sx1 = this.Transform(this.ActualMaximum);
            double sx0 = this.Transform(this.ActualMinimum);

            double sgn = Math.Sign(_scale);
            double mid = (this.ActualMaximum + this.ActualMinimum) / 2;

            double dx = (this.offset - mid) * _scale;
            var newOffset = (dx / (sgn * newScale)) + mid;
            this.SetTransform(sgn * newScale, newOffset);

            double newMaximum = this.InverseTransform(sx1);
            double newMinimum = this.InverseTransform(sx0);

            if (newMinimum < AbsoluteMinimum && newMaximum > AbsoluteMaximum)
            {
                newMinimum = AbsoluteMinimum;
                newMaximum = AbsoluteMaximum;
            }
            else
            {
                if (newMinimum < this.AbsoluteMinimum)
                {
                    double d = newMaximum - newMinimum;
                    newMinimum = this.AbsoluteMinimum;
                    newMaximum = this.AbsoluteMinimum + d;
                    if (newMaximum > this.AbsoluteMaximum)
                    {
                        newMaximum = this.AbsoluteMaximum;
                    }
                }
                else if (newMaximum > this.AbsoluteMaximum)
                {
                    double d = newMaximum - newMinimum;
                    newMaximum = this.AbsoluteMaximum;
                    newMinimum = this.AbsoluteMaximum - d;
                    if (newMinimum < this.AbsoluteMinimum)
                    {
                        newMinimum = this.AbsoluteMinimum;
                    }
                }
            }

            ViewMaximum = newMaximum;
            ViewMinimum = newMinimum;
            UpdateActualMaxMin();

            var deltaMinimum = this.ActualMinimum - oldMinimum;
            var deltaMaximum = this.ActualMaximum - oldMaximum;

            this.OnAxisChanged(new AxisChangedEventArgs(AxisChangeTypes.Zoom, deltaMinimum, deltaMaximum));
        }

        // Zooms the axis to the range [x0,x1].
        public virtual void Zoom(double x0, double x1)
        {
            if (!this.IsZoomEnabled)
            {
                return;
            }

            var oldMinimum = this.ActualMinimum;
            var oldMaximum = this.ActualMaximum;

            double newMinimum = Math.Max(Math.Min(x0, x1), this.AbsoluteMinimum);
            double newMaximum = Math.Min(Math.Max(x0, x1), this.AbsoluteMaximum);

            ViewMinimum = newMinimum;
            ViewMaximum = newMaximum;
            UpdateActualMaxMin();

            var deltaMinimum = this.ActualMinimum - oldMinimum;
            var deltaMaximum = this.ActualMaximum - oldMaximum;

            this.OnAxisChanged(new AxisChangedEventArgs(AxisChangeTypes.Zoom, deltaMinimum, deltaMaximum));
        }

        public virtual void ZoomAt(double factor, double x)
        {
            if (!this.IsZoomEnabled)
            {
                return;
            }

            var oldMinimum = this.ActualMinimum;
            var oldMaximum = this.ActualMaximum;

            double dx0 = (this.ActualMinimum - x) * _scale;
            double dx1 = (this.ActualMaximum - x) * _scale;
            _scale *= factor;

            double newMinimum = (dx0 / _scale) + x;
            double newMaximum = (dx1 / _scale) + x;

            if (newMaximum - newMinimum > this.MaximumRange)
            {
                var mid = (newMinimum + newMaximum) * 0.5;
                newMaximum = mid + (this.MaximumRange * 0.5);
                newMinimum = mid - (this.MaximumRange * 0.5);
            }

            if (newMaximum - newMinimum < this.MinimumRange)
            {
                var mid = (newMinimum + newMaximum) * 0.5;
                newMaximum = mid + (this.MinimumRange * 0.5);
                newMinimum = mid - (this.MinimumRange * 0.5);
            }

            newMinimum = Math.Max(newMinimum, this.AbsoluteMinimum);
            newMaximum = Math.Min(newMaximum, this.AbsoluteMaximum);

            ViewMinimum = newMinimum;
            ViewMaximum = newMaximum;
            UpdateActualMaxMin();

            var deltaMinimum = this.ActualMinimum - oldMinimum;
            var deltaMaximum = this.ActualMaximum - oldMaximum;

            this.OnAxisChanged(new AxisChangedEventArgs(AxisChangeTypes.Zoom, deltaMinimum, deltaMaximum));
        }

        /// <summary>
        /// Zooms the axis with the specified zoom factor at the center of the axis.
        /// </summary>
        /// <param name="factor">The zoom factor.</param>
        public virtual void ZoomAtCenter(double factor)
        {
            double sx = (this.Transform(this.ActualMaximum) + this.Transform(this.ActualMinimum)) * 0.5;
            var x = this.InverseTransform(sx);
            this.ZoomAt(factor, x);
        }

        /// <summary>
        /// Modifies the data range of the axis [DataMinimum,DataMaximum] to includes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void Include(double value)
        {
            if (!this.IsValidValue(value))
            {
                return;
            }

            this.DataMinimum = double.IsNaN(this.DataMinimum) ? value : Math.Min(this.DataMinimum, value);
            this.DataMaximum = double.IsNaN(this.DataMaximum) ? value : Math.Max(this.DataMaximum, value);
        }

        /// <summary>
        /// Resets the <see cref="DataMaximum" /> and <see cref="DataMinimum" /> values.
        /// </summary>
        internal virtual void ResetDataMaxMin()
        {
            this.DataMaximum = this.DataMinimum = this.ActualMaximum = this.ActualMinimum = double.NaN;
        }

        /// <summary>
        /// Updates the <see cref="ActualMaximum" /> and <see cref="ActualMinimum" /> values.
        /// </summary>
        /// <remarks>If the user has zoomed/panned the axis, the internal ViewMaximum/ViewMinimum
        /// values will be used. If Maximum or Minimum have been set, these values will be used. Otherwise the maximum and minimum values
        /// of the series will be used, including the 'padding'.</remarks>
        internal virtual void UpdateActualMaxMin()
        {
            if (!double.IsNaN(ViewMaximum))
            {
                // The user has zoomed/panned the axis, use the ViewMaximum value.
                ActualMaximum = ViewMaximum;
            }
            else if (!double.IsNaN(this.Maximum))
            {
                // The Maximum value has been set
                this.ActualMaximum = this.Maximum;
            }
            else
            {
                // Calculate the actual maximum, including padding
                this.ActualMaximum = this.CalculateActualMaximum();
            }

            if (!double.IsNaN(this.ViewMinimum))
            {
                this.ActualMinimum = this.ViewMinimum;
            }
            else if (!double.IsNaN(this.Minimum))
            {
                this.ActualMinimum = this.Minimum;
            }
            else
            {
                this.ActualMinimum = this.CalculateActualMinimum();
            }

            this.CoerceActualMaxMin();
        }

        /// <summary>
        /// Updates the axis with information from the plot series.
        /// </summary>
        /// <param name="series">The series collection.</param>
        /// <remarks>This is used by the category axis that need to know the number of series using the axis.</remarks>
        internal virtual void UpdateFromSeries(Series[] series)
        {
        }

        /// <summary>
        /// Updates the actual minor and major step intervals.
        /// </summary>
        /// <param name="plotArea">The plot area rectangle.</param>
        internal virtual void UpdateIntervals(OxyRect plotArea)
        {
            double labelSize = this.IntervalLength;
            double length = this.IsHorizontal() ? plotArea.Width : plotArea.Height;

            this.ActualMajorStep = !double.IsNaN(this.MajorStep)
                                       ? this.MajorStep
                                       : this.CalculateActualInterval(length, labelSize);

            this.ActualMinorStep = !double.IsNaN(this.MinorStep)
                                       ? this.MinorStep
                                       : this.CalculateMinorInterval(this.ActualMajorStep);

            if (double.IsNaN(this.ActualMinorStep))
            {
                this.ActualMinorStep = 2;
            }

            if (double.IsNaN(this.ActualMajorStep))
            {
                this.ActualMajorStep = 10;
            }

            this.ActualMinorStep = Math.Max(this.ActualMinorStep, this.MinimumMinorStep);
            this.ActualMajorStep = Math.Max(this.ActualMajorStep, this.MinimumMajorStep);

            this.ActualStringFormat = this.StringFormat ?? this.GetDefaultStringFormat();
        }

        /// <summary>
        /// Updates the scale and offset properties of the transform from the specified boundary rectangle.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        internal virtual void UpdateTransform(OxyRect bounds)
        {
            double x0 = bounds.Left;
            double x1 = bounds.Right;
            double y0 = bounds.Bottom;
            double y1 = bounds.Top;

            ScreenMin = new ScreenPoint(x0, y1);
            ScreenMax = new ScreenPoint(x1, y0);

            double a0 = IsHorizontal() ? x0 : y0;
            double a1 = IsHorizontal() ? x1 : y1;

            ScreenMin = new ScreenPoint(a0, a1);
            ScreenMax = new ScreenPoint(a1, a0);

            if (this.ActualMaximum - this.ActualMinimum < double.Epsilon)
            {
                this.ActualMaximum = this.ActualMinimum + 1;
            }

            double max = this.PreTransform(this.ActualMaximum);
            double min = this.PreTransform(this.ActualMinimum);

            double da = a0 - a1;
            double newOffset, newScale;
            if (Math.Abs(da) > double.Epsilon)
            {
                newOffset = (a0 / da * max) - (a1 / da * min);
            }
            else
            {
                newOffset = 0;
            }

            double range = max - min;
            if (Math.Abs(range) > double.Epsilon)
            {
                newScale = (a1 - a0) / range;
            }
            else
            {
                newScale = 1;
            }

            this.SetTransform(newScale, newOffset);
        }

        /// <summary>
        /// Resets the current values.
        /// </summary>
        /// <remarks>The current values may be modified during update of max/min and rendering.</remarks>
        protected internal virtual void ResetCurrentValues()
        {
        }

        protected virtual string GetDefaultStringFormat()
        {
            return "g6";
        }

        // Applies a transformation after the inverse transform of the value.
        protected virtual double PostInverseTransform(double x)
        {
            return x;
        }

        // Applies a transformation before the transform the value.
        protected virtual double PreTransform(double x)
        {
            return x;
        }

        protected virtual double CalculateMinorInterval(double majorInterval)
        {
            return AxisUtilities.CalculateMinorInterval(majorInterval);
        }

        // Creates tick values at the specified interval.
        protected virtual IList<double> CreateTickValues(double from, double to, double step, int maxTicks = 1000)
        {
            return AxisUtilities.CreateTickValues(from, to, step, maxTicks);
        }

        /// <summary>
        /// Coerces the actual maximum and minimum values.
        /// </summary>
        protected virtual void CoerceActualMaxMin()
        {
            // Coerce actual minimum
            if (double.IsNaN(this.ActualMinimum) || double.IsInfinity(this.ActualMinimum))
            {
                this.ActualMinimum = 0;
            }

            // Coerce actual maximum
            if (double.IsNaN(this.ActualMaximum) || double.IsInfinity(this.ActualMaximum))
            {
                this.ActualMaximum = 100;
            }

            if (this.AbsoluteMaximum - this.AbsoluteMinimum < this.MinimumRange)
            {
                throw new InvalidOperationException("MinimumRange should be larger than AbsoluteMaximum-AbsoluteMinimum.");
            }

            // Coerce the minimum range
            if (this.ActualMaximum - this.ActualMinimum < this.MinimumRange)
            {
                if (this.ActualMinimum + this.MinimumRange < this.AbsoluteMaximum)
                {
                    var average = (this.ActualMaximum + this.ActualMinimum) * 0.5;
                    var delta = this.MinimumRange / 2;
                    this.ActualMinimum = average - delta;
                    this.ActualMaximum = average + delta;

                    if (this.ActualMinimum < this.AbsoluteMinimum)
                    {
                        var diff = this.AbsoluteMinimum - this.ActualMinimum;
                        this.ActualMinimum = this.AbsoluteMinimum;
                        this.ActualMaximum += diff;
                    }

                    if (this.ActualMaximum > this.AbsoluteMaximum)
                    {
                        var diff = this.AbsoluteMaximum - this.ActualMaximum;
                        this.ActualMaximum = this.AbsoluteMaximum;
                        this.ActualMinimum += diff;
                    }
                }
                else
                {
                    if (this.AbsoluteMaximum - this.MinimumRange > this.AbsoluteMinimum)
                    {
                        this.ActualMinimum = this.AbsoluteMaximum - this.MinimumRange;
                        this.ActualMaximum = this.AbsoluteMaximum;
                    }
                    else
                    {
                        this.ActualMaximum = this.AbsoluteMaximum;
                        this.ActualMinimum = this.AbsoluteMinimum;
                    }
                }
            }

            // Coerce the maximum range
            if (this.ActualMaximum - this.ActualMinimum > this.MaximumRange)
            {
                if (this.ActualMinimum + this.MaximumRange < this.AbsoluteMaximum)
                {
                    var average = (this.ActualMaximum + this.ActualMinimum) * 0.5;
                    var delta = this.MaximumRange / 2;
                    this.ActualMinimum = average - delta;
                    this.ActualMaximum = average + delta;

                    if (this.ActualMinimum < this.AbsoluteMinimum)
                    {
                        var diff = this.AbsoluteMinimum - this.ActualMinimum;
                        this.ActualMinimum = this.AbsoluteMinimum;
                        this.ActualMaximum += diff;
                    }

                    if (this.ActualMaximum > this.AbsoluteMaximum)
                    {
                        var diff = this.AbsoluteMaximum - this.ActualMaximum;
                        this.ActualMaximum = this.AbsoluteMaximum;
                        this.ActualMinimum += diff;
                    }
                }
                else
                {
                    if (this.AbsoluteMaximum - this.MaximumRange > this.AbsoluteMinimum)
                    {
                        this.ActualMinimum = this.AbsoluteMaximum - this.MaximumRange;
                        this.ActualMaximum = this.AbsoluteMaximum;
                    }
                    else
                    {
                        this.ActualMaximum = this.AbsoluteMaximum;
                        this.ActualMinimum = this.AbsoluteMinimum;
                    }
                }
            }

            // Coerce the absolute maximum/minimum
            if (AbsoluteMaximum <= AbsoluteMinimum)
            {
                throw new InvalidOperationException("AbsoluteMaximum should be larger than AbsoluteMinimum.");
            }

            if (this.ActualMaximum <= this.ActualMinimum)
            {
                this.ActualMaximum = this.ActualMinimum + 100;
            }

            if (this.ActualMinimum < this.AbsoluteMinimum)
            {
                this.ActualMinimum = this.AbsoluteMinimum;
            }

            if (this.ActualMinimum > this.AbsoluteMaximum)
            {
                this.ActualMinimum = this.AbsoluteMaximum;
            }

            if (this.ActualMaximum < this.AbsoluteMinimum)
            {
                this.ActualMaximum = this.AbsoluteMinimum;
            }

            if (this.ActualMaximum > this.AbsoluteMaximum)
            {
                this.ActualMaximum = this.AbsoluteMaximum;
            }
        }

        // Formats the value to be used on the axis.
        protected virtual string FormatValueOverride(double x)
        {
            string format = string.Concat("{0:", ActualStringFormat ?? StringFormat ?? string.Empty, "}");
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, format, x);
        }

        /// <summary>
        /// Calculates the actual maximum value of the axis, including the <see cref="MaximumPadding" />.
        /// </summary>
        /// <returns>The new actual maximum value of the axis.</returns>
        protected virtual double CalculateActualMaximum()
        {
            var actualMaximum = this.DataMaximum;
            double range = this.DataMaximum - this.DataMinimum;

            if (range < double.Epsilon)
            {
                double zeroRange = this.DataMaximum > 0 ? this.DataMaximum : 1;
                actualMaximum += zeroRange * 0.5;
            }

            if (!double.IsNaN(this.DataMinimum) && !double.IsNaN(actualMaximum))
            {
                double x1 = this.PreTransform(actualMaximum);
                double x0 = this.PreTransform(this.DataMinimum);
                //    double dx = this.MaximumPadding * (x1 - x0);
                return PostInverseTransform(x1 /*+ dx*/);
            }

            return actualMaximum;
        }

        /// <summary>
        /// Calculates the actual minimum value of the axis, including the <see cref="MinimumPadding" />.
        /// </summary>
        /// <returns>The new actual minimum value of the axis.</returns>
        protected virtual double CalculateActualMinimum()
        {
            var actualMinimum = this.DataMinimum;
            double range = this.DataMaximum - this.DataMinimum;

            if (range < double.Epsilon)
            {
                double zeroRange = this.DataMaximum > 0 ? this.DataMaximum : 1;
                actualMinimum -= zeroRange * 0.5;
            }

            if (!double.IsNaN(this.ActualMaximum))
            {
                double x1 = this.PreTransform(this.ActualMaximum);
                double x0 = this.PreTransform(actualMinimum);
                //  double dx = MinimumPadding * (x1 - x0);
                return PostInverseTransform(x0 /*- dx*/);
            }

            return actualMinimum;
        }

        /// <summary>
        /// Sets the transform.
        /// </summary>
        /// <param name="newScale">The new scale.</param>
        /// <param name="newOffset">The new offset.</param>
        protected void SetTransform(double newScale, double newOffset)
        {
            _scale = newScale;
            this.offset = newOffset;
            this.OnTransformChanged(new EventArgs());
        }

        /// <summary>
        /// Calculates the actual interval.
        /// </summary>
        /// <param name="availableSize">Size of the available area.</param>
        /// <param name="maxIntervalSize">Maximum length of the intervals.</param>
        /// <returns>The calculate actual interval.</returns>
        protected virtual double CalculateActualInterval(double availableSize, double maxIntervalSize)
        {
            return this.CalculateActualInterval(availableSize, maxIntervalSize, this.ActualMaximum - this.ActualMinimum);
        }

        /// <summary>
        /// Returns the actual interval to use to determine which values are displayed in the axis.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <param name="maxIntervalSize">The maximum interval size.</param>
        /// <param name="range">The range.</param>
        /// <returns>Actual interval to use to determine which values are displayed in the axis.</returns>
        protected double CalculateActualInterval(double availableSize, double maxIntervalSize, double range)
        {
            if (availableSize <= 0)
            {
                return maxIntervalSize;
            }

            if (Math.Abs(maxIntervalSize) < double.Epsilon)
            {
                throw new ArgumentException("Maximum interval size cannot be zero.", "maxIntervalSize");
            }

            if (Math.Abs(range) < double.Epsilon)
            {
                throw new ArgumentException("Range cannot be zero.", "range");
            }

            Func<double, double> exponent = x => Math.Ceiling(Math.Log(x, 10));
            Func<double, double> mantissa = x => x / Math.Pow(10, exponent(x) - 1);

            // reduce intervals for horizontal axis.
            // double maxIntervals = Orientation == AxisOrientation.x ? MaximumAxisIntervalsPer200Pixels * 0.8 : MaximumAxisIntervalsPer200Pixels;
            // real maximum interval count
            double maxIntervalCount = availableSize / maxIntervalSize;

            range = Math.Abs(range);
            double interval = Math.Pow(10, exponent(range));
            double intervalCandidate = interval;

            // Function to remove 'double precision noise'
            // TODO: can this be improved
            Func<double, double> removeNoise = x => double.Parse(x.ToString("e14"));

            // decrease interval until interval count becomes less than maxIntervalCount
            while (true)
            {
                var m = (int)mantissa(intervalCandidate);
                if (m == 5)
                {
                    // reduce 5 to 2
                    intervalCandidate = removeNoise(intervalCandidate / 2.5);
                }
                else if (m == 2 || m == 1 || m == 10)
                {
                    // reduce 2 to 1, 10 to 5, 1 to 0.5
                    intervalCandidate = removeNoise(intervalCandidate / 2.0);
                }
                else
                {
                    intervalCandidate = removeNoise(intervalCandidate / 2.0);
                }

                if (range / intervalCandidate > maxIntervalCount)
                {
                    break;
                }

                if (double.IsNaN(intervalCandidate) || double.IsInfinity(intervalCandidate))
                {
                    break;
                }

                interval = intervalCandidate;
            }

            return interval;
        }

        protected virtual void OnAxisChanged(AxisChangedEventArgs args)
        {
            UpdateActualMaxMin();

            AxisChanged?.Invoke(this, args);
        }

        protected virtual void OnTransformChanged(EventArgs args)
        {
            TransformChanged?.Invoke(this, args);
        }
    }
}
