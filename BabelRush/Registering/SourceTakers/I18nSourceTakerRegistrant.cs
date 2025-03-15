using System;
using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.I18n;

using KirisameLib.Data.Registering;

namespace BabelRush.Registering.SourceTakers;

// ReSharper disable once InconsistentNaming
public class I18nSourceTakerRegistrant<TSource, TModel, TItem>(IRegisterDoneEventSource registerDoneEventSource)
    : II18nRegistrant<TItem>, II18nSourceTakerFactory<TSource>
    where TModel : IModel<TSource, TItem>
{
    private II18nRegTarget<TItem>? _register;
    public void AcceptTarget(II18nRegTarget<TItem> target) => _register = target;

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

        _register?.UpdateLocal(local, registrantCreator);
        return registrants;
    }

    public IEnumerable<(string Local, ISourceTaker<TSource> SourceTaker)> CreateSourceTakers(string local) => InitializeRegistration(local);
}