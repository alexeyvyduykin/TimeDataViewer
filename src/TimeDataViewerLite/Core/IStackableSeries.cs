namespace TimeDataViewerLite.Core;

public interface IStackableSeries
{
    bool IsStacked { get; }

    string StackGroup { get; set; }
}
