using System;
using System.Collections.Generic;

using KirisameLib.Data.Registers;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegisterBuilder<TItem>
{
    private (Func<string, TItem> Get, Func<string, bool> Exists, Func<IEnumerable<KeyValuePair<string, TItem>>> enumerate)? _fallback;
    private string? _defaultLocal;
    private I18nRegistrant<TItem>? _registrant;


    public I18nRegisterBuilder<TItem> WithFallback(Func<string, TItem> fallback) => WithFallback(fallback, _ => false, () => []);

    public I18nRegisterBuilder<TItem> WithFallback(TItem fallback) => WithFallback(_ => fallback, _ => false, () => []);

    public I18nRegisterBuilder<TItem> WithFallback(IEnumerableRegister<TItem> fallbackRegister) =>
        WithFallback(fallbackRegister.GetItem, fallbackRegister.ItemRegistered, () => fallbackRegister);

    public I18nRegisterBuilder<TItem> WithFallback(Func<string, TItem> fallback, Func<string, bool> exists, Func<IEnumerable<KeyValuePair<string, TItem>>> enumerate)
    {
        _fallback = (fallback, exists, enumerate);
        return this;
    }

    public I18nRegisterBuilder<TItem> WithDefaultLocal(string defaultLocal)
    {
        _defaultLocal = defaultLocal;
        return this;
    }

    public I18nRegisterBuilder<TItem> WithRegistrant(I18nRegistrant<TItem> registrant)
    {
        _registrant = registrant;
        return this;
    }

    public I18nRegister<TItem> Build()
    {
        if (_fallback is null) throw new InvalidOperationException("Fallback is not set.");
        var (fallback, fallbackExists, enumerate) = _fallback.Value;

        var register = new I18nRegister<TItem>(fallback, fallbackExists, enumerate, _defaultLocal);
        _registrant?.AcceptRegister(register);
        return register;
    }
}