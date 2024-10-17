using System;
using System.Collections.Generic;

namespace BabelRush.Data;

public interface IModel<out TTarget>
{
    TTarget Convert();
}

public interface IModel<in TSource, out TTarget> : IModel<TTarget>
{
    static abstract IModel<TTarget>? FromSource(TSource source);
}

public interface IDataModel<out TTarget> : IModel<IDictionary<string, object>, TTarget>;

public interface IResModel<out TTarget> : IModel<ResSource, TTarget>;

public interface ILangModel<out TTarget> : IModel<KeyValuePair<string, object>, TTarget>;