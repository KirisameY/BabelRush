using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;

using KirisameY.EventBus;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.SyncOrder;

namespace BabelRushR.Core.Scenery;

public class CommonScene(IEventBus eventBus) : ObservableObject, IScene
{
    // 复用缓冲，避免每帧产生垃圾。
    private readonly List<IEntity> _updateBuffer = [];
    private readonly List<IEntity> _deadBuffer = [];

    public IEventBus EventBus => eventBus;

    public INotifiableList<IEntity> Entities { get; } = new NotifiableList<IEntity>();

    public Order AddEntity(IEntity entity)
    {
        Entities.Add(entity);
        return eventBus.OrderPost(new EntityAddedEvent(entity));
    }

    public Order? RemoveEntity(IEntity entity)
        => Entities.Remove(entity) ? eventBus.OrderPost(new EntityRemovedEvent(entity)) : null;

    public void Update(double delta)
    {
        // 1. 先对快照统一推进。实体在 OnUpdate 里增删 Entities 不会破坏这里的遍历。
        _updateBuffer.Clear();
        _updateBuffer.AddRange(Entities);
        foreach (var entity in _updateBuffer) entity.Update(delta);

        // 2. 帧末清扫死亡实体。
        _deadBuffer.Clear();
        foreach (var entity in _updateBuffer)
        {
            if (!entity.IsAlive) _deadBuffer.Add(entity);
        }
        if (_deadBuffer.Count == 0) return;

        // 3. 状态全部变更完毕后再分发。Submit 是同步的，
        //    若边改边发，处理函数里的增删会破坏正在进行的遍历。
        foreach (var entity in _deadBuffer) Entities.Remove(entity);

        // Died 派生自 Removed，基类分发会把两者一起送达，不需要重复 post。
        foreach (var entity in _deadBuffer) eventBus.OrderPost(new EntityDiedEvent(entity)).Submit();
    }
}
