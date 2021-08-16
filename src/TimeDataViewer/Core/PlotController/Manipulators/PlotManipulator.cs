using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public abstract class PlotManipulator<T> : ManipulatorBase<T> where T : OxyInputEventArgs
    {
        protected PlotManipulator(IPlotView view) : base(view)
        {
            PlotView = view;
        }

        public IPlotView PlotView { get; private set; }

        protected Axis XAxis { get; set; }

        protected Axis YAxis { get; set; }

        protected DataPoint InverseTransform(double x, double y)
        {
            if (this.XAxis != null)
            {
                return this.XAxis.InverseTransform(x, y, this.YAxis);
            }

            if (this.YAxis != null)
            {
                return new DataPoint(0, this.YAxis.InverseTransform(y));
            }

            return new DataPoint();
        }

        protected void AssignAxes(ScreenPoint position)
        {
            Axis xaxis;
            Axis yaxis;
            if (this.PlotView.ActualModel != null)
            {
                this.PlotView.ActualModel.GetAxesFromPoint(out xaxis, out yaxis);
            }
            else
            {
                xaxis = null;
                yaxis = null;
            }

            this.XAxis = xaxis;
            this.YAxis = yaxis;
        }
    }
}
