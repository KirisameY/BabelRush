namespace KirisameLib.Register;

public interface IRegister<T>
{
    bool RegisterItem(string id, T item);
    T GetItem(string id);
}