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
    public List<CardActionEntry> Actions { get; set; } = [];
    [UsedImplicitly]
    public List<string> Features { get; set; } = [];

    public CardType Convert()
    {
        var actions = Actions.Select(t => (ActionRegisters.Actions.GetItem(t.Id), t.Value));
        var features = Features.Select(id => CardFeatureRegisters.Features.GetItem(id));
        return new CardType(Id, Usable, Cost, actions, features);
    }

    public static IReadOnlyCollection<IModel<CardType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, CardType>(source, out errorMessages);

    partial void CustomCheck(List<string> errorList)
    {
        foreach (CardActionEntry entry in Actions)
        {
            if (entry.Check(out var errors)) continue;
            errorList.AddRange(errors);
            break;
        }
    }


    [UsedImplicitly]
    public class CardActionEntry
    {
        private string? _id;
        public string Id
        {
            get => _id ?? throw new ModelDidNotInitializeException();
            set => _id = value;
        }
        public int Value { get; set; }

        public bool Check(out string[] errors)
        {
            if (_id is null)
            {
                errors = ["Property Id did not initialized"];
                return false;
            }
            else
            {
                errors = [];
                return true;
            }
        }
    }
}