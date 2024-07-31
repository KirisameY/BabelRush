using System;

namespace BabelRush.Actions;

public abstract record TargetPattern //仿rust式的枚举
{
    public sealed record None : TargetPattern;

    public sealed record Self : TargetPattern;

    public sealed record Any(TargetRange Range) : TargetPattern;

    public sealed record All(TargetRange Range) : TargetPattern;

    public static TargetPattern FromString(string str) => str.ToLower().Split('.') switch
    {
        ["none"]       => new None(),
        ["self"]       => new Self(),
        ["any", var r] => Enum.TryParse(r, true, out TargetRange range) ? new Any(range) : Default(),
        ["all", var r] => Enum.TryParse(r, true, out TargetRange range) ? new All(range) : Default(),
        _              => Default(),
    };

    private static None Default()
    {
        return new None();
    }
}

[Flags]
public enum TargetRange
{
    Friend = 1 << 0,
    Enemy = 1 << 1,
    All = Friend | Enemy,
}