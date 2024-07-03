using System.Collections.Generic;

using BabelRush.Action;

using Godot;

namespace BabelRush.Card;

public interface ICardType
{
    string Name { get; }
    Texture2D Icon { get; }
    int Cost { get; }
    IList<IAction> Actions { get; }

    ICard NewInstance();
}