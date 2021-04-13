using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using TimeDataViewer;

namespace AvaloniaDemo.Views
{
    public partial class SchedulerControl
    {
        private enum BackgroundMode { Hour, Day, Week, Month, Year }
        private VisualBrush _gridBrush;
        private readonly Dictionary<BackgroundMode, Grid> _backgrounds = new();
        private BackgroundMode _currentBackgroundMode;

        private VisualBrush GridBrush => _gridBrush;

        private VisualBrush CreateBrush____()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            var lin = new LinearGradientBrush()
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(AbsoluteWindow.Width, AbsoluteWindow.Height, RelativeUnit.Absolute),
            };

            lin.GradientStops.Add(new GradientStop() { Color = Colors.Yellow, Offset = 0 });
            lin.GradientStops.Add(new GradientStop() { Color = Colors.Black, Offset = 1 });

            Rectangle rect = new Rectangle()
            {
                Fill = lin
            };


            Grid.SetColumn(rect, 0);
            Grid.SetRow(rect, 0);
            grid.Children.Add(rect);

            grid.Width = AbsoluteWindow.Width;
            grid.Height = AbsoluteWindow.Height;


            var brush = new VisualBrush()
            {
                Visual = grid,
                DestinationRect = new RelativeRect(
                RenderOffsetAbsolute.X, 0,
                AbsoluteWindow.Width, AbsoluteWindow.Height, RelativeUnit.Absolute)

            };


            return brush;

        }

        private VisualBrush CreateBrush__()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            Image image1 = new Image()
            {
                //    HorizontalAlignment = HorizontalAlignment.Stretch, 
                //   VerticalAlignment = VerticalAlignment.Stretch 
            };

            image1.Source = new Bitmap(@"C:\data\textureTemp\earth.bmp");

            Grid.SetColumn(image1, 0);
            Grid.SetRow(image1, 0);
            grid.Children.Add(image1);

            grid.Width = AbsoluteWindow.Width;
            grid.Height = AbsoluteWindow.Height;


            var brush = new VisualBrush()
            {
                Visual = grid,
                TileMode = TileMode.Tile
            };


            return brush;

        }

        private void InitBackgrounds()
        {
            _backgrounds.Add(BackgroundMode.Hour, CreateGrid(60));

            _backgrounds.Add(BackgroundMode.Day, CreateGrid(24));

            _backgrounds.Add(BackgroundMode.Week, CreateGrid(7));

            _backgrounds.Add(BackgroundMode.Month, CreateGrid(12));

            _backgrounds.Add(BackgroundMode.Year, CreateGrid(12));

            _gridBrush = CreateGridBrush0();
        }

        private Grid CreateGrid(int count)
        {
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < count; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    var rect = new Rectangle() { Fill = Brushes.Silver };
                    Grid.SetColumn(rect, i);
                    grid.Children.Add(rect);
                }
                else
                {
                    var rect = new Rectangle() { Fill = Brushes.WhiteSmoke };
                    Grid.SetColumn(rect, i);
                    grid.Children.Add(rect);
                }
            }

            return grid;
        }

        private VisualBrush CreateBrush()
        {
            var grid = _backgrounds[_currentBackgroundMode];
       
            grid.Width = AbsoluteWindow.Width;
            grid.Height = AbsoluteWindow.Height;

            return new VisualBrush()
            {
                Visual = grid,
                DestinationRect = new RelativeRect(
                    RenderOffsetAbsolute.X, 0, AbsoluteWindow.Width, AbsoluteWindow.Height, RelativeUnit.Absolute)
            };
        }

        private VisualBrush CreateGridBrush0()
        {
            // Create a DrawingBrush  
            VisualBrush blackBrush = new VisualBrush();

            // Create a Geometry with white background  
            GeometryDrawing backgroundSquare = new GeometryDrawing()
            {
                Brush = Brushes.DarkGray,
                Geometry = new RectangleGeometry(new Rect(0, 0, 1, 1))
            };

            // Create a GeometryGroup that will be added to Geometry  
            // GeometryGroup gGroup = new GeometryGroup();
            // gGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 0.5, 1)));

            // Create a GeomertyDrawing  
            GeometryDrawing checkers = new GeometryDrawing()
            {
                Brush = new SolidColorBrush(Colors.GhostWhite),
                Geometry = new RectangleGeometry(new Rect(0, 0, 0.5, 1))//gGroup 
            };
            DrawingGroup checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            // blackBrush.Visual = checkersDrawingGroup;
            blackBrush.Visual = new Rectangle() { Width = 800, Height = 600, Fill = Brushes.Silver };

            // Set Viewport and TimeMode  
            blackBrush.DestinationRect = new RelativeRect(0, 0, 1.0, 1.0, RelativeUnit.Relative);//Viewport = new Rect(0, 0, 1.0, 1.0);
            blackBrush.TileMode = TileMode.Tile;

            return blackBrush;
        }

        private void SchedulerBase_OnMapZoomChanged()
        {
            var w = ViewportAreaScreen.Width;
            var w0 = ViewportAreaData.Width;

            if (w == 0)
                return;

            if (IsRange(w, 0.0, 3600.0) == true) // Hour
            {
                _currentBackgroundMode = BackgroundMode.Hour;
                var ww = 1.0 / (12.0 * 12.0 * w0 / 86400.0);
                GridBrush.DestinationRect = new RelativeRect(0, 0, ww, 1.0, RelativeUnit.Relative);// Viewport = new Rect(0, 0, ww, 1.0);
                (AxisX as TimeAxis).TimePeriodMode = TimePeriod.Hour;
            }
            else if (IsRange(w, 0.0, 86400.0) == true) // Day
            {
                _currentBackgroundMode = BackgroundMode.Day;
                var ww = 1.0 / (12.0 * w0 / 86400.0);
                GridBrush.DestinationRect = new RelativeRect(0, 0, ww, 1.0, RelativeUnit.Relative);//Viewport = new Rectangle(0, 0, ww, 1.0);
                (AxisX as TimeAxis).TimePeriodMode = TimePeriod.Day;
            }
            else if (IsRange(w, 0.0, 7 * 86400.0) == true) // Week
            {
                _currentBackgroundMode = BackgroundMode.Week;
                var ww = 1.0 / (w0 / 86400.0);
                GridBrush.DestinationRect = new RelativeRect(0, 0, ww, 1.0, RelativeUnit.Relative);//Viewport = new Rect(0, 0, ww, 1.0);
                (AxisX as TimeAxis).TimePeriodMode = TimePeriod.Week;
            }
            else if (IsRange(w, 0.0, 30 * 86400.0) == true) // Month
            {
                _currentBackgroundMode = BackgroundMode.Month;
                var ww = 1.0 / (w0 / 86400.0);
                GridBrush.DestinationRect = new RelativeRect(0, 0, ww, 1.0, RelativeUnit.Relative);//Viewport = new Rect(0, 0, ww, 1.0);
                (AxisX as TimeAxis).TimePeriodMode = TimePeriod.Month;
            }
            else if (IsRange(w, 0.0, 12 * 30 * 86400.0) == true) // Year
            {
                _currentBackgroundMode = BackgroundMode.Year;
                throw new Exception();
            }
            
            _areaBackground = CreateBrush();
        }

        private bool IsRange(double value, double min, double max) => value >= min && value <= max;
    }
}
