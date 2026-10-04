using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;
using BabelRushR.Mvvm.ViewModels;

using KirisameY.EventBus;
using KirisameY.EventBus.Bus;

namespace BabelRushR.Mvvm.Test;

public class GamePlayViewModelTests
{
    [Fact]
    public void Update_Advances_The_GamePlay()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));
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
        var gamePlay = CommonGamePlay.Create(player);
        var viewModel = new GamePlayViewModel(gamePlay);

        var playerViewModel = viewModel.Scene.Find(gamePlay.PlayerState.PCEntity);

        Assert.NotNull(playerViewModel);
        Assert.Same(playerViewModel, Assert.Single(viewModel.Scene.Entities));
    }

    [Fact]
    public void Disposing_Stops_Reflecting_The_Scene()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);

        viewModel.Dispose();
        gamePlay.Scene.AddEntity(new TestEntity(maxHP: 5));

        // 只剩下开局时放进场景的玩家实体
        Assert.Single(viewModel.Scene.Entities);
    }

    [Fact]
    public void Advancing_Time_Notifies_Time_And_DeltaTime()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        viewModel.Update(0.5);

        // 不比对序列：两条属性谁先发不属于对外契约。
        var count = notified.Count;
        Assert.Equal(2, count);
        Assert.Contains(nameof(GamePlayViewModel.Time), notified);
        Assert.Contains(nameof(GamePlayViewModel.DeltaTime), notified);
    }

    [Fact]
    public void Ignored_Update_Does_Not_Notify()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        viewModel.Update(double.NaN);
        viewModel.Update(-1);

        Assert.Empty(notified);
    }

    [Fact]
    public void Unknown_GamePlay_Property_Change_Is_Ignored()
    {
        var gamePlay = NewRaisableGamePlay();
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        gamePlay.NotifyUnknownPropertyChanged();

        Assert.Empty(notified);
    }

    [Fact]
    public void Empty_Property_Name_Notifies_Every_Mapped_Property()
    {
        var gamePlay = NewRaisableGamePlay();
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        gamePlay.NotifyAllPropertiesChanged();

        // 逐个断言而不是比序列：映射表内部的枚举次序不属于对外契约。
        var count = notified.Count;
        Assert.Equal(3, count);
        Assert.Contains(nameof(GamePlayViewModel.Scene), notified);
        Assert.Contains(nameof(GamePlayViewModel.Time), notified);
        Assert.Contains(nameof(GamePlayViewModel.DeltaTime), notified);
    }

    [Fact]
    public void Disposed_ViewModel_Stops_Listening_To_The_GamePlay()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        viewModel.Dispose();
        gamePlay.Update(0.5);

        Assert.Empty(notified);
    }

    /// <summary>
    ///     额外开放属性通知的 <see cref="CommonGamePlay"/>，用来构造它自己发不出的属性名。
    /// </summary>
    private sealed class RaisableGamePlay(IScene scene, IPlayerState playerState, IEventBus eventBus)
        : CommonGamePlay(scene, playerState, eventBus)
    {
        public void NotifyAllPropertiesChanged() => OnPropertyChanged(null);

        public void NotifyUnknownPropertyChanged() => OnPropertyChanged("NotAGamePlayProperty");
    }

    private static RaisableGamePlay NewRaisableGamePlay()
    {
        var eventBus = new SimpleEventBus();
        var pcEntity = new TestEntity(maxHP: 10);
        return new RaisableGamePlay(new CommonScene(eventBus), new CommonPlayerState(pcEntity, eventBus), eventBus);
    }
}
