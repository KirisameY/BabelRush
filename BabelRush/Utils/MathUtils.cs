using System.Numerics;

using KirisameLib;

namespace BabelRush.Utils;

public static class MathUtils
{
    public static T ClampNullable<T>(T value, T? min, T? max) where T : struct, INumber<T> => (min, max) switch
    {
        (null, null)     => value,
        (null, not null) => MathSpark.Min(value, max.Value),
        (not null, null) => MathSpark.Max(value, min.Value),
        _                => MathSpark.Clamp(value, min.Value, max.Value)
    };
}