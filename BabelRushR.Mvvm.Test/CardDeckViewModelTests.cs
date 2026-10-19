using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class CardDeckViewModelTests
{
    private static CommonPlayerState NewPlayerState() => new(new TestEntity(maxHP: 10));

    [Fact]
    public void Exposes_Source_And_Hand()
    {
        var playerState = NewPlayerState();
        playerState.CardDeck.HandPile.Add(TestCards.New(2));

        var viewModel = new CardDeckViewModel(playerState);

        Assert.Same(playerState, viewModel.SourcePlayerState);
        var card = Assert.Single(viewModel.HandPile);
        Assert.Equal(2, card.Cost);
    }

    [Fact]
    public void Hand_Is_Lazily_Created_And_Stable()
    {
        var viewModel = new CardDeckViewModel(NewPlayerState());

        var hand = viewModel.HandPile;

        Assert.Same(hand, viewModel.HandPile);
    }

    [Fact]
    public void Disposing_Disposes_The_Hand()
    {
        var card = TestCards.New(1);
        var playerState = NewPlayerState();
        playerState.CardDeck.HandPile.Add(card);
        var viewModel = new CardDeckViewModel(playerState);
        var hand = viewModel.HandPile;
        var cardViewModel = hand[0];
        var notified = NotificationRecorder.PropertyNames(cardViewModel);

        viewModel.Dispose();
        TestCards.RaiseCost(card, 1);

        Assert.Empty(hand);
        Assert.Empty(notified);
    }
}