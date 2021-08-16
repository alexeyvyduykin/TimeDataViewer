using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace TimeDataViewer.Views
{
    public class CustomItemTemplate : IDataTemplate
    {
        public IControl Build(object param)
        {
            if (param is Core.TimelineItem ival)
            {
                //var shape = ival.SeriesControl.CreateIntervalShape(/*ival*/);

                //if(shape is IControl)
                //{
                //    return (IControl)shape;
                //}

                //return new IntervalVisual() { DataContext = param };
            }

            return new TextBlock() { Text = "Template not find" };
        }

        public bool Match(object data)
        {
            return (data is Core.TimelineItem);
        }
    }
}
