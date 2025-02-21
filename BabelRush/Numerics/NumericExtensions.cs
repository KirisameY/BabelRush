using System.Numerics;

namespace BabelRush.Numerics;

public static class NumericExtensions
{
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