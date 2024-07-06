using KirisameLib.Logging;

namespace KirisameLib.Register;

public class CommonRegister<T>(Func<string, T> defaultGetter) : IRegister<T>
{
    private Dictionary<string, T> RegDict { get; } = [];
    private Func<string, T> DefaultGetter { get; } = defaultGetter;

    public bool RegisterItem(string id, T item) => RegDict.TryAdd(id, item);

    public T GetItem(string id)
    {
        if (RegDict.TryGetValue(id, out var item))
            return item;
        
        return DefaultGetter(id);
    }
}