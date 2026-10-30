using KirisameY.Numeric;

namespace BabelRushR.Core.Numeric.CommonModifiers;

public class CommonBaseAdditionModifier(double value) : INumericModifier<CommonNumericModifyOrder>
{
    public void ModifyValue(ref double targetValue)
    {
        targetValue += value;
    }

    public event EventHandler? Updated
    {
        add { }
        remove { }
    }
    public CommonNumericModifyOrder Order => CommonNumericModifyOrder.BaseAdd;
}

public class MutableCommonBaseAdditionModifier(double value = 0) : INumericModifier<CommonNumericModifyOrder>
{
    public double Value
    {
        get;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (field == value) return;
            field = value;
            Updated?.Invoke(this, EventArgs.Empty);
        }
    } = value;

    public void ModifyValue(ref double targetValue)
    {
        targetValue += Value;
    }

    public event EventHandler? Updated;
    public CommonNumericModifyOrder Order => CommonNumericModifyOrder.BaseAdd;
}