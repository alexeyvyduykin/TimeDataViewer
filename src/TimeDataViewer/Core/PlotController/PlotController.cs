namespace TimeDataViewer.Core
{
    public class PlotController : ControllerBase, IPlotController
    {
        public PlotController()
        {
            // Zoom rectangle bindings: MMB / control RMB / control+alt LMB
            this.BindMouseDown(OxyMouseButton.Middle, PlotCommands.ZoomRectangle);

            // Pan bindings: RMB / alt LMB / Up/down/left/right keys (panning direction on axis is opposite of key as it is more intuitive)
            this.BindMouseDown(OxyMouseButton.Right, PlotCommands.PanAt);

            // Tracker bindings: LMB
            this.BindMouseDown(OxyMouseButton.Left, PlotCommands.SnapTrack);

            // Zoom in/out binding: XB1 / XB2 / mouse wheels / +/- keys
            this.BindMouseWheel(PlotCommands.ZoomWheel);
        }
    }
}
