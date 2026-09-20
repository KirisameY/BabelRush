using BabelRushR.Core.Entity;
using BabelRushR.Core.Scenery;
using BabelRushR.Mvvm.Infrastructure;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     场景的视图代理：维护 <see cref="IEntity"/> 到 <see cref="EntityViewModel"/> 的映射，
///     并把实体的增删以标准通知模型暴露给视图。
/// </summary>
/// <remarks>
///     本 VM 只负责在映射表里增删；从映射表到视图的通知链路（<c>Values</c> →
///     <c>AsReadOnlyObservableCollection</c>）由库自身完成，此处不消费集合事件。
/// </remarks>
public sealed class SceneViewModel : ViewModelBase
{
    private readonly NotifiableDictionary<IEntity, EntityViewModel> _entities = [];

    /// <summary>
    ///     场景中的全部实体。
    /// </summary>
    /// <remarks>
    ///     <b>顺序无关</b>：底层是字典的值视图，视图不应依赖遍历顺序。
    /// </remarks>
    public IReadOnlyObservableCollection<EntityViewModel> Entities { get; }

    public SceneViewModel(IScene scene)
    {
        Entities = _entities.Values.AsReadOnlyObservableCollection();

        foreach (var entity in scene.Entities) Add(entity);

        scene.Entities.ListUpdated += OnEntitiesUpdated;
        Track(new ActionDisposable(() => scene.Entities.ListUpdated -= OnEntitiesUpdated));
    }

    /// <summary>
    ///     取得实体对应的 VM，不存在时返回 <see langword="null"/>。
    /// </summary>
    public EntityViewModel? Find(IEntity entity) => _entities.GetValueOrDefault(entity);

    private void OnEntitiesUpdated(object? sender, ListUpdateEventArgs<IEntity> e)
    {
        switch (e)
        {
            case IListItemAddedEventArgs<IEntity> added:
            {
                foreach (var entity in added.AddedItems) Add(entity);
                break;
            }
            // Cleared 派生自 Removed，语义一致，一并在此处理
            case IListItemRemovedEventArgs<IEntity> removed:
            {
                foreach (var entity in removed.RemovedItems) Remove(entity);
                break;
            }
            case IListItemReplacedEventArgs<IEntity> replaced:
            {
                for (var i = 0; i < replaced.OldItems.Count; i++)
                {
                    Remove(replaced.OldItems[i]);
                    Add(replaced.NewItems[i]);
                }
                break;
            }
            // 元素无增删，仅整表重排；顺序对视图无意义，忽略
            case IListSortedEventArgs<IEntity>: break;
        }
    }

    private void Add(IEntity entity) => _entities[entity] = new EntityViewModel(entity);

    private void Remove(IEntity entity)
    {
        // 先移出再释放：避免视图在收到移除通知时拿到一个已经断掉订阅的 VM。
        if (!_entities.Remove(entity, out var viewModel)) return;
        viewModel.Dispose();
    }
}