using Godot;

namespace BabelRush.Card;

public interface ICardType
{
    string Name { get; }
    Texture2D Icon { get; }
    int Cost { get; }
    
    ICard NewInstance();
}