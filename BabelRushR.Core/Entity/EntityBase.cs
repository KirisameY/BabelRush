using BabelRushR.Core.Entity.Health;
using BabelRushR.Core.Scenery.Objects;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Entity;

/// <summary>
/// 实体基类：负责 HP 钳制、存活判定与属性通知，行为由子类重写 <see cref="DoUpdate"/> 实现。
/// </summary>
public abstract class EntityBase(int maxHP) : SceneObjectBase, IEntity
{
    public abstract RegKey DisplayKey { get; }

    public IEntityHealth Health => field ??=
        new CommonEntityHealth(maxHP)
           .WithUpdatedHandler((_, args) => OnPropertyChanged(nameof(IsAlive)));

    public bool IsAlive => Health.HP > 0;

    public void Update(double delta)
    {
        if (!IsAlive) return;
        DoUpdate(delta);
    }

    protected virtual void DoUpdate(double delta) { }
}