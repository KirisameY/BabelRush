using System.Linq;

using BabelRush.Registers;

using Godot;

namespace BabelRush.Cards;

[GlobalClass]
public partial class CardTypeResource : Resource
{
    [Export]
    public string Id { get; set; } = "ID";

    [Export]
    public bool Usable { get; set; } = true;

    [Export]
    public int Cost { get; set; }

    [Export]
    public string[] Actions { get; set; } = [];

    [Export]
    public string[] Features { get; set; } = [];

    public CardType ToCardType()
    {
        var actions = Actions.Select(id => ActionRegisters.Actions.GetItem(id)).ToList();
        var features = Features.Select(id => CardFeatureRegisters.Features.GetItem(id)).ToList();
        return new CommonCardType(Id, Usable, Cost, actions, features);
    }
}