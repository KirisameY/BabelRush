using System;

using KirisameLib.Data.Registers;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegister<TItem>(Func<string, TItem> fallback, Func<string, bool> fallbackExists, string? defaultLocal = null) : IRegister<TItem>
{
    public I18nRegister(IRegister<TItem> fallbackRegister) : this(fallbackRegister.GetItem, fallbackRegister.ItemRegistered) { }
    public I18nRegister(Func<string, TItem> fallback) : this(fallback, _ => false) { }
    public I18nRegister(Func<string, TItem> fallback, string? defaultLocal) : this(fallback, _ => false, defaultLocal) { }


    public TItem GetItem(string id)
    {
        throw new NotImplementedException();
    }

    public bool ItemRegistered(string id)
    {
        throw new NotImplementedException();
    }
}