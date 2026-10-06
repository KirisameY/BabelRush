using BabelRushR.Core.Entity;
using BabelRushR.Core.Scenery;
using BabelRushR.Mvvm.Infrastructure;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.NotifiableCollections.EventArgs;
using KirisameY.Relinq.Extensions;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     场景的视图代理：维护 <see cref="IEntity"/> 到 <see cref="EntityViewModel"/> 的映射，
///     并把实体的增删以可通知集合暴露给视图。
/// </summary>
/// <remarks>
///     本 VM 只负责在映射表里增删；从映射表到视图的通知链路由库自身完成
///     （映射表的值视图本身即是 <see cref="IReadOnlyNotifiableCollection{T}"/>），此处不消费集合事件。
/// </remarks>
public sealed class SceneViewModel : ViewModelBase
{
    public IScene SourceScene { get; }
    private readonly NotifiableDictionary<IEntity, EntityViewModel> _entities = [];

    public SceneViewModel(IScene scene)
    {
        SourceScene = scene;
        Add(scene.Entities);

        scene.Entities.ListUpdated += OnEntitiesUpdated;
        Track(new ActionDisposable(() => scene.Entities.ListUpdated -= OnEntitiesUpdated));
    }

    /// <summary>
    ///     场景中的全部实体。
    /// </summary>
    /// <remarks>
    ///     <b>顺序无关</b>：底层是字典的值视图，视图不应依赖遍历顺序。
    /// </remarks>
    public IReadOnlyNotifiableCollection<EntityViewModel> Entities => field ??= _entities.Values;

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
                Add(added.AddedItems);
                break;
            }
            // Cleared 派生自 Removed，语义一致，一并在此处理
            case IListItemRemovedEventArgs<IEntity> removed:
            {
                Remove(removed.RemovedItems);
                break;
            }
            case IListItemReplacedEventArgs<IEntity> replaced:
            {
                Remove(replaced.OldItems);
                Add(replaced.NewItems);
                break;
            }
            case IListResetEventArgs<IEntity> reset:
            {
                _entities.Clear();
                Add(reset.ListView);
                break;
            }
            // 仅关注元素增删替换，忽略其余情形
        }
    }

    private void Add(IEntity entity) => _entities[entity] = new EntityViewModel(entity);

    private void Add(IEnumerable<IEntity> entities) => _entities.AddRange(
        entities.Select(e => KeyValuePair.Create(e, new EntityViewModel(e)))
    );

    private void Remove(IEntity entity)
    {
        // 先移出再释放：避免视图在收到移除通知时拿到一个已经断掉订阅的 VM。
        if (!_entities.Remove(entity, out var viewModel)) return;
        viewModel.Dispose();
    }

    private void Remove(IEnumerable<IEntity> entities)
    {
        var removed = _entities.RemoveRange(entities);
        removed.ForEach(p => p.Value.Dispose());
    }


    protected override void DisposeCore()
    {
        var snapshot = _entities.Values.ToArray();
        _entities.Clear();
        foreach (var viewModel in snapshot) viewModel.Dispose();
    }
}