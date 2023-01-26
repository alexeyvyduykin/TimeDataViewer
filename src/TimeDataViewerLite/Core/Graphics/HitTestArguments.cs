using TimeDataViewerLite.Spatial;

namespace TimeDataViewerLite.Core;

public class HitTestArguments
{
    public HitTestArguments(ScreenPoint point, double tolerance)
    {
        Point = point;
        Tolerance = tolerance;
    }

    public ScreenPoint Point { get; private set; }

    public double Tolerance { get; private set; }
}
