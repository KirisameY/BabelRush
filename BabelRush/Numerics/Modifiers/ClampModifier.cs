using System.Numerics;

using BabelRush.Utils;

namespace BabelRush.Numerics.Modifiers;

public class ClampModifier<T>(T? min, T? max) : NumericModifier<T>(ModifierPriority.Clamp) where T : struct, INumber<T>
{
    public T? Min => min;
    public T? Max => max;

    public override T Modify(T value) => MathUtils.ClampNullable(value, min, max);
}

public class DynamicClampModifier<T>(T? min = null, T? max = null) : NumericModifier<T>(ModifierPriority.Clamp) where T : struct, INumber<T>
{
    public T? Min
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            RaiseModifierChangedEvent();
        }
    } = min;
    public T? Max
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            RaiseModifierChangedEvent();
        }
    } = max;
    public override T Modify(T value) => MathUtils.ClampNullable(value, Min, Max);
}