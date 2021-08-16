using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{

    public static class ScreenPointHelper
    {

        /// <summary>
        /// Finds the point on line.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <param name="p1">The first point on the line.</param>
        /// <param name="p2">The second point on the line.</param>
        /// <returns>The nearest point on the line.</returns>
        /// <remarks>See <a href="http://paulbourke.net/geometry/pointlineplane/">Bourke</a>.</remarks>
        public static ScreenPoint FindPointOnLine(ScreenPoint p, ScreenPoint p1, ScreenPoint p2)
        {
            double dx = p2.x - p1.x;
            double dy = p2.y - p1.y;
            double u = FindPositionOnLine(p, p1, p2);

            if (double.IsNaN(u))
            {
                u = 0;
            }

            if (u < 0)
            {
                u = 0;
            }

            if (u > 1)
            {
                u = 1;
            }

            return new ScreenPoint(p1.x + (u * dx), p1.y + (u * dy));
        }

        /// <summary>
        /// Finds the nearest point on line.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <param name="p1">The start point on the line.</param>
        /// <param name="p2">The end point on the line.</param>
        /// <returns>The relative position of the nearest point.</returns>
        /// <remarks>See <a href="http://paulbourke.net/geometry/pointlineplane/">Bourke</a>.</remarks>
        public static double FindPositionOnLine(ScreenPoint p, ScreenPoint p1, ScreenPoint p2)
        {
            double dx = p2.x - p1.x;
            double dy = p2.y - p1.y;
            double u1 = ((p.x - p1.x) * dx) + ((p.y - p1.y) * dy);
            double u2 = (dx * dx) + (dy * dy);

            if (u2 < 1e-6)
            {
                return double.NaN;
            }

            return u1 / u2;
        }
    }
}
