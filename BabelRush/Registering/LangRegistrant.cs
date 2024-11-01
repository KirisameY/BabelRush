using System;
using System.Collections.Generic;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Model;
using KirisameLib.Data.Registration;

namespace BabelRush.Registering;

public class LangRegistrant(IRegistrant<IDictionary<string, object>> innerRegistrant) : IRegistrant<IDictionary<string, object>>
{
    //Getter

    public static LangRegistrant Get<TModel, TTarget>(LocalizedRegister<TTarget> register)
        where TModel : IModel<IDictionary<string, object>, TTarget> =>
        new(new LocalizedRegistrant<IDictionary<string, object>, TModel, TTarget>(register, () => FileLoader.CurrentLocal));


    //Implement

    public (string id, Func<bool> register)[] Parse(IDictionary<string, object> source, out ModelParseErrorInfo errorMessages)
    {
        return innerRegistrant.Parse(source, out errorMessages);
    }
}