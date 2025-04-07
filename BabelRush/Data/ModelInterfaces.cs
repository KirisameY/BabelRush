using System.Collections.Generic;

using Tomlyn.Syntax;

namespace BabelRush.Data;

public interface IModel<TTarget>
{
    (RegKey, TTarget) Convert(string nameSpace, string path);
}

public interface IModel<in TSource, TTarget> : IModel<TTarget>
{
    static abstract IReadOnlyCollection<IModel<TTarget>> FromSource(TSource source, out ModelParseErrorInfo errorMessages);
}

public interface IScriptModel<TTarget> : IModel<ScriptSourceInfo, TTarget>;

public interface IDataModel<TTarget> : IModel<DocumentSyntax, TTarget>;

public interface IResModel<TTarget> : IModel<ResSourceInfo, TTarget>;

public interface ILangModel<TTarget> : IModel<IDictionary<string, object>, TTarget>;