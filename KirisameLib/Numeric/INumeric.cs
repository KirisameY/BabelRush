namespace KirisameLib.Numeric;

public interface INumeric
{
    double BaseValue { get; set; }
    double FinalValue { get; }
    
    void AddModifier(IModifier modifier);
    void RemoveModifier(IModifier modifier);
}