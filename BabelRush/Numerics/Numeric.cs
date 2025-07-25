using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

using BabelRush.Numerics.Modifiers;

using KirisameLib.Collections;

namespace BabelRush.Numerics;

public sealed class Numeric<T>(T value = default) where T : struct, INumber<T>
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

    private readonly SortedList<ModifierPriority, List<NumericModifier<T>>> _modifiers = [];

    [field: AllowNull, MaybeNull]
    public IReadOnlyList<NumericModifier<T>> Modifiers => field ??=
        new DynamicCombinedListView<NumericModifier<T>>(() => _modifiers.Values);

    public T FinalValue => Modifiers.Aggregate(BaseValue, (v, m) => m.Modify(v));

    #endregion


    #region Api

    public void AddModifier(NumericModifier<T> modifier)
    {
        if (!_modifiers.TryGetValue(modifier.Priority, out var list))
        {
            _modifiers[modifier.Priority] = list = [];
        }
        list.Add(modifier);
        modifier.ModifierChanged += OnModifierChanged;
    }

    public bool RemoveModifier(NumericModifier<T> modifier)
    {
        if (!_modifiers.TryGetValue(modifier.Priority, out var list)) return false;
        if (!list.Remove(modifier)) return false;
        modifier.ModifierChanged -= OnModifierChanged;
        return true;
    }

    public static implicit operator T(Numeric<T> numeric) => numeric.FinalValue;

    #endregion


    #region Events

    public event NumericUpdatedHandler<T> BaseValueUpdated = (numeric, _, _) => numeric.OnFinalValueUpdated();
    public event NumericUpdatedHandler<T>? FinalValueUpdated;

    private T _prevBaseValue, _prevFinalValue;

    private void OnBaseValueUpdated()
    {
        if (_prevBaseValue == BaseValue) return;
        BaseValueUpdated.Invoke(this, _prevBaseValue, BaseValue);
        _prevBaseValue = BaseValue;
        OnFinalValueUpdated();
    }

    private void OnFinalValueUpdated()
    {
        var newValue = FinalValue;
        if (_prevFinalValue == newValue) return;
        FinalValueUpdated?.Invoke(this, _prevFinalValue, newValue);
        _prevFinalValue = newValue;
    }

    private void OnModifierChanged(object? source, EventArgs args)
    {
        OnFinalValueUpdated();
    }

    #endregion
}

public delegate void NumericUpdatedHandler<T>(Numeric<T> numeric, T oldValue, T newValue) where T : struct, INumber<T>;