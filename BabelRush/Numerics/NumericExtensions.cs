using System.Numerics;

using BabelRush.Numerics.Modifiers;

namespace BabelRush.Numerics;

public static class NumericExtensions
{
    public static Numeric<T> WithModifier<T>(this Numeric<T> numeric, NumericModifier<T> modifier)
        where T : struct, INumber<T>
    {
        numeric.AddModifier(modifier);
        return numeric;
    }

    public static Numeric<T> WithBaseValueUpdatedHandler<T>(this Numeric<T> numeric, NumericUpdatedHandler<T> handler)
        where T : struct, INumber<T>
    {
        numeric.BaseValueUpdated += handler;
        return numeric;
    }

    public static Numeric<T> WithFinalValueUpdatedHandler<T>(this Numeric<T> numeric, NumericUpdatedHandler<T> handler)
        where T : struct, INumber<T>
    {
        numeric.FinalValueUpdated += handler;
        return numeric;
    }
}