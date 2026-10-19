using BabelRushR.Core.Scenery;
using BabelRushR.Mvvm.Infrastructure;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     场景的视图代理：把 <see cref="IScene.Entities"/> 包装成 <see cref="EntityViewModel"/> 的可通知集合。
/// </summary>
/// <remarks>
///     集合翻译交给 <see cref="CollectionWrapper.Collection"/>；本 VM 只负责选择包装类型并暴露源场景。
///     <see cref="Entities"/> 底层是字典值视图，<b>顺序无关</b>，视图不应依赖遍历顺序。
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