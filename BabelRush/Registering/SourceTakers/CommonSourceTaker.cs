using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

namespace BabelRush.Registering.SourceTakers;

public abstract class CommonSourceTaker<TSource, TModel, TTarget> : ISourceTaker<TSource>
    where TModel : IModel<TSource, TTarget>
{
    public void Take(TSource source, out ModelParseErrorInfo errorMessages)
    {
        var models = TModel.FromSource(source, out errorMessages);
        List<string> errors = [];
        foreach (var model in models)
        {
            if (!RegisterItem(model.Id, model.Convert()))
                errors.Add($"Failed to register item {model.Id}, Possibly there's already a registered item with a duplicate ID.");
        }
        if (errors.Count != 0)
        {
            var finalErrors = errorMessages.Messages.Concat(errors).ToArray();
            errorMessages = new(finalErrors.Length, finalErrors);
        }
    }

    protected abstract bool RegisterItem(string id, TTarget item);
}