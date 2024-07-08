namespace KirisameLib.Numeric;

public interface IModifierType<T> : IComparable<IModifierType<T>>
{
    IModifier<T> NewInstance();
}

public interface IModifierType<TBase, TMod> : IModifierType<TBase>
{
    delegate void ModifyFunc(TMod mod, ref TBase value);

    ModifyFunc Modify { get; }
}