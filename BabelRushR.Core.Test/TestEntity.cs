using BabelRushR.Core.Entity;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

/// <summary>
/// 只记录自己被更新了多少次，不做任何行为，方便断言更新是否真的发生。
/// </summary>
internal sealed class TestEntity(int maxHP) : EntityBase(maxHP)
{
    public int UpdateCount { get; private set; }

    public double LastDelta { get; private set; }

    public Action<double>? DoUpdateCallback { get; init; }

    public override RegKey DisplayKey => field ??= RegKey.From("core", "test");

    protected override void DoUpdate(double delta)
    {
        UpdateCount++;
        LastDelta = delta;
        DoUpdateCallback?.Invoke(delta);
    }
}