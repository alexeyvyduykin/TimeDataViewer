using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public class HitTestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HitTestResult" /> class.
        /// </summary>
        /// <param name="element">The element that was hit.</param>
        /// <param name="nearestHitPoint">The nearest hit point.</param>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        public HitTestResult(UIElement element, Point2D nearestHitPoint, object item = null, double index = 0)
        {
            this.Element = element;
            this.NearestHitPoint = nearestHitPoint;
            this.Item = item;
            this.Index = index;
        }

        /// <summary>
        /// Gets the index of the hit (if available).
        /// </summary>
        /// <value>The index.</value>
        /// <remarks>If the hit was in the middle between point 1 and 2, index = 1.5.</remarks>
        public double Index { get; private set; }

        /// <summary>
        /// Gets the item of the hit (if available).
        /// </summary>
        /// <value>The item.</value>
        public object Item { get; private set; }

        /// <summary>
        /// Gets the element that was hit.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        public UIElement Element { get; private set; }

        /// <summary>
        /// Gets the position of the nearest hit point.
        /// </summary>
        /// <value>The nearest hit point.</value>
        public Point2D NearestHitPoint { get; private set; }
    }
}
