using System;

using BabelRush.Registering;

using KirisameLib.Data.Registers;
using KirisameLib.Event;

namespace BabelRush.I18n;

// ReSharper disable once InconsistentNaming
public abstract class DynamicI18nItem
{
    #region Factory

    public static DynamicI18nItem<TItem> Create<TItem>(Func<TItem> getValue) where TItem : notnull =>
        new(_ => true, _ => getValue());

    public static DynamicI18nItem<TItem> Create<TKey, TItem>(IRegister<TKey, TItem> register, TKey key) where TKey : notnull where TItem : notnull =>
        new(_ => true, _ => register[key]);

    public static DynamicI18nItem<TResult> Create<TKey, TReg, TResult>(IRegister<TKey, TReg> register, TKey key, Func<TResult?, TReg, TResult> updater)
        where TKey : notnull where TResult : notnull
    {
        TReg? cache = default(TReg);
        return new DynamicI18nItem<TResult>(_ => !Equals(cache, register[key]), pre =>
        {
            cache = register[key];
            return updater.Invoke(pre, cache);
        });
    }

    #endregion
}

// ReSharper disable once InconsistentNaming
public class DynamicI18nItem<T>(Func<T?, bool> checkDirty, Func<T?, T> updateValue) : DynamicI18nItem where T : notnull
{
    private string? _cachedLocal;
    private T? _cachedValue;

    public T Get()
    {
        if (_cachedLocal == DynamicI18nItemHelper.CurrentLocal) return _cachedValue ??= updateValue(_cachedValue);
        _cachedLocal = DynamicI18nItemHelper.CurrentLocal;

        if (!checkDirty(_cachedValue)) return _cachedValue ??= updateValue(_cachedValue);
        return _cachedValue = updateValue(_cachedValue);
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