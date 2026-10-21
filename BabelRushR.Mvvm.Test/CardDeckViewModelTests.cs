using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class CardDeckViewModelTests
{
    private static CommonGamePlay NewGamePlay() => CommonGamePlay.Create(new TestEntity(maxHP: 10));

    private static CardDeckViewModel NewViewModel(CommonGamePlay gamePlay) =>
        new(gamePlay.PlayerState.CardDeck, gamePlay.CardDirector);

    [Fact]
    public void Exposes_The_Source_Card_Deck_And_Director()
    {
        var gamePlay = NewGamePlay();

        var viewModel = NewViewModel(gamePlay);

        Assert.Same(gamePlay.PlayerState.CardDeck, viewModel.SourceCardDeck);
        Assert.Same(gamePlay.CardDirector, viewModel.SourceCardDirector);
    }

    [Fact]
    public void HandPile_Maps_Cards_To_ViewModels()
    {
        var gamePlay = NewGamePlay();
        gamePlay.PlayerState.CardDeck.HandPile.Add(TestCards.New(2));

        var viewModel = NewViewModel(gamePlay);

        var card = Assert.Single(viewModel.HandPile);
        Assert.Equal(2, card.Cost);
    }

    [Fact]
    public void HandPile_Is_Lazily_Created_And_Stable()
    {
        var viewModel = NewViewModel(NewGamePlay());

        var hand = viewModel.HandPile;

        Assert.Same(hand, viewModel.HandPile);
    }

    [Fact]
    public void TryPlayCard_Plays_The_Source_Card_Through_The_Director()
    {
        var gamePlay = NewGamePlay();
        var card = TestCards.New(2);
        gamePlay.PlayerState.CardDeck.HandPile.Add(card);
        gamePlay.PlayerState.AP = 6;
        var viewModel = NewViewModel(gamePlay);
        var cardViewModel = Assert.Single(viewModel.HandPile);

        viewModel.TryPlayCard(cardViewModel);

        // 打牌走 Core 的导演：扣费、移出手牌、进弃牌堆。
        Assert.Equal(4, gamePlay.PlayerState.AP);
        Assert.Empty(gamePlay.PlayerState.CardDeck.HandPile);
        Assert.Same(card, Assert.Single(gamePlay.PlayerState.CardDeck.DiscardPile));
    }
}
