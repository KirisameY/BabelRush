using System.Numerics;

namespace KirisameLib;

public static class MathSpark
{
    public static T Max<T>(T a, T b)
        where T : INumber<T> =>
        a > b ? a : b;

    public static T Min<T>(T a, T b)
        where T : INumber<T> =>
        a < b ? a : b;

    public static T Clamp<T>(T value, T min, T max)
        where T : INumber<T> =>
        Max(Min(value, max), min);
}