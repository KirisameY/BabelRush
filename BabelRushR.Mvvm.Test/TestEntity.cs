using BabelRushR.Core.Entity;

using KirisameY.Registration.Data;

namespace BabelRushR.Mvvm.Test;

/// <summary>
///     不承载任何行为，仅用于驱动 <see cref="EntityBase"/> 的属性通知。
/// </summary>
internal sealed class TestEntity(int maxHP) : EntityBase(maxHP)
{
    public override RegKey DisplayKey => field ??= RegKey.From("core", "test");
}