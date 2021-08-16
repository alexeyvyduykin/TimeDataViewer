namespace TimeDataViewer.Shapes
{
    //public abstract class BaseShape : Control
    //{   
    //    private Core.TimelineItem? _marker;
    //    private Timeline? _scheduler;

    //    public BaseShape()
    //    {
    //        DataContextProperty.Changed.AddClassHandler<BaseShape>((d, e) => d.MarkerChanged(e));
    //    }

    //    public Timeline? Scheduler => _scheduler;

    //    public Core.TimelineItem? Marker => _marker;

    //    private void MarkerChanged(AvaloniaPropertyChangedEventArgs e)
    //    {
    //        if (e.NewValue is Core.TimelineItem marker)
    //        {
    //            _marker = marker;        
    //        }
    //    }

    //    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    //    {
    //        base.OnAttachedToLogicalTree(e);

    //        ILogical control = this;

    //        while((control.LogicalParent is Timeline) == false)
    //        {
    //            control = control.LogicalParent;
    //        }

    //        _scheduler = (Timeline)control.LogicalParent;
    //    }
    //}
}
