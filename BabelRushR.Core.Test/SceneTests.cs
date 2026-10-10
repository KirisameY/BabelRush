using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Test;

public class SceneTests
{
    /// <summary>
    ///     造一个已经挂到 <see cref="CommonGamePlay"/> 上的场景。
    ///     场景自身不再持有事件总线，事件一律经由宿主发布。
    /// </summary>
    private static (CommonScene Scene, IGamePlay GamePlay) NewAttachedScene()
    {
        var scene = new CommonScene();
        var gamePlay = new CommonGamePlay
        {
            Scene       = scene,
            PlayerState = new CommonPlayerState(new TestEntity(maxHP: 10)),
        };
        return (scene, gamePlay);
    }

    [Fact]
    public void AddEntity_Adds_And_Publishes_Immediately()
    {
        var (scene, gamePlay) = NewAttachedScene();
        var added = new List<IEntity>();
        gamePlay.EventBus.Subscribe<EntityAddedEvent>(e => added.Add(e.Entity));

        var entity = new TestEntity(maxHP: 5);

        Assert.True(scene.AddEntity(entity));

        Assert.Contains(entity, scene.Entities);
        Assert.Equal(new IEntity[] { entity }, added);
    }

    [Fact]
    public void AddEntity_Rejects_A_Duplicate_Without_Publishing_Again()
    {
        var (scene, gamePlay) = NewAttachedScene();
        var added = new List<IEntity>();
        gamePlay.EventBus.Subscribe<EntityAddedEvent>(e => added.Add(e.Entity));

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        Assert.False(scene.AddEntity(entity));
        Assert.Single(scene.Entities);
        Assert.Single(added);
    }

    [Fact]
    public void Added_Event_Is_Published_Only_After_The_Entity_Is_In_The_Scene()
    {
        var (scene, gamePlay) = NewAttachedScene();
        var presentWhenNotified = false;
        gamePlay.EventBus.Subscribe<EntityAddedEvent>(e => presentWhenNotified = scene.Entities.Contains(e.Entity));

        scene.AddEntity(new TestEntity(maxHP: 5));

        Assert.True(presentWhenNotified);
    }

    [Fact]
    public void RemoveEntity_Publishes_And_Returns_False_When_Entity_Is_Absent()
    {
        var (scene, gamePlay) = NewAttachedScene();
        var removed = new List<IEntity>();
        gamePlay.EventBus.Subscribe<EntityRemovedEvent>(e => removed.Add(e.Entity));

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        Assert.True(scene.RemoveEntity(entity));
        Assert.Equal(new IEntity[] { entity }, removed);

        Assert.False(scene.RemoveEntity(entity));
        Assert.Single(removed);
    }

    [Fact]
    public void Dead_Entity_Is_Swept_At_End_Of_Frame()
    {
        var (scene, _) = NewAttachedScene();
        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        entity.HP = 0;
        scene.Update(0.016);

        Assert.DoesNotContain(entity, scene.Entities);
        Assert.Equal(0, entity.UpdateCount);
    }

    [Fact]
    public void A_Scene_Without_A_GamePlay_Publishes_Nothing()
    {
        // 场景不再自持总线：没有宿主时增删只是静默生效，不该抛异常。
        var scene = new CommonScene();
        var entity = new TestEntity(maxHP: 5);

        Assert.Null(scene.GamePlay);
        Assert.True(scene.AddEntity(entity));
        Assert.True(scene.RemoveEntity(entity));
    }

    [Fact]
    public void A_Scene_Without_A_GamePlay_Still_Sweeps_Dead_Entities()
    {
        // 清扫只依赖实体自身状态，与有没有事件总线无关。
        var scene = new CommonScene();
        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        entity.HP = 0;
        scene.Update(0.016);

        Assert.DoesNotContain(entity, scene.Entities);
    }

    [Fact]
    public void EntityEvent_Receives_Events_Through_Base_Class_Dispatch()
    {
        var (scene, gamePlay) = NewAttachedScene();
        var entityEvents = new List<EntityEvent>();
        gamePlay.EventBus.Subscribe<EntityEvent>(entityEvents.Add);

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        Assert.IsType<EntityAddedEvent>(Assert.Single(entityEvents));
    }

