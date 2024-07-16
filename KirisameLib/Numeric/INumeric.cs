namespace KirisameLib.Numeric;

public interface INumeric<T>
{
    T BaseValue { get; set; }
    T FinalValue { get; }

    void AddModifier(IModifier<T> modifier);
    void RemoveModifier(IModifier<T> modifier);
}