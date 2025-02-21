using System.Numerics;

using BabelRush.Utils;

namespace BabelRush.Numerics;

public class Numeric<T>(T value = default) where T : struct, INumber<T>
{
    #region Properties

    public T BaseValue
    {
        get;
        set
        {
            field = value;
            OnBaseValueUpdated();
        }
    } = value;

    public (T? Min, T? Max) Clamp
    {
        get;
        set
        {
            field = value;
            OnFinalValueUpdated();
        }
    } = (null, null);

    public T FinalValue => MathUtils.ClampNullable(BaseValue, Clamp.Min, Clamp.Max);

    #endregion


    public static implicit operator T(Numeric<T> numeric) => numeric.FinalValue;


    #region Events

    public event NumericUpdatedHandler<T> BaseValueUpdated = (numeric, _, _) => numeric.OnFinalValueUpdated();
    public event NumericUpdatedHandler<T>? FinalValueUpdated;

    private T _prevBaseValue, _prevFinalValue;

    private void OnBaseValueUpdated()
    {
        if (_prevBaseValue == BaseValue) return;
        BaseValueUpdated.Invoke(this, _prevBaseValue, BaseValue);
        _prevBaseValue = BaseValue;
    }

    private void OnFinalValueUpdated()
    {
        var newValue = FinalValue;
        if (_prevFinalValue == newValue) return;
        FinalValueUpdated?.Invoke(this, _prevFinalValue, newValue);
        _prevFinalValue = newValue;
    }

    #endregion
}

public delegate void NumericUpdatedHandler<T>(Numeric<T> numeric, T oldValue, T newValue) where T : struct, INumber<T>;