using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public class TrackerHitResult
    {
        /// <summary>
        /// Gets or sets the nearest or interpolated data point.
        /// </summary>
        public DataPoint DataPoint { get; set; }

        /// <summary>
        /// Gets or sets the source item of the point.
        /// If the current point is from an ItemsSource and is not interpolated, this property will contain the item.
        /// </summary>
        public object Item { get; set; }

        /// <summary>
        /// Gets or sets the index for the Item.
        /// </summary>
        public double Index { get; set; }

        /// <summary>
        /// Gets or sets the horizontal/vertical line extents.
        /// </summary>
        public OxyRect LineExtents { get; set; }

        public PlotModel PlotModel { get; set; }

        /// <summary>
        /// Gets or sets the position in screen coordinates.
        /// </summary>
        public ScreenPoint Position { get; set; }

        public Series Series { get; set; }

        /// <summary>
        /// Gets or sets the text shown in the tracker.
        /// </summary>
        public string Text { get; set; }

        public Axis XAxis
        {
            get
            {
                var xyas = Series as XYAxisSeries;
                return xyas != null ? xyas.XAxis : null;
            }
        }

        public Axis YAxis
        {
            get
            {
                var xyas = Series as XYAxisSeries;
                return xyas != null ? xyas.YAxis : null;
            }
        }

        public override string ToString()
        {
            return Text != null ? Text.Trim() : string.Empty;
        }
    }
}
