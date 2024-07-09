namespace KirisameLib.Numeric;

public class CommonModifierType<TBase, TMod>(int order, TMod defaultValue, IModifierType<TBase, TMod>.ModifyFunc modify)
    : IModifierType<TBase, TMod>
{
    public int Order { get; } = order;

    public int CompareTo(IModifierType<TBase>? other) => Order.CompareTo(other?.Order);

    public IModifier<TBase> NewInstance() => new CommonModifier<TBase, TMod>(this, defaultValue);

    public IModifierType<TBase, TMod>.ModifyFunc Modify { get; } = modify;
}