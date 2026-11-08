using System.Runtime.CompilerServices;

using BabelRushR.Core.EntityAction;
using BabelRushR.Core.GamePlay;

using KirisameY.EventBus.EventComponents;

namespace BabelRushR.Core.Card.Operations;

public sealed class CardDirector(IGamePlay gamePlay)
{
    private ICardDeck CardDeck => gamePlay.PlayerState.CardDeck;

    public void PlayCard(ICard card)
    {
        gamePlay.EventBus.Publish(new CardPlayingEvent(card));

        var ctx = new EntityActionContext(gamePlay, gamePlay.PlayerState.PCEntity);
        foreach (var action in card.Actions)
        {
            action.Act(ctx);
        }

        gamePlay.EventBus.Publish(new CardPlayedEvent(card));
        CardDeck.SetCardTo(card, CardPile.Discard);
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

public static class CardDirectorAttachment
{
    private static readonly ConditionalWeakTable<IGamePlay, CardDirector> _cache = [];

    extension(IGamePlay gamePlay)
    {
        public CardDirector CardDirector => _cache.GetOrAdd(gamePlay, play => new(play));
    }
}