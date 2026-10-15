using System.Collections.Frozen;
using System.Collections.Immutable;

using BabelRushR.Core.EntityAction;
using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.ViewModels;

public class EntityActionViewModel(IEntityAction action) : ViewModelBase<EntityActionViewModel>(action, Reactions)
{
    private static readonly PropertyChangeReactor<EntityActionViewModel> Reactions = new(
        new Dictionary<string, (ImmutableArray<Action<EntityActionViewModel>> Updates, ImmutableArray<string> Notifications)>
            { }.ToFrozenDictionary()
    );
}