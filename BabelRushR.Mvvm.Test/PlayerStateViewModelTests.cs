using BabelRushR.Core.GamePlay;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class PlayerStateViewModelTests
{
    private static CommonPlayerState NewPlayerState() => new(new TestEntity(maxHP: 10));

    [Fact]
    public void Exposes_Source_And_Hand()
    {
        var playerState = NewPlayerState();
        playerState.HandPile.Add(TestCards.New(2));

        var viewModel = new PlayerStateViewModel(playerState);

        Assert.Same(playerState, viewModel.SourcePlayerState);
        var card = Assert.Single(viewModel.Hand.Cards);
        Assert.Equal(2, card.Cost);
    }

    [Fact]
    public void Hand_Is_Lazily_Created_And_Stable()
    {
        var viewModel = new PlayerStateViewModel(NewPlayerState());

        var hand = viewModel.Hand;

        Assert.Same(hand, viewModel.Hand);
    }

    [Fact]
    public void Disposing_Disposes_The_Hand()
    {
        var card = TestCards.New(1);
        var playerState = NewPlayerState();
        playerState.HandPile.Add(card);
        var viewModel = new PlayerStateViewModel(playerState);
        var hand = viewModel.Hand;
        var cardViewModel = hand.Cards[0];
        var notified = NotificationRecorder.PropertyNames(cardViewModel);

        viewModel.Dispose();
        TestCards.RaiseCost(card, 1);

        Assert.Empty(hand.Cards);
        Assert.Empty(notified);
    }
}
