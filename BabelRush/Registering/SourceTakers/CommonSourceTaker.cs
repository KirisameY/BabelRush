using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

namespace BabelRush.Registering.SourceTakers;

public abstract class CommonSourceTaker<TSource, TModel, TTarget> : ISourceTaker<TSource>
    where TModel : IModel<TSource, TTarget>
{
    public void Take(TSource source, string path, out ModelParseErrorInfo errorMessages)
    {
        var models = TModel.FromSource(source, out errorMessages);
        List<string> errors = [];
        foreach (var model in models)
        {
            RegisterItem(model, path, out var e);
            errors.AddRange(e);
        }
        if (errors.Count != 0)
        {
            var finalErrors = errorMessages.Messages.Concat(errors).ToArray();
            errorMessages = new(finalErrors.Length, finalErrors);
        }
    }

    protected abstract void RegisterItem(IModel<TTarget> model, string path, out string[] errors);
}