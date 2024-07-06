namespace KirisameLib.Register;

public interface IRegister<T>
    where T : class
{
    bool RegisterItem(string id, T item);
    T GetItem(string id);
}