namespace KirisameLib.Numeric;

public class CommonModifierType(int order, IModifierType.ModifyFunc modify) : IModifierType
{
    public int Order { get; } = order;

    public int CompareTo(IModifierType? other) => Order.CompareTo(other?.Order);

    public IModifier NewInstance(double value) => new CommonModifier(this, value);

    public IModifierType.ModifyFunc Modify { get; } = modify;
}