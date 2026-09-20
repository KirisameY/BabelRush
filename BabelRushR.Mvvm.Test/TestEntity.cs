using BabelRushR.Core.Entity;

namespace BabelRushR.Mvvm.Test;

/// <summary>
///     不承载任何行为，仅用于驱动 <see cref="EntityBase"/> 的属性通知。
/// </summary>
internal sealed class TestEntity(int maxHP) : EntityBase(maxHP)
{
    /// <summary>发出"全部属性失效"的空属性名通知。</summary>
    public void NotifyAllPropertiesChanged() => OnPropertyChanged(null);

    /// <summary>发出一个不属于 <see cref="IEntity"/> 的属性名通知。</summary>
    public void NotifyUnknownPropertyChanged() => OnPropertyChanged("NotAnEntityProperty");
}
