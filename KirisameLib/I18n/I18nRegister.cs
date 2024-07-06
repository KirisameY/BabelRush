using System.Diagnostics.CodeAnalysis;

using KirisameLib.Register;

namespace KirisameLib.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegister<T>(string defaultLocal, IRegister<T> defaultRegister) : IRegister<T>

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
        if (GetItemInLocal(LocalSettings.Local, id, out var item)) return item;
        if (GetItemInLocal(DefaultLocal,        id, out item)) return item;
        return DefaultRegister.GetItem(id);
    }

    private bool GetItemInLocal(string local, string id, [NotNullWhen(true)] out T? item)
    {
        item = default(T);
        if (!LocalRegisterDict.TryGetValue(local, out var regDict))
            return false;
        return regDict.TryGetValue(id, out item);
    }
}