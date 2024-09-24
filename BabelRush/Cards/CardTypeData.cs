using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

using KirisameLib.Core.Extensions;

using BabelRush.Registers;

using Tomlyn.Model;

namespace BabelRush.Cards;

public record CardTypeData(string Id, bool Usable, int Cost, IEnumerable<string> Actions, IEnumerable<string> Features)
    : ITomlData<CardType, CardTypeData>
{
    public CardType ToModel()
    {
        var actions = Actions.Select(id => ActionRegisters.Actions.GetItem(id));
        var features = Features.Select(id => CardFeatureRegisters.Features.GetItem(id));
        return new CommonCardType(Id, Usable, Cost, actions, features);
    }

    private static CardTypeData FromTomlEntry(TomlTable entry)
    {
        var id = (string)entry["id"];
        var usable = (bool)entry["usable"];
        var cost = (int)(long)entry["cost"];

        var actions =
            (entry.GetOrDefault("actions") as TomlArray)?.Select(x => x!.ToString()!) ?? [];
        var features =
            (entry.GetOrDefault("features") as TomlArray)?.Select(x => x!.ToString()!) ?? [];
        return new CardTypeData(id, usable, cost, actions, features);
    }

    public static IEnumerable<ParseResult<CardTypeData>> FromTomlTable(TomlTable table)
    {
        if (!table.TryGetValue("cards", out var cards)) yield break;
        if (cards is not TomlTableArray cardList) yield break;
        foreach (var cardEntry in cardList)
        {
            ParseResult<CardTypeData> result;
            try
            {
                result = new(FromTomlEntry(cardEntry));
            }
            catch (Exception e)
            {
                result = new(e);
            }
            yield return result;
        }
    }
}