using System.Diagnostics.CodeAnalysis;

using BabelRushR.Core.Common;
using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Numeric;
using BabelRushR.Core.Numeric.CommonModifiers;

using KirisameY.GenericUtils;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.Numeric;
using KirisameY.Randomizer;

namespace BabelRushR.Core.Card.Operations;

public class CommonCardDeck(IGamePlay gamePlay) : ICardDeck
{
    public IModifierEditableNumeric<int, CommonNumericModifyOrder> DrawThreshold => field ??=
        INumeric.CreateReadonly(4, TypeA.Of<CommonNumericModifyOrder>())
                .WithModifier(new CommonLimitModifier(min: 1, max: ICardDeck.HandLimit))
                .WithUpdateHandler((_, _) => CheckHandAndDrawPile());


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


    #region Operations

    public bool AddCard(ICard card, CardPile targetPile, int pos = -1)
    {
        if (this.FindCard(card) is not { Pile: CardPile.None }) return false;
        if (targetPile is CardPile.Hand && _handPile.Count >= ICardDeck.HandLimit) // 手牌溢出处理
        {
            targetPile = CardPile.Draw;
            pos        = -1;
        }

        var pile = PileOf(targetPile);
        if (pos < 0) pos = pile.Count;
        pile.Insert(pos, card);
        gamePlay.EventBus.Publish(new CardDeckAddedCardEvent(card, targetPile, pos));
        CheckHandAndDrawPile();
        return true;
    }

    public bool MoveCard(CardPile fromPile, int fromPos, CardPile targetPile, int targetPos = -1)
    {
        if (fromPile is CardPile.None || targetPile is CardPile.None) return false;
        if (targetPile is CardPile.Hand && fromPile is not CardPile.Hand && _handPile.Count >= ICardDeck.HandLimit) // 手牌溢出处理
        {
            targetPile = fromPile is CardPile.Draw ? CardPile.Discard : CardPile.Draw;
            targetPos  = -1;
        }

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
        CheckHandAndDrawPile();
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
        CheckHandAndDrawPile();
        return true;
    }

    public bool RemoveCard(ICard card)
    {
        var (pile, index) = this.FindCard(card);
        return RemoveCard(pile, index, out _);
    }

    #endregion


    #region CardLoop

    private bool _checking = false;

    public void ShuffleDiscardPileToDrawPile()
    {
        var randomizer = gamePlay.GetRandomizer("core.card_shuffle");
        while (DiscardPile.Count > 0)
        {
            var from = randomizer.NextI32(DiscardPile.Count);
            MoveCard(CardPile.Discard, from, CardPile.Draw);
        }
    }

    private void CheckHandAndDrawPile()
    {
        if (Interlocked.Exchange(ref _checking, true)) return;
        using var flagHolder = new FlagHolder(ref _checking);

        var draw = DrawThreshold.Value;
        var inHand = HandPile.Count();
        if (inHand >= draw) return;

        if (inHand == 0 && DrawPile.Count == 0) ShuffleDiscardPileToDrawPile();
        while (inHand < draw && DrawPile.Count > 0)
        {
            MoveCard(CardPile.Draw, -1, CardPile.Hand);
            if (true) inHand++;
        }
    }

    #endregion
}