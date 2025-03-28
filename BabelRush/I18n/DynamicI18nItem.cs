using System;

using BabelRush.Registering;

using KirisameLib.Data.Registers;
using KirisameLib.Event;

namespace BabelRush.I18n;

// ReSharper disable once InconsistentNaming
public abstract class DynamicI18nItem
{
    #region Factory

    public static DynamicI18nItem<TItem> Create<TItem>(Func<TItem> getValue) => new(getValue);

    public static DynamicI18nItem<TItem> Create<TKey, TItem>(IRegister<TKey, TItem> register, TKey key) where TKey : notnull => new(() => register[key]);

    public static DynamicI18nItem<TResult> Create<TKey, TReg, TResult>(IRegister<TKey, TReg> register, TKey key, Func<TReg, TResult> selector)
        where TKey : notnull => new(() => selector.Invoke(register[key]));

    #endregion
}

// ReSharper disable once InconsistentNaming
public class DynamicI18nItem<T>(Func<T> getValue) : DynamicI18nItem
{
    private string? _cachedLocal;
    private T? _cachedValue;

    public T Get()
    {
        if (_cachedLocal == DynamicI18nItemHelper.CurrentLocal) return _cachedValue ??= getValue();

        _cachedLocal = DynamicI18nItemHelper.CurrentLocal;
        return _cachedValue = getValue();
    }
}

// ReSharper disable once InconsistentNaming
[EventHandlerContainer]
internal static partial class DynamicI18nItemHelper
{
    internal static string? CurrentLocal;

    [EventHandler([Game.LoadEventGroup])]
    private static void OnLocalChanged(LocalRegisterStartEvent e) => CurrentLocal = e.Local;
}