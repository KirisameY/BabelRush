using System;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegister<TItem>(Func<string, TItem> fallback, Func<string, bool> fallbackExists, string? defaultLocal = null) : IRegister<TItem>
{
    public I18nRegister(IRegister<TItem> fallbackRegister) : this(fallbackRegister.GetItem, fallbackRegister.ItemRegistered) { }
    public I18nRegister(Func<string, TItem> fallback) : this(fallback, _ => false) { }
    public I18nRegister(Func<string, TItem> fallback, string? defaultLocal) : this(fallback, _ => false, defaultLocal) { }

    private bool _initialized = false;
    private IRegister<TItem>? _defaultLocalRegister;
    private IRegister<TItem>? _innerRegister;

    public void UpdateLocal(string local, Func<string, IRegistrant<TItem>> registrantCreator, IRegisterDoneEventSource registerDoneEventSource)
    {
        if (defaultLocal is not null)
        {
            _defaultLocalRegister ??= new RegisterBuilder<TItem>()
                                     .WithFallback(fallback)
                                     .AddRegistrant(registrantCreator.Invoke(defaultLocal))
                                     .WithRegisterDoneEventSource(registerDoneEventSource)
                                     .Build();
        }

        if (local == defaultLocal) _innerRegister = _defaultLocalRegister;
        else
        {
            var realFallback = defaultLocal is null ? fallback : _defaultLocalRegister!.GetItem;
            _innerRegister = new RegisterBuilder<TItem>()
                            .WithFallback(fallback)
                            .AddRegistrant(registrantCreator.Invoke(local))
                            .WithRegisterDoneEventSource(registerDoneEventSource)
                            .Build();
        }

        _initialized = true;
    }

    public TItem GetItem(string id)
    {
        if (!_initialized) throw new InvalidOperationException("I18nRegister is not initialized.");
        return _innerRegister!.GetItem(id);
    }

    public bool ItemRegistered(string id)
    {
        if (!_initialized) throw new InvalidOperationException("I18nRegister is not initialized.");
        return _innerRegister!.ItemRegistered(id) || (_defaultLocalRegister?.ItemRegistered(id) ?? false) || fallbackExists(id);
    }
}