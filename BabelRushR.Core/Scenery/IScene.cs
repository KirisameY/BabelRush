using System.ComponentModel;

using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.SyncOrder;

namespace BabelRushR.Core.Scenery;

public interface IScene : INotifyPropertyChanged, IUpdatable
{
    INotifiableList<IEntity> Entities { get; }

    /// <summary>
    /// 实体已加入 <see cref="Entities"/>，返回该次加入的事件 Order。
    /// 由调用方决定何时 <c>Submit</c>，或先 <c>ContinueWith</c> 注册接续处理再链式提交。
    /// </summary>
    Order AddEntity(IEntity entity);

    /// <summary>
    /// 实体已移出 <see cref="Entities"/>，返回该次移除的事件 Order；实体本就不在场景中时返回 <c>null</c>。
    /// </summary>
    Order? RemoveEntity(IEntity entity);
}
