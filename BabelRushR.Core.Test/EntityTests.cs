using BabelRushR.Core.Entity;

namespace BabelRushR.Core.Test;

public class EntityTests
{
    [Fact]
    public void HP_Is_Clamped_To_Range()
    {
        var entity = new TestEntity(maxHP: 10);

        entity.HP = 99;
        Assert.Equal(10, entity.HP);

        entity.HP = -5;
        Assert.Equal(0, entity.HP);
    }

    [Fact]
    public void IsAlive_Is_Computed_From_HP()
    {
        var entity = new TestEntity(maxHP: 10);

        Assert.True(entity.IsAlive);

        entity.HP = 1;
        Assert.True(entity.IsAlive);

        entity.HP = 0;
        Assert.False(entity.IsAlive);
    }

    [Fact]
    public void Entity_With_Zero_MaxHP_Is_Born_Dead()
    {
        var entity = new TestEntity(maxHP: 0);

        Assert.Equal(0, entity.HP);
        Assert.False(entity.IsAlive);
    }

    [Fact]
    public void Dead_Entity_Does_Not_Update()
    {
        var entity = new TestEntity(maxHP: 10);
        entity.HP = 0;

        entity.Update(0.016);

        Assert.Equal(0, entity.UpdateCount);
    }

    [Fact]
    public void Update_Forwards_Delta_To_DoUpdate()
    {
        var entity = new TestEntity(maxHP: 10);

        entity.Update(0.25);

        Assert.Equal(1, entity.UpdateCount);
        Assert.Equal(0.25, entity.LastDelta, 6);
    }

    [Fact]
    public void HP_And_Position_Changes_Raise_PropertyChanged()
    {
        var entity = new TestEntity(maxHP: 10);
        var changed = new List<string?>();
        entity.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        entity.HP = 5;
        entity.Position = 3.5;

        Assert.Contains(nameof(EntityBase.HP), changed);
        Assert.Contains(nameof(EntityBase.Position), changed);
    }

    [Fact]
    public void Unchanged_Value_Does_Not_Raise_PropertyChanged()
    {
        var entity = new TestEntity(maxHP: 10);
        var changed = new List<string?>();
        entity.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        entity.HP = 10;
        entity.Position = 0;

        Assert.Empty(changed);
    }
}
