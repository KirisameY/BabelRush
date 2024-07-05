using System.Collections.Generic;

using BabelRush.Actions;

using Godot;

namespace BabelRush.Cards;

public interface ICardType
{
    string Name { get; }
    Texture2D Icon { get; }
    bool Usable { get; }
    int Cost { get; }
    IReadOnlyList<IAction> Actions { get; }

    ICard NewInstance();
}