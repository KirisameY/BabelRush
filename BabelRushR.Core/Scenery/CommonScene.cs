using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;

using KirisameY.EventBus;
using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Scenery;

public class CommonScene(IEventBus eventBus) : ObservableObject, IScene
{
    // 复用缓冲，避免每帧产生垃圾。
    private readonly List<IEntity> _updateBuffer = [];
    private readonly List<IEntity> _deadBuffer = [];

    public IEventBus EventBus => eventBus;

    public INotifiableList<IEntity> Entities { get; } = new NotifiableList<IEntity>();

    public bool AddEntity(IEntity entity)
    {
        if (Entities.Contains(entity)) return false;
        Entities.Add(entity);
        eventBus.Publish(new EntityAddedEvent(entity));
        return true;
    }

    public bool RemoveEntity(IEntity entity)
    {
        if (!Entities.Remove(entity)) return false;

        eventBus.Publish(new EntityRemovedEvent(entity));
        return true;
    }

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

        // 3. 状态全部变更完毕后再分发。Publish 是同步的，
        //    若边改边发，处理函数里的增删会破坏正在进行的遍历。
        foreach (var entity in _deadBuffer) Entities.Remove(entity);

        foreach (var entity in _deadBuffer) eventBus.Publish(new EntityDiedEvent(entity));
    }
}