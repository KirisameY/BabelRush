using System.ComponentModel;

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

        // Dispose 会把场景 VM 连同里面的实体 VM 一起放空，之后场景再变也不反应
        Assert.Empty(viewModel.Scene.Entities);
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
        var gamePlay = NewTestGamePlay(new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        gamePlay.Raise("NotAGamePlayProperty");

        Assert.Empty(notified);
    }

    [Fact]
    public void Empty_Property_Name_Notifies_Every_Mapped_Property()
    {
        var gamePlay = NewTestGamePlay(new TestEntity(maxHP: 10));
        var viewModel = new GamePlayViewModel(gamePlay);
        var notified = NotificationRecorder.PropertyNames(viewModel);

        gamePlay.Raise(null);

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

    [Fact]
    public void Switching_The_Scene_Rebuilds_The_Scene_ViewModel()
    {
        var player = new TestEntity(maxHP: 10) { HP = 5 };
        var gamePlay = NewTestGamePlay(player);
        var viewModel = new GamePlayViewModel(gamePlay);
        var oldSceneViewModel = viewModel.Scene;
        var oldPlayerViewModel = oldSceneViewModel.Find(player)!;

        gamePlay.SwitchScene(NewScene(player));

        Assert.NotSame(oldSceneViewModel, viewModel.Scene);
        Assert.Same(gamePlay.Scene, viewModel.Scene.SourceScene);
        Assert.NotSame(oldPlayerViewModel, viewModel.Scene.Find(player));

        var oldNotified = NotificationRecorder.PropertyNames(oldPlayerViewModel);
        var newNotified = NotificationRecorder.PropertyNames(viewModel.Scene.Find(player)!);
        player.HP = 3;

        // 旧树已被拆干净，新树接着跟
        Assert.Empty(oldNotified);
        Assert.Contains(nameof(EntityViewModel.HPRatio), newNotified);
    }

    [Fact]
    public void Repeating_The_Same_Scene_Does_Not_Rebuild_It()
    {
        var player = new TestEntity(maxHP: 10);
        var gamePlay = NewTestGamePlay(player);
        var viewModel = new GamePlayViewModel(gamePlay);
        var sceneViewModel = viewModel.Scene;
        var playerViewModel = sceneViewModel.Find(player)!;

        // 没真换场景时，点名通知和"全部失效"都不该推倒重来
        gamePlay.Raise(nameof(IGamePlay.Scene));
        gamePlay.Raise(null);

        Assert.Same(sceneViewModel, viewModel.Scene);
        Assert.Same(playerViewModel, viewModel.Scene.Find(player));
    }

    [Fact]
    public void Disposing_After_A_Scene_Switch_Releases_The_New_Scene()
    {
        var player = new TestEntity(maxHP: 10) { HP = 5 };
        var gamePlay = NewTestGamePlay(player);
        var viewModel = new GamePlayViewModel(gamePlay);

        gamePlay.SwitchScene(NewScene(player));
        var playerViewModel = viewModel.Scene.Find(player)!;
        var notified = NotificationRecorder.PropertyNames(playerViewModel);

        viewModel.Dispose();
        player.HP = 3;

        // 换进来的那个场景 VM 也得被 Track 上，Dispose 时才摘得干净
        Assert.Empty(notified);
    }

    /// <summary>
    ///     可以随意发属性通知、也能真的换掉场景的 <see cref="IGamePlay"/> 替身——
    ///     <see cref="CommonGamePlay"/> 的 <c>Scene</c> 建好就换不了了，只能手写。
    /// </summary>
    private sealed class TestGamePlay(IScene scene, IPlayerState playerState, IEventBus eventBus) : IGamePlay
    {
        public IEventBus EventBus => eventBus;

        public IPlayerState PlayerState => playerState;

        public IScene Scene { get; private set; } = scene;

        public double Time { get; private set; }

        public double DeltaTime { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Update(double delta)
        {
            DeltaTime =  delta;
            Time      += delta;
        }

        public void SwitchScene(IScene newScene)
        {
            Scene = newScene;
            Raise(nameof(Scene));
        }

        public void Raise(string? propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static TestGamePlay NewTestGamePlay(TestEntity pcEntity)
    {
        var eventBus = new SimpleEventBus();
        var scene = new CommonScene(eventBus);
        scene.AddEntity(pcEntity);
        return new TestGamePlay(scene, new CommonPlayerState(pcEntity, eventBus), eventBus);
    }

    private static CommonScene NewScene(TestEntity entity)
    {
        var scene = new CommonScene(new SimpleEventBus());
        scene.AddEntity(entity);
        return scene;
    }
}
