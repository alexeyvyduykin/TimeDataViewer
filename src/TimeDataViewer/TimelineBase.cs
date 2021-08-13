using Avalonia.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using System.ComponentModel;
using Avalonia.Metadata;
using Avalonia.Styling;
using TimeDataViewer.ViewModels;
using TimeDataViewer;
using TimeDataViewer.Spatial;
using System.Xml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Metadata;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using TimeDataViewer.Core;
using Avalonia.Controls.Generators;
using System.Threading.Tasks;
using TimeDataViewer.Views;
using Avalonia.Collections;
using Core = TimeDataViewer.Core;

namespace TimeDataViewer
{
    public abstract partial class TimelineBase : TemplatedControl, IPlotView
    {
        private Canvas _canvas;
        private Canvas _canvasX;
        private DrawCanvas _drawCanvas;
        private Panel _panel;
        private Panel _panelX;  
        // Invalidation flag (0: no update, 1: update visual elements).  
        private int _isPlotInvalidated;      
        private Canvas _overlays;
        private ContentControl _zoomControl;
        private readonly ObservableCollection<TrackerDefinition> _trackerDefinitions;
        private IControl _currentTracker;

        protected TimelineBase()
        {
            DisconnectCanvasWhileUpdating = true;
            _trackerDefinitions = new ObservableCollection<TrackerDefinition>();
            this.GetObservable(TransformedBoundsProperty).Subscribe(bounds => OnSizeChanged(this, bounds?.Bounds.Size ?? new Size()));
        }

        // Gets or sets a value indicating whether to disconnect the canvas while updating.
        public bool DisconnectCanvasWhileUpdating { get; set; }

        // Gets the actual model in the view.
        Model IView.ActualModel => ActualModel;

        public abstract PlotModel ActualModel { get; }

        IController IView.ActualController => ActualController;

        public abstract IPlotController ActualController { get; }

        // Gets the coordinates of the client area of the view.
        public OxyRect ClientArea => new OxyRect(0, 0, Bounds.Width, Bounds.Height);

        public ObservableCollection<TrackerDefinition> TrackerDefinitions => _trackerDefinitions;
        
        public void HideTracker()
        {
            if (_currentTracker != null)
            {
                _overlays.Children.Remove(_currentTracker);
                _currentTracker = null;
            }
        }

        public void HideZoomRectangle()
        {
            _zoomControl.IsVisible = false;
        }

        //public void PanAllAxes(Vector delta)
        //{
        //    if (ActualModel != null)
        //    {
        //        ActualModel.PanAllAxes(delta.X, delta.Y);
        //    }

        //    InvalidatePlot(false);
        //}

        //public void ZoomAllAxes(double factor)
        //{
        //    if (ActualModel != null)
        //    {
        //        ActualModel.ZoomAllAxes(factor);
        //    }

        //    InvalidatePlot(false);
        //}

        //public void ResetAllAxes()
        //{
        //    if (ActualModel != null)
        //    {
        //        ActualModel.ResetAllAxes();
        //    }

        //    InvalidatePlot(false);
        //}

        // Invalidate the PlotView (not blocking the UI thread)
        public void InvalidatePlot(bool updateData = true)
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            UpdateModel(updateData);

            if (Interlocked.CompareExchange(ref _isPlotInvalidated, 1, 0) == 0)
            {
                // Invalidate the arrange state for the element.
                // After the invalidation, the element will have its layout updated,
                // which will occur asynchronously unless subsequently forced by UpdateLayout.
                BeginInvoke(InvalidateArrange);
                BeginInvoke(InvalidateVisual);
            }
        }

        // When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass)
        // call <see cref="M:System.Windows.Controls.Control.ApplyTemplate" /> . In simplest terms, this means the method is called 
        // just before a UI element displays in an application. For more information, see Remarks.
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _panel = e.NameScope.Find("PART_Panel") as Panel;
            _panelX = e.NameScope.Find("PART_PanelX") as Panel;           
            if (_panel == null)
            {
                return;
            }

            _panel.PointerEnter += _panel_PointerEnter;
            _panel.PointerLeave += _panel_PointerLeave;            
            _panel.PointerWheelChanged += _panel_PointerWheelChanged;            
            _panel.PointerPressed += _panel_PointerPressed;            
            _panel.PointerMoved += _panel_PointerMoved;            
            _panel.PointerReleased += _panel_PointerReleased;            

            _canvas = new Canvas() { Background = Brushes.Transparent };
            _drawCanvas = new DrawCanvas() { Background = Brushes.Transparent };

            _canvasX = new Canvas() { Background = Brushes.Transparent };
     
            _panel.Children.Add(_canvas);
            _panel.Children.Add(_drawCanvas);
            _panelX.Children.Add(_canvasX);
            
            _overlays = new Canvas { Name = "Overlays" };
            _panel.Children.Add(_overlays);

