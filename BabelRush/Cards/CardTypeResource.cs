using System;

using Godot;

namespace BabelRush.Cards;

[GlobalClass]
public partial class CardTypeResource : Resource
{
    [Export]
    public string Name { get; set; } = "Name";

    [Export]
    public Texture2D? Icon { get; set; }

    [Export]
    public bool Usable { get; set; } = true;

    [Export]
    public int Cost { get; set; }

    [Export]
    public string[] Actions { get; set; } = [];

    public CardType ToCardType()
    {
        throw new NotImplementedException();
    }
}