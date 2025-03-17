using System.Collections.Generic;

using NLua;

using Tomlyn.Syntax;

namespace BabelRush.Data;

public interface IModel<out TTarget>
{
    string Id { get; }
    TTarget Convert();
}

public interface IModel<in TSource, out TTarget> : IModel<TTarget>
{
    static abstract IReadOnlyCollection<IModel<TTarget>> FromSource(TSource source, out ModelParseErrorInfo errorMessages);
}

public interface IScriptModel<out TTarget> : IModel<ScriptSourceInfo, TTarget>;

public interface IDataModel<out TTarget> : IModel<DocumentSyntax, TTarget>;

public interface IResModel<out TTarget> : IModel<ResSourceInfo, TTarget>;

public interface ILangModel<out TTarget> : IModel<IDictionary<string, object>, TTarget>;