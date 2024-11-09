using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;

using KirisameLib.Core.Extensions;

using BabelRush.Registers;

using KirisameLib.Data.Model;

using Tomlyn.Syntax;

namespace BabelRush.Cards;

public record CardTypeModel(string Id, bool Usable, int Cost, ImmutableArray<string> Actions, ImmutableArray<string> Features)
    : IDataModel<CardType>
{
    public CardType Convert()
    {
        var actions = Actions.Select(id => ActionRegisters.Actions.GetItem(id));
        var features = Features.Select(id => CardFeatureRegisters.Features.GetItem(id));
        return new CardType(Id, Usable, Cost, actions, features);
    }

    // public static CardTypeModel FromEntry(IDictionary<string, object> entry)
    // {
    //     var id = (string)entry["id"];
    //     var usable = (bool)entry["usable"];
    //     var cost = System.Convert.ToInt32(entry["cost"]);
    //
    //     var actions =
    //         (entry.GetOrDefault("actions") as IList<object?>)?.Select(x => x!.ToString()!) ?? [];
    //     var features =
    //         (entry.GetOrDefault("features") as IList<object?>)?.Select(x => x!.ToString()!) ?? [];
    //     return new CardTypeModel(id, usable, cost, [..actions], [..features]);
    // }
    
    public static IModel<CardType>[] FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages)
    {
        throw new System.NotImplementedException();
    }
}