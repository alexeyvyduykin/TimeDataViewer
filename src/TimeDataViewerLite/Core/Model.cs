namespace TimeDataViewerLite.Core;

public abstract partial class Model
{
    private readonly object _syncRoot = new();

    protected Model() { }

    public object SyncRoot => _syncRoot;
}
