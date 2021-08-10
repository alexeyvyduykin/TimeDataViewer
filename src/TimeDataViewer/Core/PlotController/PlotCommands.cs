﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeDataViewer.Core
{
    public static class PlotCommands
    {
        static PlotCommands()
        {
            // commands that can be triggered from mouse down events
            //ResetAt = new DelegatePlotCommand<OxyMouseEventArgs>((view, controller, args) => HandleReset(view, args));
            PanAt = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) => controller.AddMouseManipulator(view, new PanManipulator(view), args));
            //ZoomRectangle = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) => controller.AddMouseManipulator(view, new ZoomRectangleManipulator(view), args));
            //    Track = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) => controller.AddMouseManipulator(view, new TrackerManipulator(view) { Snap = false, PointsOnly = false }, args));
            //    SnapTrack = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) => controller.AddMouseManipulator(view, new TrackerManipulator(view) { Snap = true, PointsOnly = false }, args));
            //    PointsOnlyTrack = new DelegatePlotCommand<OxyMouseDownEventArgs>((view, controller, args) => controller.AddMouseManipulator(view, new TrackerManipulator(view) { Snap = false, PointsOnly = true }, args));
            ZoomWheel = new DelegatePlotCommand<OxyMouseWheelEventArgs>((view, controller, args) => HandleZoomByWheel(view, args));
            //ZoomWheelFine = new DelegatePlotCommand<OxyMouseWheelEventArgs>((view, controller, args) => HandleZoomByWheel(view, args, 0.1));
            ZoomInAt = new DelegatePlotCommand<OxyMouseEventArgs>((view, controller, args) => HandleZoomAt(view, args, 0.05));
            ZoomOutAt = new DelegatePlotCommand<OxyMouseEventArgs>((view, controller, args) => HandleZoomAt(view, args, -0.05));

            // commands that can be triggered from mouse enter events
            //  HoverTrack = new DelegatePlotCommand<OxyMouseEventArgs>((view, controller, args) => controller.AddHoverManipulator(view, new TrackerManipulator(view) { LockToInitialSeries = false, Snap = false, PointsOnly = false }, args));
            //  HoverSnapTrack = new DelegatePlotCommand<OxyMouseEventArgs>((view, controller, args) => controller.AddHoverManipulator(view, new TrackerManipulator(view) { LockToInitialSeries = false, Snap = true, PointsOnly = false }, args));
            //  HoverPointsOnlyTrack = new DelegatePlotCommand<OxyMouseEventArgs>((view, controller, args) => controller.AddHoverManipulator(view, new TrackerManipulator(view) { LockToInitialSeries = false, Snap = false, PointsOnly = true }, args));
        }

        // Gets the reset axes command (for mouse events).     
        //public static IViewCommand<OxyMouseEventArgs> ResetAt { get; private set; }

        // Gets the pan command.   
        public static IViewCommand<OxyMouseDownEventArgs> PanAt { get; private set; }

        // Gets the zoom rectangle command.     
        //public static IViewCommand<OxyMouseDownEventArgs> ZoomRectangle { get; private set; }

        // Gets the zoom by mouse wheel command.       
        public static IViewCommand<OxyMouseWheelEventArgs> ZoomWheel { get; private set; }

        // Gets the fine-control zoom by mouse wheel command.      
        //public static IViewCommand<OxyMouseWheelEventArgs> ZoomWheelFine { get; private set; }

        // Gets the tracker command.        
        //  public static IViewCommand<OxyMouseDownEventArgs> Track { get; private set; }

        // Gets the snap tracker command.       
        //  public static IViewCommand<OxyMouseDownEventArgs> SnapTrack { get; private set; }

        // Gets the points only tracker command.     
        //  public static IViewCommand<OxyMouseDownEventArgs> PointsOnlyTrack { get; private set; }

        // Gets the mouse hover tracker.    
        //  public static IViewCommand<OxyMouseEventArgs> HoverTrack { get; private set; }

        // Gets the mouse hover snap tracker.   
        //   public static IViewCommand<OxyMouseEventArgs> HoverSnapTrack { get; private set; }

        // Gets the mouse hover points only tracker.   
        //   public static IViewCommand<OxyMouseEventArgs> HoverPointsOnlyTrack { get; private set; }

        // Gets the zoom in command.       
        public static IViewCommand<OxyMouseEventArgs> ZoomInAt { get; private set; }

        // Gets the zoom out command.   
        public static IViewCommand<OxyMouseEventArgs> ZoomOutAt { get; private set; }

        // Handles the reset event.
        private static void HandleReset(IPlotView view, OxyInputEventArgs args)
        {
            args.Handled = true;
            view.ActualModel.ResetAllAxes();
            view.InvalidatePlot(false);
        }

        // Zooms the view by the specified factor at the position specified in the <see cref="OxyMouseEventArgs" />.
        private static void HandleZoomAt(IPlotView view, OxyMouseEventArgs args, double delta)
        {
            var m = new MyZoomStepManipulator(view) { Step = delta };
            m.Started(args);
        }

        // Zooms the view by the mouse wheel delta in the specified <see cref="OxyKeyEventArgs" />.
        private static void HandleZoomByWheel(IPlotView view, OxyMouseWheelEventArgs args, double factor = 1)
        {
            var m = new MyZoomStepManipulator(view) { Step = args.Delta * 0.001 * factor };
            m.Started(args);
        }

        // Zooms the view by the key in the specified factor.
        private static void HandleZoomCenter(IPlotView view, OxyInputEventArgs args, double delta)
        {
            args.Handled = true;
            view.ActualModel.ZoomAllAxes(1 + (delta * 0.12));
            view.InvalidatePlot(false);
        }

        // Pans the view by the key in the specified vector.
        private static void HandlePan(IPlotView view, OxyInputEventArgs args, double dx, double dy)
        {
            args.Handled = true;
            dx *= view.ActualModel.PlotArea.Width;
            dy *= view.ActualModel.PlotArea.Height;
            view.ActualModel.PanAllAxes(dx, dy);
            view.InvalidatePlot(false);
        }
    }
}
