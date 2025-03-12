using System;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public interface II18nRegTarget<TItem>
{
    void UpdateLocal(string local, Func<string, IRegistrant<TItem>> registrantCreator, IRegisterDoneEventSource registerDoneEventSource);
}