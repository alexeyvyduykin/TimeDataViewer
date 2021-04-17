using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using TimeDataViewer;

namespace TimeDataViewer
{
    public partial class SchedulerControl
    {
        private enum BackgroundMode { Hour, Day, Week, Month, Year }  
        private readonly IBrush _brushFirst = new SolidColorBrush() { Color = Color.Parse("#BDBDBD") /*Colors.Silver*/ };
        private readonly IBrush _brushSecond = new SolidColorBrush() { Color = Color.Parse("#F5F5F5") /*Colors.WhiteSmoke*/ };
        private int _prevCount = 0;
        private Grid _grid;
        private Brush _areaBackground;

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
                    var rect = new Rectangle() { Fill = _brushFirst };
                    Grid.SetColumn(rect, i);
                    grid.Children.Add(rect);
                }
                else
                {
                    var rect = new Rectangle() { Fill = _brushSecond };
                    Grid.SetColumn(rect, i);
                    grid.Children.Add(rect);
                }
            }

            return grid;
        }

        private void UpdateBackgroundBrush()
        {
            var w = ViewportAreaScreen.Width;
            var len = ViewportAreaData.Width;
     
            if (w == 0)
                return;

            int count = 0;
        
            if (IsRange(w, 0.0, 3600.0) == true) // Hour
            {            
                AxisX.TimePeriodMode = TimePeriod.Hour;
                count = (int)(len / (60 * 24 * 86400.0));
            }
            else if (IsRange(w, 0.0, 86400.0) == true) // Day
            {              
                AxisX.TimePeriodMode = TimePeriod.Day;
                count = (int)(len / (24 * 86400.0));
            }
            else if (IsRange(w, 0.0, 7 * 86400.0) == true) // Week
            {              
                AxisX.TimePeriodMode = TimePeriod.Week;
                count = (int)(len / 86400.0);
            }
            else if (IsRange(w, 0.0, 30 * 86400.0) == true) // Month
            {             
                AxisX.TimePeriodMode = TimePeriod.Month;
                count = (int)(len / 86400.0);
            }
            else if (IsRange(w, 0.0, 12 * 30 * 86400.0) == true) // Year
            {              
                throw new Exception();
            }
        
            if (count != _prevCount)
            {
                _grid = CreateGrid(count);
                _prevCount = count;
            }

            _grid.Width = AbsoluteWindow.Width;
            _grid.Height = AbsoluteWindow.Height;

            _areaBackground = new VisualBrush()
            {
                Visual = _grid,
                DestinationRect = new RelativeRect(
                     RenderOffsetAbsolute.X, 0, AbsoluteWindow.Width, AbsoluteWindow.Height, RelativeUnit.Absolute)
            };
        }

        private bool IsRange(double value, double min, double max) => value >= min && value <= max;
    }
}
