using System;
using System.Numerics;

namespace BabelRush.Numerics.Modifiers;

public abstract class NumericModifier<T>(ModifierPriority priority) where T : struct, INumber<T>
{
    public ModifierPriority Priority => priority;

    public abstract T Modify(T value);

    public event EventHandler? ModifierChanged;

    protected void RaiseModifierChangedEvent() => ModifierChanged?.Invoke(this, EventArgs.Empty);
}