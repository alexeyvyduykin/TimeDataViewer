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
        private DrawCanvas _drawCanvas;
        private Panel _panel;
        private Canvas _overlays;
        private ContentControl _zoomControl;

        // Invalidation flag (0: no update, 1: update visual elements).  
        private int _isPlotInvalidated;

        protected TimelineBase()
        {
            DisconnectCanvasWhileUpdating = true;
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
        public RectD ClientArea => new RectD(0, 0, Bounds.Width, Bounds.Height);

        public void HideZoomRectangle()
        {
            _zoomControl.IsVisible = false;
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

        public void ShowZoomRectangle(RectD r)
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

        protected override Size ArrangeOverride(Size finalSize)
        {
            var actualSize = base.ArrangeOverride(finalSize);
            if (actualSize.Width > 0 && actualSize.Height > 0)
            {
                //if (Interlocked.CompareExchange(ref _isPlotInvalidated, 0, 1) == 1)
                {
                    UpdateVisuals();
                }
            }

            return actualSize;
        }

        public void InvalidatePlot(bool updateData = true)
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            UpdateModel(updateData);

            //if (Interlocked.CompareExchange(ref _isPlotInvalidated, 1, 0) == 0)
            //{
            //    // Invalidate the arrange state for the element.
            //    // After the invalidation, the element will have its layout updated,
            //    // which will occur asynchronously unless subsequently forced by UpdateLayout.
            //    BeginInvoke(InvalidateArrange);
            //    BeginInvoke(InvalidateVisual);
            //}
            
            BeginInvoke(InvalidateVisual);
        }
        
        // Called when the size of the control is changed.
        private void OnSizeChanged(object sender, Size size)
        {
            if (size.Height > 0 && size.Width > 0)
            {
                InvalidatePlot(false);
            }
        }
        
        protected virtual void UpdateModel(bool updateData = true)
        {
            if (ActualModel != null)
            {
                ((PlotModel)ActualModel).Update(updateData);
            }
        }


        // When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass)
        // call <see cref="M:System.Windows.Controls.Control.ApplyTemplate" /> . In simplest terms, this means the method is called 
        // just before a UI element displays in an application. For more information, see Remarks.
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _panel = e.NameScope.Find("PART_Panel") as Panel;
            if (_panel == null)
            {
                return;
            }

            _drawCanvas = new DrawCanvas() { Background = Brushes.Transparent };
         
            _panel.Children.Add(_drawCanvas);
        
            _overlays = new Canvas { Name = "Overlays" };
            _panel.Children.Add(_overlays);

            _zoomControl = new ContentControl();
            _overlays.Children.Add(_zoomControl);
        }

        // Provides the behavior for the Arrange pass of Silverlight layout.
        // Classes can override this method to define their own Arrange pass behavior.
        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    var actualSize = base.ArrangeOverride(finalSize);
        //    if (actualSize.Width > 0 && actualSize.Height > 0)
        //    {
        //        if (Interlocked.CompareExchange(ref _isPlotInvalidated, 0, 1) == 1)
        //        {
        //            UpdateVisuals();
        //        }
        //    }

        //    return actualSize;
        //}



        // Determines whether the plot is currently visible to the user.
        //protected bool IsVisibleToUser()
        //{
        //    return IsUserVisible(this);
        //}

        // Determines whether the specified element is currently visible to the user.
        //private static bool IsUserVisible(Control element)
        //{
        //    return element.IsEffectivelyVisible && element.TransformedBounds.HasValue;
        //}



        // Gets the relevant parent.
        //private Control GetRelevantParent<T>(IVisual obj) where T : Control
        //{
        //    var container = obj.VisualParent;

        //    if (container is ContentPresenter contentPresenter)
        //    {
        //        container = GetRelevantParent<T>(contentPresenter);
        //    }

        //    if (container is Panel panel)
        //    {
        //        container = GetRelevantParent<ScrollViewer>(panel);
        //    }

        //    if ((container is T) == false && (container != null))
        //    {
        //        container = GetRelevantParent<T>(container);
        //    }

        //    return (Control)container;
        //}

        private void UpdateVisuals()
        {
            if (_drawCanvas == null)
            {
                return;
            }

            //if (IsVisibleToUser() == false)
            //{
            //    return;
            //}

            // Clear the canvas
            _drawCanvas.Children.Clear();

            if (ActualModel != null)
            {
                if (DisconnectCanvasWhileUpdating)
                {
                    // TODO: profile... not sure if this makes any difference
                    var idx = _panel.Children.IndexOf(_drawCanvas);
                    if (idx != -1)
                    {
                        _panel.Children.RemoveAt(idx);
                    }

                    ((PlotModel)ActualModel).Render(_drawCanvas.Bounds.Width, _drawCanvas.Bounds.Height);
                    MyRenderSeries(_drawCanvas);
                    // reinsert the canvas again
                    if (idx != -1)
                    {
                        _panel.Children.Insert(idx, _drawCanvas);
                    }
                }
                else
                {
                    ((PlotModel)ActualModel).Render(_drawCanvas.Bounds.Width, _drawCanvas.Bounds.Height);
                    MyRenderSeries(_drawCanvas);
                }
            }
        }

        protected abstract void MyRenderSeries(DrawCanvas drawCanvas);

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
