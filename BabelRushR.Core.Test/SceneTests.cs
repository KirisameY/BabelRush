using BabelRushR.Core.Entity;
using BabelRushR.Core.Scenery;

using KirisameY.EventBus.Bus;

namespace BabelRushR.Core.Test;

public class SceneTests
{
    [Fact]
    public void AddEntity_Reports_The_Entity_Immediately_But_Does_Not_Publish_Until_Submitted()
    {
        var scene = new CommonScene(new AutoEventBus());
        var added = new List<IEntity>();
        scene.EventBus.Subscribe<EntityAddedEvent>(e => added.Add(e.Entity));

        var entity = new TestEntity(maxHP: 5);
        var order = scene.AddEntity(entity);

        Assert.Contains(entity, scene.Entities);
        Assert.Empty(added);

        order.Submit();

        Assert.Equal(new IEntity[] { entity }, added);
    }

    [Fact]
    public void AddEntity_Order_Can_Chain_A_Continuation_Before_Submitting()
    {
        var scene = new CommonScene(new AutoEventBus());
        var log = new List<string>();
        scene.EventBus.Subscribe<EntityAddedEvent>(_ => log.Add("published"));

        scene.AddEntity(new TestEntity(maxHP: 5))
             .ContinueWith(() => log.Add("continued"))
             .Submit();

        Assert.Equal(new[] { "published", "continued" }, log);
    }

    [Fact]
    public void RemoveEntity_Publishes_And_Returns_Null_When_Entity_Is_Absent()
    {
        var scene = new CommonScene(new AutoEventBus());
        var removed = new List<IEntity>();
        scene.EventBus.Subscribe<EntityRemovedEvent>(e => removed.Add(e.Entity));

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity).Submit();

        scene.RemoveEntity(entity)!.Submit();
        Assert.Equal(new IEntity[] { entity }, removed);

        Assert.Null(scene.RemoveEntity(entity));
        Assert.Single(removed);
    }

    [Fact]
    public void Dead_Entity_Is_Swept_At_End_Of_Frame()
    {
        var scene = new CommonScene(new AutoEventBus());
        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity).Submit();

        entity.HP = 0;
        scene.Update(0.016);

        Assert.DoesNotContain(entity, scene.Entities);
        Assert.Equal(0, entity.UpdateCount);
    }

    [Fact]
    public void EntityEvent_Receives_Events_Through_Base_Class_Dispatch()
    {
        var scene = new CommonScene(new AutoEventBus());
        var entityEvents = new List<EntityEvent>();
        scene.EventBus.Subscribe<EntityEvent>(entityEvents.Add);

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity).Submit();

        Assert.IsType<EntityAddedEvent>(Assert.Single(entityEvents));
    }

    [Fact]
    public void Sweeping_Posts_Only_Died_Which_Also_Reaches_Removed_And_Entity_Subscribers()
    {
        var scene = new CommonScene(new AutoEventBus());
        var entityEvents = new List<EntityEvent>();
        var removed = new List<IEntity>();
        var died = new List<IEntity>();
        scene.EventBus.Subscribe<EntityEvent>(entityEvents.Add);
        scene.EventBus.Subscribe<EntityRemovedEvent>(e => removed.Add(e.Entity));
        scene.EventBus.Subscribe<EntityDiedEvent>(e => died.Add(e.Entity));

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity).Submit();
        entityEvents.Clear(); // 丢弃加入事件

        entity.HP = 0;
        scene.Update(0.016);

        Assert.Equal(new IEntity[] { entity }, removed);
        Assert.Equal(new IEntity[] { entity }, died);

        // 只 post 了 Died 一个事件，经基类分发同时到达三个订阅者。
        Assert.IsType<EntityDiedEvent>(Assert.Single(entityEvents));
    }

    [Fact]
    public void Entity_Killed_During_Update_Is_Swept_In_The_Same_Frame()
    {
        var scene = new CommonScene(new AutoEventBus());
        var victim = new TestEntity(maxHP: 5);
        var killer = new TestEntity(maxHP: 5) { DoUpdateCallback = _ => victim.HP = 0 };
        scene.AddEntity(victim).Submit();
        scene.AddEntity(killer).Submit();

        scene.Update(0.016);

        Assert.DoesNotContain(victim, scene.Entities);
        Assert.Contains(killer, scene.Entities);
    }

    [Fact]
    public void Died_Event_Is_Published_Only_After_The_Entity_Is_Removed()
    {
        var scene = new CommonScene(new AutoEventBus());
        var stillPresentWhenNotified = true;
        scene.EventBus.Subscribe<EntityDiedEvent>(e => stillPresentWhenNotified = scene.Entities.Contains(e.Entity));

        var entity = new TestEntity(maxHP: 5);
        scene.AddEntity(entity).Submit();

        entity.HP = 0;
        scene.Update(0.016);

        Assert.False(stillPresentWhenNotified);
    }

    [Fact]
    public void Entity_Added_During_Update_Is_Not_Updated_Until_Next_Frame()
    {
        var scene = new CommonScene(new AutoEventBus());
        var latecomer = new TestEntity(maxHP: 5);
        var adder = new TestEntity(maxHP: 5) { DoUpdateCallback = _ => scene.AddEntity(latecomer).Submit() };
        scene.AddEntity(adder).Submit();

        scene.Update(0.016);

        Assert.Contains(latecomer, scene.Entities);
        Assert.Equal(0, latecomer.UpdateCount);
    }

    [Fact]
    public void Entity_Removed_By_Another_Entity_Does_Not_Break_Iteration()
    {
        var scene = new CommonScene(new AutoEventBus());
        var victim = new TestEntity(maxHP: 5);
        var remover = new TestEntity(maxHP: 5) { DoUpdateCallback = _ => scene.RemoveEntity(victim)?.Submit() };
        scene.AddEntity(victim).Submit();
        scene.AddEntity(remover).Submit();

        scene.Update(0.016);

        Assert.DoesNotContain(victim, scene.Entities);
        Assert.Equal(1, remover.UpdateCount);
    }
}
