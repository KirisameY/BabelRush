using System.Collections.Generic;

namespace BabelRush.Registering.Parsing;

public interface IBox<in TSource, out TBox, out TAsset> where TBox : IBox<TSource, TBox, TAsset>
{
    string Id { get; }
    TAsset GetAsset();
    static abstract TBox FromEntry(TSource entry);
}

#region Sub-Interfaces

public interface IDataBox<out TData, out TBox>
    : IBox<IDictionary<string, object>, TBox, TData>
    where TBox : IDataBox<TData, TBox>;

public interface IResBox<out TRes, out TBox>
    : IBox<ResSource, TBox, TRes>
    where TBox : IResBox<TRes, TBox>;

public interface ILangBox<out TLang, out TBox>
    : IBox<KeyValuePair<string, object>, TBox, TLang>
    where TBox : ILangBox<TLang, TBox>;

#endregion