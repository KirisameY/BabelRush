using System.Diagnostics.CodeAnalysis;

using BabelRushR.Core.GamePlay;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Card.Operations;

public class CommonCardDeck(IGamePlay gamePlay) : ICardDeck
{
    private readonly NotifiableList<ICard> _handPile = [];
    private readonly NotifiableList<ICard> _drawPile = [];
    private readonly NotifiableList<ICard> _discardPile = [];

    public IReadOnlyNotifiableList<ICard> HandPile => field ??= _handPile.AsReadOnlyNotifiableList();
    public IReadOnlyNotifiableList<ICard> DrawPile => field ??= _drawPile.AsReadOnlyNotifiableList();
    public IReadOnlyNotifiableList<ICard> DiscardPile => field ??= _discardPile.AsReadOnlyNotifiableList();


    private NotifiableList<ICard> PileOf(CardPile pile) => pile switch
    {
        CardPile.Hand    => _handPile,
        CardPile.Draw    => _drawPile,
        CardPile.Discard => _discardPile,
        _                => throw new ArgumentOutOfRangeException() // todo: 等待 closed enums
    };

    public bool AddCard(ICard card, CardPile targetPile, int pos = -1)
    {
        if (this.FindCard(card) is not { Pile: CardPile.None }) return false;

        var pile = PileOf(targetPile);
        if (pos < 0) pos = pile.Count;
        pile.Insert(pos, card);
        gamePlay.EventBus.Publish(new CardDeckAddedCardEvent(card, targetPile, pos));
        return true;
    }

    public bool MoveCard(CardPile fromPile, int fromPos, CardPile targetPile, int targetPos = -1)
    {
        if (fromPile is CardPile.None || targetPile is CardPile.None) return false;

        var from = PileOf(fromPile);
        var to = PileOf(targetPile);
        if (fromPos < 0) fromPos = from.Count - 1;

        ICard card;
        if (fromPile == targetPile)
        {
            if (targetPos < 0) targetPos = to.Count - 1;
            card = from[fromPos];
            from.Move(fromPos, targetPos);
        }
        else
        {
            if (targetPos < 0) targetPos = to.Count;
            card = from.RemoveAt(fromPos);
            to.Insert(targetPos, card);
        }
        gamePlay.EventBus.Publish(new CardDeckMovedCardEvent(card, fromPile, fromPos, targetPile, targetPos));
        return true;
    }

    public bool SetCardTo(ICard card, CardPile targetPile, int pos = -1)
    {
        if (this.FindCard(card) is { Pile: not CardPile.None and var pile, Index: var index })
        {
            return MoveCard(pile, index, targetPile, pos);
        }
        return AddCard(card, targetPile, pos);
    }

    public bool RemoveCard(CardPile fromPile, int pos, [MaybeNullWhen(false)] out ICard removed)
    {
        removed = null;
        if (fromPile is CardPile.None) return false;

        removed = PileOf(fromPile).RemoveAt(pos);
        gamePlay.EventBus.Publish(new CardDeckRemovedCardEvent(removed, fromPile, pos));
        return true;
    }

    public bool RemoveCard(ICard card)
    {
        var (pile, index) = this.FindCard(card);
        return RemoveCard(pile, index, out _);
    }
}