using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;

using KirisameY.EventBus.Bus;

namespace BabelRushR.Core.Test;

public class GamePlayTests
{
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
    public void Injected_Constructor_Leaves_The_Scene_Untouched()
    {
        var bus = new SimpleEventBus();
        var scene = new CommonScene(bus);
        var playerState = new CommonPlayerState(new TestEntity(maxHP: 10), bus, maxAP: 2);

        var game = new CommonGamePlay(scene, playerState, bus);

        Assert.Empty(scene.Entities);
        Assert.Same(scene, game.Scene);
        Assert.Same(playerState, game.PlayerState);
        Assert.Same(bus, game.EventBus);
    }
}
