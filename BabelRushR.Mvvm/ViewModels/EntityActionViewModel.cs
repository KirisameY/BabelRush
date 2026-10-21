using System.Collections.Frozen;
using System.Collections.Immutable;

using BabelRushR.Core.EntityAction;
using BabelRushR.Mvvm.Infrastructure;

using KirisameY.GenericUtils;

namespace BabelRushR.Mvvm.ViewModels;

public class EntityActionViewModel(IEntityAction action) : ViewModelBase<EntityActionViewModel>(action, Reactions)
{
    private static readonly PropertyChangeReactor<EntityActionViewModel> Reactions = new(
        new Dictionary<string, (ImmutableArray<Action<EntityActionViewModel>> Updates, ImmutableArray<string> Notifications)>
        {
            [nameof(IValuedEntityAction.Value)] = ([], [nameof(BaseValue), nameof(Value)]),
        }.ToFrozenDictionary()
    );

    public Optional<int> BaseValue => action is IValuedEntityAction valued ? Optional.From(valued.Value.BaseValue) : Optional.Empty<int>();
    public Optional<int> Value => action is IValuedEntityAction valued ? Optional.From(valued.Value.Value) : Optional.Empty<int>();
}