using System;
using System.Collections.Generic;

using BabelRush.Data;

namespace BabelRush.Registering.SourceTakers;

public class LangSourceTaker(ISourceTaker<IDictionary<string, object>> innerRegistrant) : ISourceTaker<IDictionary<string, object>>
{
    //Getter

    public static LangSourceTaker Get<TModel, TTarget>(RegisterLocalItem<TTarget> register)
        where TModel : IModel<IDictionary<string, object>, TTarget> =>
        new(new LocalizedSourceTaker<IDictionary<string, object>, TModel, TTarget>(register, () => FileLoader.CurrentLocal));


    //Implement
    public (string id, Func<bool> register)[] Take(IDictionary<string, object> source, out ModelParseErrorInfo errorMessages)
    {
        return innerRegistrant.Take(source, out errorMessages);
    }
}