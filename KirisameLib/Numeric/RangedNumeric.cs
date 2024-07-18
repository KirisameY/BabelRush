using System.Numerics;

namespace KirisameLib.Numeric;

public class RangedNumeric<T>(T minValue = default, T maxValue = default, T value = default) : NumericBase<T>
    where T : struct, INumber<T>
{
    public override T BaseValue { get; set; } = value;
    public override T FinalValue
    {
        get
        {
            var result = BaseValue;
            if (result < MinValue) result = MinValue;
            if (result > MaxValue) result = MaxValue;
            return result;
        }
    }

    public T MinValue { get; set; } = minValue;
    public T MaxValue { get; set; } = maxValue;
}