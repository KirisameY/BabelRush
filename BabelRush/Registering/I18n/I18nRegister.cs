using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegister<TItem>(Func<string, TItem> fallback, Func<string, bool> fallbackExists,
                                 Func<IEnumerable<KeyValuePair<string, TItem>>> getFallbacks, string? defaultLocal = null)
    : IEnumerableRegister<TItem>, II18nRegTarget<TItem>
{
    public I18nRegister(IEnumerableRegister<TItem> fallbackRegister) :
        this(fallbackRegister.GetItem, fallbackRegister.ItemRegistered, () => fallbackRegister) { }

    public I18nRegister(Func<string, TItem> fallback) : this(fallback, _ => false, () => []) { }

    public I18nRegister(Func<string, TItem> fallback, string? defaultLocal) : this(fallback, _ => false, () => [], defaultLocal) { }

    private bool _initialized = false;
    private string? _currentLocal;
    private IEnumerableRegister<TItem>? _defaultLocalRegister;

    [field: AllowNull, MaybeNull]
    private IEnumerableRegister<TItem> InnerRegister
    {
        get => _initialized ? field! : throw new InvalidOperationException("I18nRegister is not initialized.");
        set;
    }


    public void UpdateLocal(string local, Func<string, IRegistrant<TItem>> registrantCreator, IRegisterDoneEventSource registerDoneEventSource)
    {
        if (_currentLocal == local && _initialized) return;
        _currentLocal = local;

        if (defaultLocal is not null)
        {
            _defaultLocalRegister ??= new RegisterBuilder<TItem>()
                                     .WithFallback(fallback)
                                     .AddRegistrant(registrantCreator.Invoke(defaultLocal))
                                     .WithRegisterDoneEventSource(registerDoneEventSource)
                                     .Build();
        }

        if (local == defaultLocal) InnerRegister = _defaultLocalRegister!;
        else
        {
            var realFallback = defaultLocal is null ? fallback : _defaultLocalRegister!.GetItem;
            InnerRegister = new RegisterBuilder<TItem>()
                           .WithFallback(realFallback)
                           .AddRegistrant(registrantCreator.Invoke(local))
                           .WithRegisterDoneEventSource(registerDoneEventSource)
                           .Build();
        }

        _initialized = true;
    }

    public TItem GetItem(string id) => InnerRegister!.GetItem(id);

    public bool ItemRegistered(string id) =>
        InnerRegister!.ItemRegistered(id) || (_defaultLocalRegister?.ItemRegistered(id) ?? false) || fallbackExists(id);

    public IEnumerator<KeyValuePair<string, TItem>> GetEnumerator() =>
        InnerRegister!
           .Concat(getFallbacks())
           .GroupBy(p => p.Key, p => p.Value)
           .ToImmutableDictionary(g => g.Key, g => g.First())
           .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => InnerRegister!.Concat(getFallbacks()).GroupBy(p => p.Key).Count();

    public bool TryGetValue(string key, out TItem value)
    {
        value = GetItem(key);
        return ItemRegistered(key);
    }

    public TItem this[string key] => GetItem(key);
    public IEnumerable<string> Keys => InnerRegister.Concat(getFallbacks()).Select(p => p.Key).Distinct();
    public IEnumerable<TItem> Values => this.Select(p => p.Value);
}