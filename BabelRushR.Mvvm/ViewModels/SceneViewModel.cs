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
public sealed class SceneViewModel(IScene scene) : ViewModelBase
{
    public IScene SourceScene => scene;

    /// <summary>
    ///     场景中的全部实体。
    /// </summary>
    /// <remarks>
    ///     <b>顺序无关</b>：底层是字典的值视图，视图不应依赖遍历顺序。
    /// </remarks>
    public IReadOnlyNotifiableCollection<EntityViewModel> Entities => field ??= CollectionWrapper.Collection(
        scene.Entities, e => new EntityViewModel(e), Track
    );
}