using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Entity;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Test;

public class GamePlayTests
{
    /// <summary>
    ///     手动装配一局：<see cref="CommonGamePlay.Scene"/> 与 <c>PlayerState</c> 都是 required 的，
    ///     只能经由对象初始化器交给它。与 <see cref="CommonGamePlay.Create"/> 不同，这里不塞玩家实体。
    /// </summary>
    private static CommonGamePlay NewGamePlay(IScene? scene = null, IPlayerState? playerState = null)
        => new()
        {
            Scene       = scene ?? new CommonScene(),
            PlayerState = playerState ?? new CommonPlayerState(new TestEntity(maxHP: 10)),
        };

    [Fact]
    public void Update_Accumulates_Time_And_Tracks_Last_Delta()
    {
        var game = CommonGamePlay.Create(new TestEntity(maxHP: 10));

        game.Update(0.25);
        game.Update(0.75);

        Assert.Equal(1.0, game.Time, 6);
        Assert.Equal(0.75, game.DeltaTime, 6);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Update_Ignores_Invalid_Delta(double delta)
    {
        var game = CommonGamePlay.Create(new TestEntity(maxHP: 10), maxAP: 3, apRegeneration: 1.0);
        game.Update(0.5);

        game.Update(delta);

        Assert.Equal(0.5, game.Time, 6);
        Assert.Equal(0.5, game.DeltaTime, 6);
    }

    [Fact]
    public void Update_Drives_PlayerState_And_Scene()
    {
        var pc = new TestEntity(maxHP: 10);
        var enemy = new TestEntity(maxHP: 5);
        var game = CommonGamePlay.Create(pc, maxAP: 3, apRegeneration: 1.0);
        game.Scene.AddEntity(enemy);

        game.Update(0.5);

        // Create 会把 PC 放进场景，因此它也应该被更新。
        Assert.Equal(1, pc.UpdateCount);
        Assert.Equal(1, enemy.UpdateCount);
        Assert.Equal(0.5, enemy.LastDelta, 6);
    }

    [Fact]
    public void AP_Is_Updated_Before_Entities_So_Effects_See_This_Frames_Cost()
    {
        CommonGamePlay? game = null;
        var apSeenByEntity = -1;
        var pc = new TestEntity(maxHP: 10)
        {
            DoUpdateCallback = _ => apSeenByEntity = game!.PlayerState.AP,
        };

        game = CommonGamePlay.Create(pc, maxAP: 3, apRegeneration: 4.0);
        game.Update(0.25);

        Assert.Equal(1, apSeenByEntity);
    }

    [Fact]
    public void Hand_Assembled_GamePlay_Takes_The_Scene_As_Given()
    {
        var scene = new CommonScene();
        var playerState = new CommonPlayerState(new TestEntity(maxHP: 10), maxAP: 2);

        var gamePlay = NewGamePlay(scene, playerState);

        Assert.Empty(scene.Entities);
        Assert.Same(scene, gamePlay.Scene);
        Assert.Same(playerState, gamePlay.PlayerState);
    }

    [Fact]
    public void Create_Puts_The_Player_Into_The_Scene()
    {
        var pc = new TestEntity(maxHP: 10);

        var gamePlay = CommonGamePlay.Create(pc);

        Assert.Same(pc, gamePlay.PlayerState.PCEntity);
        Assert.Same(pc, Assert.Single(gamePlay.Scene.Entities));
    }

    [Fact]
    public void Create_Defaults_To_Six_Max_AP()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));

        Assert.Equal(6, gamePlay.PlayerState.MaxAP.Value);
    }

    [Fact]
    public void EventBus_Is_One_Cached_Instance_Per_GamePlay()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));

        Assert.Same(gamePlay.EventBus, gamePlay.EventBus);
        Assert.NotSame(gamePlay.EventBus, CommonGamePlay.Create(new TestEntity(maxHP: 10)).EventBus);
    }

    [Fact]
    public void Assembling_A_Scene_Attaches_It_To_The_GamePlay()
    {
        var scene = new CommonScene();

        var gamePlay = NewGamePlay(scene);

        Assert.Same(gamePlay, scene.GamePlay);
    }

    [Fact]
    public void Swapping_The_Scene_Moves_The_Attachment_And_Publishes_The_Replacement()
    {
        var first = new CommonScene();
        var gamePlay = NewGamePlay(first);
        var replaced = new List<GameSceneReplacedEvent>();
        gamePlay.EventBus.Subscribe<GameSceneReplacedEvent>(replaced.Add);

        var second = new CommonScene();
        gamePlay.Scene = second;

        var published = Assert.Single(replaced);
        Assert.Same(first, published.OldScene);
        Assert.Same(second, published.NewScene);

        // 先摘旧的再挂新的，两边不会有同时归属一局的时刻。
        Assert.Null(first.GamePlay);
        Assert.Same(gamePlay, second.GamePlay);
        Assert.Same(second, gamePlay.Scene);
    }

    [Fact]
    public void A_Scene_That_Was_Swapped_Out_Stops_Publishing()
    {
        var first = new CommonScene();
        var gamePlay = NewGamePlay(first);
        var second = new CommonScene();
        gamePlay.Scene = second;

        var added = new List<IEntity>();
        gamePlay.EventBus.Subscribe<EntityAddedEvent>(e => added.Add(e.Entity));

        first.AddEntity(new TestEntity(maxHP: 5));
        Assert.Empty(added);

        second.AddEntity(new TestEntity(maxHP: 5));
        Assert.Single(added);
    }

    [Fact]
    public void Setting_The_Same_Scene_Instance_Is_A_No_Op()
    {
        var scene = new CommonScene();
        var gamePlay = NewGamePlay(scene);
        var replaced = new List<GameSceneReplacedEvent>();
        gamePlay.EventBus.Subscribe<GameSceneReplacedEvent>(replaced.Add);
        var changed = new List<string?>();
        gamePlay.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        gamePlay.Scene = scene;

        Assert.Empty(replaced);
        Assert.Empty(changed);
        Assert.Same(gamePlay, scene.GamePlay);
    }

    [Fact]
    public void Swapping_The_Scene_Raises_PropertyChanged()
    {
        var gamePlay = NewGamePlay();
        var changed = new List<string?>();
        gamePlay.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        gamePlay.Scene = new CommonScene();

        Assert.Contains(nameof(IGamePlay.Scene), changed);
    }

    [Fact]
    public void Every_Kind_Of_Game_Event_Shares_The_GamePlayEvent_Base()
    {
        var gamePlay = CommonGamePlay.Create(new TestEntity(maxHP: 10));
        var events = new List<GamePlayEvent>();
        gamePlay.EventBus.Subscribe<GamePlayEvent>(events.Add);

        gamePlay.Scene.AddEntity(new TestEntity(maxHP: 5)); // EntityAddedEvent
        gamePlay.PlayerState.AP = 1;                        // APChangedEvent
        gamePlay.Scene = new CommonScene();                 // GameSceneReplacedEvent

        Assert.Collection(events,
            e => Assert.IsType<EntityAddedEvent>(e),
            e => Assert.IsType<APChangedEvent>(e),
            e => Assert.IsType<GameSceneReplacedEvent>(e));
    }
}
