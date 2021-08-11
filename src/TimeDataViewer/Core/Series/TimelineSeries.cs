using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeDataViewer.Spatial;

namespace TimeDataViewer.Core
{
    public class TimelineSeries : Series
    {
        public IList<TimelineItem> Items { get; private set; } = new List<TimelineItem>();

        public TimeDataViewer.Series SeriesControl{get;set;}

        public string CategoryField { get; set; }

        public string BeginField { get; set; }

        public string EndField { get; set; }

        public double MinTime() => (Items.Count == 0) ? 0.0 : Items.Min(s => s.Begin);

        public double MaxTime() => (Items.Count == 0) ? 0.0 : Items.Max(s => s.End);

        protected internal override void UpdateData()
        {
            if (ItemsSource != null)
            {
                if (ItemsSource is IEnumerable<TimelineItem> ivals)
                {
                    Items = new List<TimelineItem>(ivals);
                }
                else
                {
                    Items = UpdateItems();
                }
            }
        }

        private IList<TimelineItem> UpdateItems()
        {
            if (string.IsNullOrWhiteSpace(BeginField) == false && string.IsNullOrWhiteSpace(EndField) == false)
            {
                var list = new List<TimelineItem>();
             
                foreach (var item in ItemsSource)
                {
                    var propertyInfoLeft = item.GetType().GetProperty(BeginField);
                    var propertyInfoRight = item.GetType().GetProperty(EndField);

                    var valueLeft = propertyInfoLeft?.GetValue(item, null);
                    var valueRight = propertyInfoRight?.GetValue(item, null);

                    if (valueLeft is not null && valueRight is not null && valueLeft is double left && valueRight is double right)
                    {
                        list.Add(new TimelineItem(left, right)
                        {
                            ZIndex = 100,
                            SeriesControl = SeriesControl
                        });
                    }
                }
                return list;
            }

            return new List<TimelineItem>();
        }


        public override void Render()
        {            
            if (Parent is not null)
            {                
                double heightY = 10;

                MyRectList.Clear();
                MyClippingRect = Parent.PlotArea;

                for (var i = 0; i < Items.Count; i++)
                {
                    var item = Items[i];

                    var d1 = Parent.FromLocalToAbsolute(new Point2D(item.Begin, item.LocalPosition.Y)).X;
                    var d2 = Parent.FromLocalToAbsolute(new Point2D(item.End, item.LocalPosition.Y)).X;

                    var widthX = d2 - d1;

                    if (widthX == 0.0)
                    {
                        widthX = 10;
                        //return;
                    }

                    var p0 = new Point2D(-widthX / 2.0, -heightY / 2.0);
                    var p1 = new Point2D(widthX / 2.0, heightY / 2.0);
                
                    var offset = Parent.WindowOffset;

                    var p = Parent.FromLocalToScreen(item.LocalPosition);
                                        
                    var rectangle = RectD.FromPoints(
                        p0.X - offset.X + p.X, 
                        p0.Y + offset.Y + p.Y, 
                        p1.X - offset.X + p.X, 
                        p1.Y + offset.Y + p.Y);
                    
                    MyRectList.Add(rectangle);
                }
            }   
        }
        
        public RectD MyClippingRect { get; private set; }
        public List<RectD> MyRectList { get; private set; } = new List<RectD>();

        public override void MyOnRender()
        {
            MyRender?.Invoke(this, EventArgs.Empty);
        }
    }
}
