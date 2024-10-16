namespace BabelRush.Data;

public abstract class DynamicProvider<T>
{
    //Abstracts
    protected abstract bool DependencyChanged();
    protected abstract T GetItem();


    //Cache
    private T? _cached;


    //Public Methods
    public T Get()
    {
        if (_cached is null || DependencyChanged())
            _cached = GetItem();
        return _cached;
    }
}