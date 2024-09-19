using System.Collections.Generic;
using System.Linq;

using KirisameLib.Core.Extensions;

using BabelRush.Registers;

using Tomlyn.Model;

namespace BabelRush.Cards;

public record CardTypeData(string Id, bool Usable, int Cost, IEnumerable<string> Actions, IEnumerable<string> Features)
{
    public CardType ToCardType()
    {
        var actions = Actions.Select(id => ActionRegisters.Actions.GetItem(id));
        var features = Features.Select(id => CardFeatureRegisters.Features.GetItem(id));
        return new CommonCardType(Id, Usable, Cost, actions, features);
    }

    public static IEnumerable<CardTypeData> FromTomlTable(TomlTable table)
    {
        if (!table.TryGetValue("cards", out var cards)) yield break;
        if (cards is not TomlTableArray cardList) yield break;
        foreach (var cardEntry in cardList)
        {
            if (!cardEntry.TryGetValue("id",     out var oId) || oId is not string id) continue;
            if (!cardEntry.TryGetValue("usable", out var oUsable) || oUsable is not bool usable) continue;
            if (!cardEntry.TryGetValue("cost",   out var oCost) || oCost is not long cost) continue;

            var actions =
                (cardEntry.GetOrDefault("actions") as TomlArray)?.Select(x => x!.ToString()!) ?? [];
            var features =
                (cardEntry.GetOrDefault("features") as TomlArray)?.Select(x => x!.ToString()!) ?? [];

            yield return new CardTypeData(id, usable, (int)cost, actions, features);
        }
    }
}