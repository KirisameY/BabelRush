using BabelRush.Cards;

namespace BabelRush.GamePlay;

public class PlayerInfo
{
    //Action
    public double MovingSpeed { get; set; } = 16;
    public bool Moving { get; set; }


    //Cards
    public CardPile CardField { get; } = [];
    public CardPile DrawPile { get; } = [];
    public CardPile DiscardPile { get; } = [];
}