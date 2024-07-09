namespace KirisameLib.Numeric;

public interface IModifierType : IComparable<IModifierType>
{
    delegate void ModifyFunc(double mod, ref double value);

    ModifyFunc Modify { get; }
    int Order { get; }
    IModifier NewInstance(double value);
}