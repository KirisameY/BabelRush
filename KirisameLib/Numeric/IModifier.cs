namespace KirisameLib.Numeric;

public interface IModifier<T>
{
    IModifierType<T> Type { get; }
    
    void Modify(ref T value);
}

public interface IModifier<TBase, TMod> : IModifier<TBase>
{
    new IModifierType<TBase, TMod> Type { get; }
    TMod Value { get; set; }
}