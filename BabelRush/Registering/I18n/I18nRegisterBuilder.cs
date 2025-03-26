using System;
using System.Collections.Generic;

using BabelRush.Data;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;
using KirisameLib.Extensions;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegisterBuilder<TItem>
{
    private (Func<RegKey, TItem> Get, Func<RegKey, bool> Exists, Func<IEnumerable<KeyValuePair<RegKey, TItem>>> enumerate)? _fallback;
    private IRegisterDoneEventSource? _registerDoneEventSource;
    private string? _defaultLocal;
    private readonly HashSet<II18nRegistrant<TItem>> _registrants = [];


    public I18nRegisterBuilder<TItem> WithFallback(Func<RegKey, TItem> fallback) => WithFallback(fallback, _ => false, () => []);

    public I18nRegisterBuilder<TItem> WithFallback(TItem fallback) => WithFallback(_ => fallback, _ => false, () => []);

    public I18nRegisterBuilder<TItem> WithFallback(IEnumerableRegister<RegKey, TItem> fallbackRegister) =>
        WithFallback(fallbackRegister.GetItem, fallbackRegister.ItemRegistered, () => fallbackRegister);

    public I18nRegisterBuilder<TItem> WithFallback(Func<RegKey, TItem> fallback, Func<RegKey, bool> exists, Func<IEnumerable<KeyValuePair<RegKey, TItem>>> enumerate)
    {
        _fallback = (fallback, exists, enumerate);
        return this;
    }

    public I18nRegisterBuilder<TItem> WithRegisterDoneEventSource(IRegisterDoneEventSource registerDoneEventSource)
    {
        _registerDoneEventSource = registerDoneEventSource;
        return this;
    }

    public I18nRegisterBuilder<TItem> WithDefaultLocal(string defaultLocal)
    {
        _defaultLocal = defaultLocal;
        return this;
    }

    public I18nRegisterBuilder<TItem> WithRegistrant(II18nRegistrant<TItem> registrant)
    {
        _registrants.Add(registrant);
        return this;
    }

    public I18nRegister<TItem> Build()
    {
        if (_fallback is null) throw new InvalidOperationException("Fallback is not set.");
        if (_registerDoneEventSource is null) throw new InvalidOperationException("RegisterDoneEventSource is not set.");
        var (fallback, fallbackExists, enumerate) = _fallback.Value;

        var register = new I18nRegister<TItem>(fallback, fallbackExists, enumerate, _registerDoneEventSource, _defaultLocal);
        _registrants.ForEach(r => r.AcceptTarget(register));
        return register;
    }
}