using KirisameY.Numeric;

namespace BabelRushR.Core.Numeric.CommonModifiers;

public class CommonLimitModifier(double min = double.NegativeInfinity, double max = double.PositiveInfinity) : INumericModifier<CommonNumericModifyOrder>
{
    public void ModifyValue(ref double value)
    {
        value = Math.Clamp(value, min, max);
    }

    public event EventHandler? Updated
    {
        add { }
        remove { }
    }
    public CommonNumericModifyOrder Order => CommonNumericModifyOrder.Limit;
}

public class MutableLimitModifier(double min = double.NegativeInfinity, double max = double.PositiveInfinity) : INumericModifier<CommonNumericModifyOrder>
{
    public double MinValue
    {
        get;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (field == value) return;
            field = value;
            Updated?.Invoke(this, EventArgs.Empty);
        }
    } = min;

    public double MaxValue
    {
        get;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (field == value) return;
            field = value;
            Updated?.Invoke(this, EventArgs.Empty);
        }
    } = max;

    public void ModifyValue(ref double value)
    {
        value = Math.Clamp(value, MinValue, MaxValue);
    }

    public event EventHandler? Updated;
    public CommonNumericModifyOrder Order => CommonNumericModifyOrder.Limit;
}