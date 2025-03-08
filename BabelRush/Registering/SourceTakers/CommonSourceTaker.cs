using System;
using System.Linq;

using BabelRush.Data;

namespace BabelRush.Registering.SourceTakers;

public class CommonSourceTaker<TSource, TModel, TTarget>(RegisterItem<TTarget> registerItem) : ISourceTaker<TSource>
    where TModel : IModel<TSource, TTarget>
{
    public (string id, Func<bool> register)[] Take(TSource source, out ModelParseErrorInfo errorMessages)
    {
        var models = TModel.FromSource(source, out errorMessages);
        return models.Select(GetRegister).ToArray();

        (string id, Func<bool> register) GetRegister(IModel<TTarget> model)
        {
            return (model.Id, () => registerItem(model.Id, model.Convert()));
        }
    }
}

public delegate bool RegisterItem<in TItem>(string id, TItem item);