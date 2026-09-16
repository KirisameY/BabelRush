using BabelRushR.Core.Common;

namespace BabelRushR.Core.Entity;

/// <summary>
/// 实体基类：负责 HP 钳制、存活判定与属性通知，行为由子类重写 <see cref="DoUpdate"/> 实现。
/// </summary>
public abstract class EntityBase(int maxHP) : ObservableObject, IEntity
{
    public int MaxHP { get; } = maxHP;

    public int HP
    {
        get;
        set => SetProperty(ref field, Math.Clamp(value, 0, MaxHP));
    } = maxHP;

    public bool IsAlive => HP > 0;

    public double Position
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// 已死亡的实体不再推进逻辑，等待所属场景在本帧末尾清扫。
    /// </summary>
    public void Update(double delta)
    {
        if (!IsAlive) return;
        DoUpdate(delta);
    }

    protected virtual void DoUpdate(double delta) { }
}