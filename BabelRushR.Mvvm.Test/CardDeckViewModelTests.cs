using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class CardDeckViewModelTests
{
    private static CommonPlayerState NewPlayerState() => new(new TestEntity(maxHP: 10));

    [Fact]
    public void Exposes_The_Source_Player_State()
    {
        var playerState = NewPlayerState();

        var viewModel = new CardDeckViewModel(playerState);

        Assert.Same(playerState, viewModel.SourcePlayerState);
    }

    [Fact]
    public void HandPile_Maps_Cards_To_ViewModels()
    {
        var playerState = NewPlayerState();
        playerState.CardDeck.HandPile.Add(TestCards.New(2));

        var viewModel = new CardDeckViewModel(playerState);

        var card = Assert.Single(viewModel.HandPile);
        Assert.Equal(2, card.Cost);
    }

    [Fact]
    public void HandPile_Is_Lazily_Created_And_Stable()
    {
        var viewModel = new CardDeckViewModel(NewPlayerState());

        var hand = viewModel.HandPile;

        Assert.Same(hand, viewModel.HandPile);
    }
}
