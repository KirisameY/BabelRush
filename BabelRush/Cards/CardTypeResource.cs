using System.Linq;

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

    public ICardType ToCardType()
    {
        var actions = Actions.Select(id => Registers.Actions.GetItem(id)).ToList();
        return new CardType(Id, Usable, Cost, actions);
    }
}