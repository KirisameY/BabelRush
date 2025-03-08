using System;

using BabelRush.Data;

namespace BabelRush.Registering.SourceTakers;

public class LocalizedSourceTaker<TSource, TModel, TTarget>(RegisterLocalItem<TTarget> registerLocalItem, Func<string> localGetter)
    : CommonSourceTaker<TSource, TModel, TTarget>((id, item) => registerLocalItem(localGetter(), id, item))
    where TModel : IModel<TSource, TTarget>;

public delegate bool RegisterLocalItem<in TItem>(string local, string id, TItem item);