namespace KirisameLib.Numeric;

public class CommonNumeric(double baseValue) : INumeric
{
    public double BaseValue { get; set; } = baseValue;
    public double FinalValue => CalculateFinalValue();

    private double CalculateFinalValue()
    {
        var value = BaseValue;
        foreach (var modifierList in ModifierLists.Values)
        {
            modifierList.ForEach(modifier => modifier.Modify(ref value));
        }

        return value;
    }

    private SortedList<IModifierType, List<IModifier>> ModifierLists { get; } = [];

    public void AddModifier(IModifier modifier)
    {
        if (!ModifierLists.TryGetValue(modifier.Type, out var list))
        {
            list = [];
            ModifierLists.Add(modifier.Type, list);
        }

        list.Add(modifier);
    }

    public void RemoveModifier(IModifier modifier)
    {
        if (!ModifierLists.TryGetValue(modifier.Type, out var list)) return;
        list.Remove(modifier);
    }
}