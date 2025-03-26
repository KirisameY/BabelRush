using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegister<TItem>(Func<RegKey, TItem> fallback, Func<RegKey, bool> fallbackExists,
                                 Func<IEnumerable<KeyValuePair<RegKey, TItem>>> getFallbacks,
                                 IRegisterDoneEventSource registerDoneEventSource, string? defaultLocal = null)
    : IEnumerableRegister<RegKey, TItem>, II18nRegTarget<TItem>
{
    private enum State
    {
        Uninitialized,
        Registering,
        Ready
    }


    #region Fields

    private string? _currentLocal;
    private State _state = State.Uninitialized;

    private MoltenRegister<RegKey, TItem>? _defaultLocalRegisterMolten;
    private FrozenRegister<RegKey, TItem>? _defaultLocalRegister;

    private MoltenRegister<RegKey, TItem>? _innerRegisterMolten;
    private FrozenRegister<RegKey, TItem>? _innerRegister;

    private FrozenRegister<RegKey, TItem> InnerRegister =>
        _state is State.Ready ? _innerRegister! : throw new InvalidOperationException("I18nRegister is not ready.");

    #endregion


    public void UpdateLocal(string local, Func<string, IRegistrant<RegKey, TItem>> registrantCreator)
    {
        if (_currentLocal == local && _state is State.Ready) return;

        // Begin registration
        if (_state is not State.Registering)
        {
            _currentLocal = local;
            _state = State.Registering;
            _innerRegister = null;

            // Initialize default local register
            if (defaultLocal is not null && _defaultLocalRegister is null)
            {
                _defaultLocalRegisterMolten = new MoltenRegister<RegKey, TItem>(fallback);
                registerDoneEventSource.RegisterDone += () =>
                {
                    _defaultLocalRegister = _defaultLocalRegisterMolten!.Freeze();
                    _defaultLocalRegisterMolten = null;
                };
            }

            // Initialize inner register
            if (local != defaultLocal)
            {
                var realFallback = defaultLocal is null ? fallback : id => _defaultLocalRegister!.GetItem(id);
                _innerRegisterMolten = new MoltenRegister<RegKey, TItem>(realFallback);
                registerDoneEventSource.RegisterDone += () =>
                {
                    _innerRegister = _innerRegisterMolten!.Freeze();
                    _innerRegisterMolten = null;
                };
            }
            else registerDoneEventSource.RegisterDone += () => _innerRegister = _defaultLocalRegister!;

            // Set state done
            registerDoneEventSource.RegisterDone += () => _state = State.Ready;
        }

        if (_currentLocal != local) throw new InvalidOperationException("I18nRegister is in registering state with different local.");

        // during registration
        if (_defaultLocalRegisterMolten is not null) registrantCreator(defaultLocal!).AcceptTarget(_defaultLocalRegisterMolten);
        if (_innerRegisterMolten is not null) registrantCreator(local).AcceptTarget(_innerRegisterMolten);
    }


    // Reading
    public TItem GetItem(RegKey id) => InnerRegister.GetItem(id);

    public bool ItemRegistered(RegKey id) =>
        InnerRegister.ItemRegistered(id) || (_defaultLocalRegister?.ItemRegistered(id) ?? false) || fallbackExists(id);

    public IEnumerator<KeyValuePair<RegKey, TItem>> GetEnumerator() =>
        InnerRegister
           .Concat(getFallbacks())
           .GroupBy(p => p.Key, p => p.Value)
           .ToImmutableDictionary(g => g.Key, g => g.First())
           .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => InnerRegister.Concat(getFallbacks()).GroupBy(p => p.Key).Count();

    public bool TryGetValue(RegKey key, out TItem value)
    {
        value = GetItem(key);
        return ItemRegistered(key);
    }

    public TItem this[RegKey key] => GetItem(key);
    public IEnumerable<RegKey> Keys => InnerRegister.Concat(getFallbacks()).Select(p => p.Key).Distinct();
    public IEnumerable<TItem> Values => this.Select(p => p.Value);
}