            _zoomControl = new ContentControl();
            _overlays.Children.Add(_zoomControl);
        }

        public void SetCursorType(CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.Pan:
                    Cursor = PanCursor;
                    break;
                case CursorType.PanHorizontal:
                    Cursor = PanHorizontalCursor;
                    break;
                case CursorType.ZoomRectangle:
                    Cursor = ZoomRectangleCursor;
                    break;
                case CursorType.ZoomHorizontal:
                    Cursor = ZoomHorizontalCursor;
                    break;
                case CursorType.ZoomVertical:
                    Cursor = ZoomVerticalCursor;
                    break;
                default:
                    Cursor = Cursor.Default;
                    break;
            }
        }

        public void ShowTracker(TrackerHitResult trackerHitResult)
        {
            if (trackerHitResult == null)
            {
                HideTracker();
                return;
            }

            var trackerTemplate = DefaultTrackerTemplate;
            if (trackerHitResult.Series != null && !string.IsNullOrEmpty(trackerHitResult.Series.TrackerKey))
            {
                var match = TrackerDefinitions.FirstOrDefault(t => t.TrackerKey == trackerHitResult.Series.TrackerKey);
                if (match != null)
                {
                    trackerTemplate = match.TrackerTemplate;
                }
            }

            if (trackerTemplate == null)
            {
                HideTracker();
                return;
            }

            var tracker = trackerTemplate.Build(new ContentControl());

            // ReSharper disable once RedundantNameQualifier
            if (!object.ReferenceEquals(tracker, _currentTracker))
            {
                HideTracker();
                _overlays.Children.Add(tracker.Control);
                _currentTracker = tracker.Control;
            }

            if (_currentTracker != null)
            {
                _currentTracker.DataContext = trackerHitResult;
            }
        }

        public void ShowZoomRectangle(OxyRect r)
        {
            _zoomControl.Width = r.Width;
            _zoomControl.Height = r.Height;
            Canvas.SetLeft(_zoomControl, r.Left);
            Canvas.SetTop(_zoomControl, r.Top);
            _zoomControl.Template = ZoomRectangleTemplate;
            _zoomControl.IsVisible = true;
        }

        // Stores text on the clipboard.
        public async void SetClipboardText(string text)
        {
            await AvaloniaLocator.Current.GetService<IClipboard>().SetTextAsync(text);
        }

        // Provides the behavior for the Arrange pass of Silverlight layout.
        // Classes can override this method to define their own Arrange pass behavior.
        protected override Size ArrangeOverride(Size finalSize)
        {
            var actualSize = base.ArrangeOverride(finalSize);
            if (actualSize.Width > 0 && actualSize.Height > 0)
            {
                if (Interlocked.CompareExchange(ref _isPlotInvalidated, 0, 1) == 1)
                {
                    UpdateVisuals();
                }
            }

            return actualSize;
        }

        protected virtual void UpdateModel(bool updateData = true)
        {
            if (ActualModel != null)
            {
                ((IPlotModel)ActualModel).Update(updateData);
            }
        }

        // Determines whether the plot is currently visible to the user.
        protected bool IsVisibleToUser()
        {
            return IsUserVisible(this);
        }

        // Determines whether the specified element is currently visible to the user.
        private static bool IsUserVisible(Control element)
        {
            return element.IsEffectivelyVisible && element.TransformedBounds.HasValue;
        }

        // Called when the size of the control is changed.
        private void OnSizeChanged(object sender, Size size)
        {
            if (size.Height > 0 && size.Width > 0)
            {
                InvalidatePlot(false);
            }
        }

        // Gets the relevant parent.
        private Control GetRelevantParent<T>(IVisual obj) where T : Control
        {
            var container = obj.VisualParent;

            if (container is ContentPresenter contentPresenter)
            {
                container = GetRelevantParent<T>(contentPresenter);
            }

            if (container is Panel panel)
            {
                container = GetRelevantParent<ScrollViewer>(panel);
            }

            if ((container is T) == false && (container != null))
            {
                container = GetRelevantParent<T>(container);
            }

            return (Control)container;
        }

        private void UpdateVisuals()
        {
            if (_canvas == null)
            {
                return;
            }

            if (IsVisibleToUser() == false)
            {
                return;
            }

            // Clear the canvas
            _canvas.Children.Clear();
            _canvasX.Children.Clear();

            if (ActualModel != null)
            {
                if (DisconnectCanvasWhileUpdating)
                {
                    // TODO: profile... not sure if this makes any difference
                    var idx = _panel.Children.IndexOf(_canvas);
                    if (idx != -1)
                    {
                        _panel.Children.RemoveAt(idx);
                    }

                    ((IPlotModel)ActualModel).Render(_canvas.Bounds.Width, _canvas.Bounds.Height);
                    MyRenderSeries(_canvas, _drawCanvas);
                    MyRenderAxisX(_canvasX, _canvas);                  
                    // reinsert the canvas again
                    if (idx != -1)
                    {
                        _panel.Children.Insert(idx, _canvas);
                    }
                }
                else
                {
                    ((IPlotModel)ActualModel).Render(_canvas.Bounds.Width, _canvas.Bounds.Height);
                    MyRenderSeries(_canvas, _drawCanvas);
                    MyRenderAxisX(_canvasX, _canvas);                
                }
            }
        }

        protected abstract void MyRenderAxisX(Canvas canvasAxis, Canvas canvasPlot);
        protected abstract void MyRenderAxisY(Canvas canvasAxis, Canvas canvasPlot);

        protected abstract void MyRenderSeries(Canvas canvasPlot, DrawCanvas drawCanvas);

        // Invokes the specified action on the dispatcher, if necessary.
        private static void BeginInvoke(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Loaded);
            }
            else
            {
                action?.Invoke();
            }
        }
    }
}
