using System.Collections.Generic;

using BabelRush.Cards;

using Tomlyn.Model;

namespace BabelRush.Data;

public interface ITomlData<out TSelf> where TSelf : ITomlData<TSelf>
{
    public static abstract TSelf FromTomlEntry(TomlTable table);
}