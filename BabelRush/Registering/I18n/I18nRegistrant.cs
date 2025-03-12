using System;
using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.SourceTakers;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering.I18n;

// ReSharper disable once InconsistentNaming
public abstract class I18nRegistrant<TItem>
{
    protected II18nRegTarget<TItem>? Register { get; private set; }
    public void AcceptRegister(II18nRegTarget<TItem> register) => Register = register;
}

// ReSharper disable once InconsistentNaming
public class I18nRegistrant<TSource, TModel, TItem>(IRegisterDoneEventSource registerDoneEventSource)
    : I18nRegistrant<TItem>, II18nSourceTakerFactory<TSource>
    where TModel : IModel<TSource, TItem>
{
    public IEnumerable<(string Local, ISourceTaker<TSource> SourceTaker)> InitializeRegistration(string local)
    {
        List<(string Local, ISourceTaker<TSource> SourceTaker)> registrants = [];
        // ReSharper disable once ConvertToLocalFunction
        Func<string, IRegistrant<TItem>> registrantCreator = l =>
        {
            var result = new RegistrantSourceTaker<TSource, TModel, TItem>();
            registrants.Add((l, result));
            return result;
        };

        Register?.UpdateLocal(local, registrantCreator, registerDoneEventSource);
        return registrants;
    }

    public IEnumerable<(string Local, ISourceTaker<TSource> SourceTaker)> CreateSourceTakers(string local) => InitializeRegistration(local);
}