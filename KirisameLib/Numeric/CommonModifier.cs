namespace KirisameLib.Numeric;

public class CommonModifier(IModifierType type, double value) : IModifier
{
    public IModifierType Type { get; } = type;
    IModifierType IModifier.Type => Type;
    public double Value { get; set; } = value;


    public void Modify(ref double value)
    {
        Type.Modify(Value, ref value);
    }
}