using System.ComponentModel;

using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Scenery;

public interface IScene : INotifyPropertyChanged, IUpdatable
{
    INotifiableList<IEntity> Entities { get; }

    /// <summary>
    /// 向 <see cref="Entities"/> 中加入给定的实体。
    /// </summary>
    /// <returns>
    /// 若加入为 <c>true</c>，反之（已存在于列表中）为 <c>false</c>。
    /// </returns>
    bool AddEntity(IEntity entity);

    /// <summary>
    /// 从 <see cref="Entities"/> 移除该实体。
    /// </summary>
    /// <returns>
    /// 若成功找到并移除则为 <c>true</c>，反之为 <c>false</c>。
    /// </returns>
    bool RemoveEntity(IEntity entity);
}
