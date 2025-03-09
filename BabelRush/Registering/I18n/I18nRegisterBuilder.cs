using System;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public class I18nRegisterBuilder<TItem>
{
    private Func<string, TItem>? _fallback;
}