    [Fact]
    public void Entity_Events_Also_Reach_GamePlayEvent_Subscribers()
    {
        var (scene, gamePlay) = NewAttachedScene();
        var events = new List<GamePlayEvent>();
        gamePlay.EventBus.Subscribe<GamePlayEvent>(events.Add);

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);
        scene.RemoveEntity(entity);

        // 实体事件如今挂在 GamePlayEvent 这个统一基类下，按基类订阅即可拿到全部游戏事件。
        Assert.Collection(events,
            e => Assert.IsType<EntityAddedEvent>(e),
            e => Assert.IsType<EntityRemovedEvent>(e));
    }

    [Fact]
    public void Sweeping_Publishes_Only_Died_Which_Also_Reaches_Removed_And_Entity_Subscribers()
    {
        var (scene, gamePlay) = NewAttachedScene();
        var entityEvents = new List<EntityEvent>();
        var removed = new List<IEntity>();
        var died = new List<IEntity>();
        gamePlay.EventBus.Subscribe<EntityEvent>(entityEvents.Add);
        gamePlay.EventBus.Subscribe<EntityRemovedEvent>(e => removed.Add(e.Entity));
        gamePlay.EventBus.Subscribe<EntityDiedEvent>(e => died.Add(e.Entity));

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);
        entityEvents.Clear(); // 丢弃加入事件

        entity.HP = 0;
        scene.Update(0.016);

        Assert.Equal(new IEntity[] { entity }, removed);
        Assert.Equal(new IEntity[] { entity }, died);

        // 只发布了 Died 一个事件，经基类分发同时到达三个订阅者。
        Assert.IsType<EntityDiedEvent>(Assert.Single(entityEvents));
    }

    [Fact]
    public void Entity_Killed_During_Update_Is_Swept_In_The_Same_Frame()
    {
        var (scene, _) = NewAttachedScene();
        var victim = new TestEntity(maxHP: 5);
        var killer = new TestEntity(maxHP: 5) { DoUpdateCallback = _ => victim.HP = 0 };
        scene.AddEntity(victim);
        scene.AddEntity(killer);

        scene.Update(0.016);

        Assert.DoesNotContain(victim, scene.Entities);
        Assert.Contains(killer, scene.Entities);
    }

    [Fact]
    public void Died_Event_Is_Published_Only_After_The_Entity_Is_Removed()
    {
        var (scene, gamePlay) = NewAttachedScene();
        var stillPresentWhenNotified = true;
        gamePlay.EventBus.Subscribe<EntityDiedEvent>(e => stillPresentWhenNotified = scene.Entities.Contains(e.Entity));

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity);

        entity.HP = 0;
        scene.Update(0.016);

        Assert.False(stillPresentWhenNotified);
    }

    [Fact]
    public void Entity_Added_During_Update_Is_Not_Updated_Until_Next_Frame()
    {
        var (scene, _) = NewAttachedScene();
        var latecomer = new TestEntity(maxHP: 5);
        var adder = new TestEntity(maxHP: 5) { DoUpdateCallback = _ => scene.AddEntity(latecomer) };
        scene.AddEntity(adder);

        scene.Update(0.016);

        Assert.Contains(latecomer, scene.Entities);
        Assert.Equal(0, latecomer.UpdateCount);
    }

    [Fact]
    public void Entity_Removed_By_Another_Entity_Does_Not_Break_Iteration()
    {
        var (scene, _) = NewAttachedScene();
        var victim = new TestEntity(maxHP: 5);
        var remover = new TestEntity(maxHP: 5) { DoUpdateCallback = _ => scene.RemoveEntity(victim) };
        scene.AddEntity(victim);
        scene.AddEntity(remover);

        scene.Update(0.016);

        Assert.DoesNotContain(victim, scene.Entities);
        Assert.Equal(1, remover.UpdateCount);
    }
}
