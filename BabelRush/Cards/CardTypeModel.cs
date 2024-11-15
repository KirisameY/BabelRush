using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registers;

using JetBrains.Annotations;

using KirisameLib.Data.Model;

using Tomlyn.Syntax;

namespace BabelRush.Cards;

[ModelSet("Card")]
internal partial class CardTypeModel : IDataModel<CardType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    [NecessaryProperty]
    public partial bool Usable { get; set; }
    [NecessaryProperty]
    public partial int Cost { get; set; }
    [UsedImplicitly]
    public List<string> Actions { get; set; } = [];
    [UsedImplicitly]
    public List<string> Features { get; set; } = [];

    public CardType Convert()
    {
        var actions = Actions.Select(id => ActionRegisters.Actions.GetItem(id));
        var features = Features.Select(id => CardFeatureRegisters.Features.GetItem(id));
        return new CardType(Id, Usable, Cost, actions, features);
    }

    public static IReadOnlyCollection<IModel<CardType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        IDataModel<CardType>.ParseFromSource<ModelSet>(source, out errorMessages);
}