using System.Collections.Generic;
using System.Linq;

using Tomlyn;
using Tomlyn.Syntax;

namespace BabelRush.Data;

public static class ModelUtils
{
    public static IReadOnlyCollection<IModel<TTarget>> ParseFromSource<TModelSet, TTarget>
        (DocumentSyntax source, out ModelParseErrorInfo errorMessages)
        where TModelSet : class, IModelSet<IModel<TTarget>>, new()
    {
        source.TryToModel<TModelSet>(out var modelSet, out var diagnostics);
        string[] checkErrors = [];
        IReadOnlyCollection<IModel<TTarget>> result = modelSet?.CheckAll(out checkErrors) ?? [];
        var errors = diagnostics.Select(msg => msg.ToString()).Concat(checkErrors).ToArray();
        errorMessages = new(errors.Length, errors);
        return result;
    }
}