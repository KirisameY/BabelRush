using BabelRush.Cards;

using JetBrains.Annotations;

using KirisameLib.Core.Events;

namespace BabelRush.GamePlay;

public class CardManager
{
    //Pile
    public CardPile CardField { get; } = [];
    public CardPile DrawPile { get; } = [];
    public CardPile DiscardPile { get; } = [];


    //Todo:整体列表 ，进出整体卡组记录


    //EventHandlers
    [EventHandler] [UsedImplicitly]
    private static void OnCardDiscard(CardDiscardEvent e)
    {
        var cardManager = Play.CardManager;
        cardManager.DrawPile.RemoveCard(e.Card);
        cardManager.CardField.RemoveCard(e.Card);
        cardManager.DiscardPile.AddCard(e.Card);
    }

    [EventHandler] [UsedImplicitly]
    private static void OnCardExhaust(CardExhaustEvent e)
    {
        var cardManager = Play.CardManager;
        cardManager.DrawPile.RemoveCard(e.Card);
        cardManager.CardField.RemoveCard(e.Card);
        cardManager.DiscardPile.RemoveCard(e.Card);
    }
}