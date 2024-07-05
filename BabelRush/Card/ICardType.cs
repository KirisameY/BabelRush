using System.Collections.Generic;

using BabelRush.Action;

using Godot;

namespace BabelRush.Card;

public interface ICardType
{
    string Name { get; }
    Texture2D Icon { get; }
    bool Usable { get; }
    int Cost { get; }
    IReadOnlyList<IAction> Actions { get; }

    ICard NewInstance();
}