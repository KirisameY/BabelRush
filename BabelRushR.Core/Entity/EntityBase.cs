using BabelRushR.Core.Common;
using BabelRushR.Core.Numeric;

using KirisameY.Numeric;

namespace BabelRushR.Core.Entity;

/// <summary>
/// 实体基类：负责 HP 钳制、存活判定与属性通知，行为由子类重写 <see cref="DoUpdate"/> 实现。
/// </summary>
public abstract class EntityBase(int maxHP) : ObservableObject, IEntity
{
    // 初值单独存一份：主构造参数若同时被初始化器和成员体引用，会触发 CS9124。
    private readonly int _baseMaxHP = maxHP;

    public IModifierEditableNumeric<int, CommonNumericModifyOrder> MaxHP =>
        field ??= INumeric.CreateReadonly<int, CommonNumericModifyOrder>(_baseMaxHP)
                          .WithUpdateHandler((_, _) => HP = HP) // 更新时重新赋值HP触发钳制
                          .WithUpdateHandler(PropertyChangedHandler());

    public int HP
    {
        get;
        set
        {
            if (SetProperty(ref field, Math.Clamp(value, 0, MaxHP.Value)))
                OnPropertyChanged(nameof(IsAlive));
        }
    } = maxHP;

    public bool IsAlive => HP > 0;

    public double Position
    {
        get;
        set => SetProperty(ref field, value);
    }

    public void Update(double delta)
    {
        if (!IsAlive) return;
        DoUpdate(delta);
    }

    protected virtual void DoUpdate(double delta) { }
}