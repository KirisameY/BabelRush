using BabelRushR.Core.Card;
using BabelRushR.Core.EntityAction;

using KirisameY.EventBus.EventComponents;

namespace BabelRushR.Core.GamePlay.CardRelated;

public class CommonCardDirector(IGamePlay gamePlay) : ICardDirector
{
    private ICardDeck CardDeck => gamePlay.PlayerState.CardDeck;

    public void PlayCard(ICard card)
    {
        var fromDeck = true;
        var cardPlace = CardDeck.FindCard(card);
        gamePlay.EventBus.Publish(new CardPlayingEvent(card));

        if (cardPlace is { Pile: var pile, Index: var i }) pile.RemoveAt(i);
        else fromDeck = false;

        var ctx = new EntityActionContext(gamePlay.PlayerState.PCEntity);
        foreach (var action in card.Actions)
        {
            action.Act(ctx);
        }

        gamePlay.EventBus.Publish(new CardPlayedEvent(card));
        if (fromDeck) CardDeck.DiscardPile.Add(card);
    }

    public bool TryPlayCard(ICard card)
    {
        var request = gamePlay.EventBus.Publish(new CardPlayRequestEvent(card, new()));
        if (request.Canceled) return false;

        gamePlay.EventBus.Publish(new CardCostCalculatingEvent(card));
        var cost = card.Cost.Value;
        if (gamePlay.PlayerState.AP < cost) return false;
        gamePlay.PlayerState.AP -= cost;

        PlayCard(card);
        return true;
    }
}