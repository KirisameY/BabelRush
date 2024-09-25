using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

using KirisameLib.Core.Extensions;

using BabelRush.Registers;

using Godot.NativeInterop;

using Tomlyn.Model;

namespace BabelRush.Cards;

public record CardTypeData(string Id, bool Usable, int Cost, IEnumerable<string> Actions, IEnumerable<string> Features)
    : ITomlData<CardTypeData>
{
    public CardType ToCardType()
    {
        var actions = Actions.Select(id => ActionRegisters.Actions.GetItem(id));
        var features = Features.Select(id => CardFeatureRegisters.Features.GetItem(id));
        return new CommonCardType(Id, Usable, Cost, actions, features);
    }

    public static CardTypeData FromTomlEntry(TomlTable entry)
    {
        var id = (string)entry["id"];
        var usable = (bool)entry["usable"];
        var cost = Convert.ToInt32(entry["cost"]);

        var actions =
            (entry.GetOrDefault("actions") as TomlArray)?.Select(x => x!.ToString()!) ?? [];
        var features =
            (entry.GetOrDefault("features") as TomlArray)?.Select(x => x!.ToString()!) ?? [];
        return new CardTypeData(id, usable, cost, actions, features);
    }
}