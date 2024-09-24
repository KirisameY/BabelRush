using System.Collections.Generic;

using BabelRush.Cards;

using Tomlyn.Model;

namespace BabelRush.Data;

public interface ITomlData<out TModel, TSelf> : IData<TModel> where TSelf : ITomlData<TModel, TSelf>
{
    public static abstract IEnumerable<ParseResult<TSelf>> FromTomlTable(TomlTable table);
}

public interface ISavableTomlData<TModel, TSelf> : ITomlData<TModel, TSelf>, ISavableData<TModel, TSelf>
    where TSelf : ISavableTomlData<TModel, TSelf> { }