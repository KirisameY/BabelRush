namespace KirisameLib.Numeric;

public class CommonModifier<TBase, TMod>(IModifierType<TBase, TMod> type, TMod defaultValue) : IModifier<TBase, TMod>
{
    public IModifierType<TBase, TMod> Type { get; } = type;
    IModifierType<TBase> IModifier<TBase>.Type => Type;
    public TMod Value { get; set; } = defaultValue;


    public void Modify(ref TBase value)
    {
        Type.Modify(Value, ref value);
    }
}