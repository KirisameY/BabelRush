using System.Numerics;

namespace BabelRush.Numerics.Modifiers;

public abstract class NumericModifier<T> where T : struct, INumber<T>
{
    public abstract ModifierPriority Priority { get; }

    protected internal abstract void Modify(ref T value);
}