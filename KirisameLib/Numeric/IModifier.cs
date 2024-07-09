namespace KirisameLib.Numeric;

public interface IModifier
{
    IModifierType Type { get; }
    double Value { get; set; }
    void Modify(ref double value);
}