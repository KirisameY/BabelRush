using BabelRushR.Core.Entity;

namespace BabelRushR.Mvvm.Test;

/// <summary>
///     不承载任何行为，仅用于驱动 <see cref="EntityBase"/> 的属性通知。
/// </summary>
internal sealed class TestEntity(int maxHP) : EntityBase(maxHP);
