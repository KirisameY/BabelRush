using System;

using KirisameLib.Data.Register;

namespace BabelRush.Data;

public struct DynamicRef<T>(Func<T> getter)
{
    //Constructor
    public DynamicRef(IRegister<T> register, string id) : this(() => register.GetItem(id)) { }


    //Cache
    private T? _cached;


    //Public
    public T Get() => _cached = getter();
    public bool ValueChanged() => !Equals(_cached, getter());
}