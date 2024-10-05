using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using KirisameLib.Core.Extensions;

using BabelRush.Registers;

namespace BabelRush.Cards;

public record CardTypeData(string Id, bool Usable, int Cost, ImmutableArray<string> Actions, ImmutableArray<string> Features)
{
    public CardType ToCardType()
    {
        var actions = Actions.Select(id => ActionRegisters.Actions.GetItem(id));
        var features = Features.Select(id => CardFeatureRegisters.Features.GetItem(id));
        return new CardType(Id, Usable, Cost, actions, features);
    }

    public static CardTypeData FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var usable = (bool)entry["usable"];
        var cost = Convert.ToInt32(entry["cost"]);

        var actions =
            (entry.GetOrDefault("actions") as IList<object?>)?.Select(x => x!.ToString()!) ?? [];
        var features =
            (entry.GetOrDefault("features") as IList<object?>)?.Select(x => x!.ToString()!) ?? [];
        return new CardTypeData(id, usable, cost, [..actions], [..features]);
    }
}