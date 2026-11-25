using System.Reflection;

using BabelRushR.Core.Card.Operations;
using BabelRushR.Core.Common;
using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;
using BabelRushR.Mvvm.ViewModels;

namespace BabelRushR.Mvvm.Test;

public class GamePlayViewModelTests
{
    [Fact]
    public void Update_Advances_The_GamePlay()
    {
        var gamePlay = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);

        viewModel.Update(0.5);

        Assert.Equal(0.5, gamePlay.Time, 10);
        Assert.Equal(0.5, gamePlay.DeltaTime, 10);
        Assert.Equal(0.5, viewModel.Time, 10);
        Assert.Equal(0.5, viewModel.DeltaTime, 10);
    }

    [Fact]
    public void Player_Entity_Appears_Exactly_Once_In_The_Scene()
    {
        var player = new TestEntity(maxHP: 10);
        var gamePlay = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), player);
        var viewModel = new GamePlayViewModel(gamePlay);

        var playerViewModel = viewModel.Scene.Find(gamePlay.PlayerState.PCEntity);

        Assert.NotNull(playerViewModel);
        Assert.Same(playerViewModel, Assert.Single(viewModel.Scene.Entities));
    }

    [Fact]
    public void Disposing_Stops_Reflecting_The_Scene()
    {
        var gamePlay = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);

        viewModel.Dispose();
        gamePlay.Scene.AddEntity(new TestEntity(maxHP: 5));

        // Dispose 会把场景 VM 连同里面的实体 VM 一起放空，之后场景再变也不反应
        Assert.Empty(viewModel.Scene.Entities);
    }

    [Fact]
    public void Advancing_Time_Notifies_Time_And_DeltaTime()
    {
        var gamePlay = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        viewModel.Update(0.5);

        // 不钉通知条数：具体 VM 的通知集合不属于基类转发契约。
        Assert.Contains(nameof(GamePlayViewModel.Time), notified);
        Assert.Contains(nameof(GamePlayViewModel.DeltaTime), notified);
    }

    [Fact]
    public void Ignored_Update_Does_Not_Notify()
    {
        var gamePlay = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        viewModel.Update(double.NaN);
        viewModel.Update(-1);

        Assert.Empty(notified);
    }

    [Fact]
    public void Switching_The_Scene_Rebuilds_The_Scene_ViewModel()
    {
        var player = new TestEntity(maxHP: 10);
        player.Health.HP = 5;
        var gamePlay = NewTestGamePlay(player);
        var viewModel = new GamePlayViewModel(gamePlay);
        var oldSceneViewModel = viewModel.Scene;
        var oldPlayerViewModel = oldSceneViewModel.Find(player)!;

        gamePlay.Scene = NewScene(player);

        Assert.NotSame(oldSceneViewModel, viewModel.Scene);
        Assert.Same(gamePlay.Scene, viewModel.Scene.SourceScene);
        Assert.NotSame(oldPlayerViewModel, viewModel.Scene.Find(player));

        var oldNotified = NotificationRecorder.PropertyNames(oldPlayerViewModel);
        var newNotified = NotificationRecorder.PropertyNames(viewModel.Scene.Find(player)!);
        player.Health.HP = 3;

        // 旧树已被拆干净，新树接着跟
        Assert.Empty(oldNotified);
        Assert.Contains(nameof(EntityViewModel.IsAlive), newNotified);
    }

    [Fact]
    public void Repeating_The_Same_Scene_Does_Not_Rebuild_It()
    {
        var player = new TestEntity(maxHP: 10);
        var gamePlay = NewTestGamePlay(player);
        var viewModel = new GamePlayViewModel(gamePlay);
        var sceneViewModel = viewModel.Scene;
        var playerViewModel = sceneViewModel.Find(player)!;

        // 没真换场景时，点名通知、"全部失效"、以及把同一个场景实例再赋一次，都不该推倒重来
        RaisePropertyChanged(gamePlay, nameof(IGamePlay.Scene));
        RaisePropertyChanged(gamePlay, null);
        gamePlay.Scene = gamePlay.Scene;

        Assert.Same(sceneViewModel, viewModel.Scene);
        Assert.Same(playerViewModel, viewModel.Scene.Find(player));
    }

    [Fact]
    public void Disposing_After_A_Scene_Switch_Releases_The_New_Scene()
    {
        var player = new TestEntity(maxHP: 10);
        player.Health.HP = 5;
        var gamePlay = NewTestGamePlay(player);
        var viewModel = new GamePlayViewModel(gamePlay);

        gamePlay.Scene = NewScene(player);
        var playerViewModel = viewModel.Scene.Find(player)!;
        var notified = NotificationRecorder.PropertyNames(playerViewModel);

        viewModel.Dispose();
        player.Health.HP = 3;

        // 换进来的那个场景 VM 也得被 Track 上，Dispose 时才摘得干净
        Assert.Empty(notified);
    }

    [Fact]
    public void Exposes_The_Card_Deck()
    {
        var gamePlay = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);

        var cardDeck = viewModel.CardDeck;

        Assert.Same(gamePlay.PlayerState.CardDeck, cardDeck.SourceCardDeck);
        Assert.Same(gamePlay.CardDirector, cardDeck.SourceCardDirector);
        Assert.Same(cardDeck, viewModel.CardDeck);
    }

    [Fact]
    public void Disposing_Releases_The_Card_Deck()
    {
        var card = TestCards.New(1);
        var gamePlay = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10));
        gamePlay.PlayerState.CardDeck.AddCard(card, CardPile.Hand);
        var viewModel = new GamePlayViewModel(gamePlay);
        var hand = viewModel.CardDeck.HandPile;
        var cardViewModel = hand[0];
        var notified = NotificationRecorder.PropertyNames(cardViewModel);

        viewModel.Dispose();
        TestCards.RaiseCost(card, 1);

        // Dispose 会沿 Track 链把卡牌区 VM、手牌列表和牌 VM 一起放空
        Assert.Empty(hand);
        Assert.Empty(notified);
    }

    /// <summary>
    ///     <c>OnPropertyChanged</c> 是 protected，测试靠反射拿到它来伪造属性通知。
    /// </summary>
    private static readonly MethodInfo RaisePropertyChangedMethod =
        typeof(ObservableObject).GetMethod("OnPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static void RaisePropertyChanged(IGamePlay gamePlay, string? propertyName) =>
        RaisePropertyChangedMethod.Invoke(gamePlay, [propertyName]);

    private static CommonGamePlay NewTestGamePlay(TestEntity pcEntity) =>
        CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), pcEntity);

    private static CommonScene NewScene(TestEntity entity)
    {
        var scene = new CommonScene();
        scene.AddEntity(entity);
        return scene;
    }
}