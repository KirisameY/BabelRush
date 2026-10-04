using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;

using BabelRushR.Core.Entity;
using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.ViewModels;

/// <summary>
///     单个实体的视图代理：把 <see cref="IEntity"/> 面向逻辑的数据形态翻译成视图直接可用的形态。
/// </summary>
/// <remarks>
///     与 Core 一一对应，实例由 <see cref="SceneViewModel"/> 独占创建与释放，
///     以保证同一个实体在整棵 VM 树里只有一份代理。
/// </remarks>
public sealed class EntityViewModel : ViewModelBase
{
    /// <summary>
    ///     Core 属性名 → 该变化引发的更新与需要一并通知的属性名。
    /// </summary>
    /// <remarks>
    ///     Core 只发它自己那层属性的变化，派生量（比例之类）与计算属性都要在这里接出来。
    ///     本 VM 的值都是现取的，无需就地更新，所以更新列表一律为空。
    /// </remarks>
    private static readonly PropertyChangeReactor<EntityViewModel> Reactions = new(
        new Dictionary<string, (ImmutableArray<Action<EntityViewModel>> Updates, ImmutableArray<string> Notifications)>
        {
            [nameof(IEntity.HP)]       = ([], [nameof(HP), nameof(HPRatio)]),
            [nameof(IEntity.MaxHP)]    = ([], [nameof(MaxHP), nameof(HPRatio)]),
            [nameof(IEntity.IsAlive)]  = ([], [nameof(IsAlive)]),
            [nameof(IEntity.Position)] = ([], [nameof(Position)]),
        }.ToFrozenDictionary()
    );

    private readonly IEntity _entity;

    public EntityViewModel(IEntity entity)
    {
        _entity = entity;
        Track(entity.Subscribe(OnEntityPropertyChanged));
    }

    public int HP => _entity.HP;

    public int MaxHP => _entity.MaxHP.Value;

    /// <summary>
    ///     血量比例，供血条一类的控件直接使用。
    /// </summary>
    public double HPRatio => MaxHP > 0 ? HP / (double)MaxHP : 0;

    public bool IsAlive => _entity.IsAlive;

    public double Position => _entity.Position;

    private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        Reactions.Forward(e.PropertyName, this, OnPropertyChanged);
}