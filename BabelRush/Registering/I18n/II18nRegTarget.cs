using System;

using BabelRush.Data;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public interface II18nRegTarget<in TItem>
{
    void UpdateLocal(string local, Func<string, IRegistrant<RegKey, TItem>> registrantCreator);
}