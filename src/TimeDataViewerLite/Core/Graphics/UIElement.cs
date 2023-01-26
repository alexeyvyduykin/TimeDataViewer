namespace TimeDataViewerLite.Core;

public abstract class UIElement : SelectableElement
{
    public HitTestResult? HitTest(HitTestArguments args)
    {
        return HitTestOverride(args);
    }

    protected virtual HitTestResult? HitTestOverride(HitTestArguments args)
    {
        return null;
    }
}
