using BabelRushR.Core.Card;
using BabelRushR.Core.Card.Operations;
using BabelRushR.Core.Card.Types;
using BabelRushR.Core.GamePlay;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

public class CardDeckShuffleTests
{
    private const int CardCount = 6;

    private static IReadOnlyList<ICard> NewCards() =>
        [..Enumerable.Range(0, CardCount)
                    .Select(i => new RecordCardType(RegKey.From("test", $"card.{i}"), 1, []).CreateCard())];

    /// <summary>
    ///     發 6 張牌再全部棄掉：手牌會自動補到 4 張，兩張留在抽牌堆；
    ///     棄到手牌與抽牌堆同時為空時觸發「棄牌堆洗回抽牌堆」。
    ///     回傳洗牌後「手牌 + 抽牌堆」的順序。
    /// </summary>
    private static IReadOnlyList<RegKey> Shuffle(ulong seed)
    {
        var game = CommonGamePlay.Create(seed, new TestEntity(maxHP: 10));
        var deck = game.PlayerState.CardDeck;

        foreach (var card in NewCards()) Assert.True(deck.AddCard(card, CardPile.Draw));

        for (var i = 0; i < CardCount; i++) Assert.True(deck.MoveCard(CardPile.Hand, -1, CardPile.Discard));

        Assert.Empty(deck.DiscardPile);
        Assert.Equal(CardCount, deck.HandPile.Count + deck.DrawPile.Count);

        return
        [
            ..deck.HandPile.Select(card => card.DisplayKey),
            ..deck.DrawPile.Select(card => card.DisplayKey),
        ];
    }

    [Fact]
    public void The_Same_Seed_Shuffles_Into_The_Same_Order()
    {
        Assert.Equal(Shuffle(0x5EED), Shuffle(0x5EED));
    }

    [Fact]
    public void Different_Seeds_Shuffle_Into_Different_Orders()
    {
        Assert.NotEqual(Shuffle(1), Shuffle(2));
    }

    [Fact]
    public void Shuffling_Keeps_Every_Card_Exactly_Once()
    {
        var order = Shuffle(0x5EED);
        var expected = Enumerable.Range(0, CardCount).Select(i => RegKey.From("test", $"card.{i}")).ToList();

        Assert.Equal(CardCount, order.Distinct().Count());
        Assert.Empty(expected.Except(order));
        Assert.Empty(order.Except(expected));
    }
}
