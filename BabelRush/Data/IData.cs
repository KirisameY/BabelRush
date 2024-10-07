using System.Collections.Generic;

namespace BabelRush.Data;

public interface IData<out TModel, out TData> where TData : IData<TModel, TData>
{
    string Id { get; }
    TModel ToModel();
    static abstract TData FromEntry(IDictionary<string, object> entry);
}