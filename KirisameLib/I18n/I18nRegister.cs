using KirisameLib.Register;

namespace KirisameLib.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegister<T>(string defaultLocal, IRegister<T> defaultRegister) : IRegister<T>
    where T : class
{
    private Dictionary<string, Dictionary<string, T>> LocalRegisterDict { get; } = [];
    private string DefaultLocal { get; } = defaultLocal;
    private IRegister<T> DefaultRegister { get; } = defaultRegister;

    public bool RegisterLocalizedItem(string local, string id, T item)
    {
        LocalRegisterDict.TryGetValue(local, out var regDict);
        if (regDict is null)
        {
            regDict = [];
            LocalRegisterDict.Add(local, regDict);
        }

        return regDict.TryAdd(id, item);
    }

    public bool RegisterItem(string id, T item) => DefaultRegister.RegisterItem(id, item);

    public T GetItem(string id)
    {
        var result = GetItemInLocal(LocalSettings.Local, id);
        result ??= GetItemInLocal(DefaultLocal, id);
        result ??= DefaultRegister.GetItem(id);
        return result;
    }

    private T? GetItemInLocal(string local, string id)
    {
        LocalRegisterDict.TryGetValue(LocalSettings.Local, out var regDict);
        return regDict?.GetValueOrDefault(id);
    }
}