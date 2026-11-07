using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;

using BabelRushR.Core.Entity.Health;
using BabelRushR.Mvvm.Infrastructure;

namespace BabelRushR.Mvvm.ViewModels;

public class HealthBarViewModel(IEntityHealth health) : ViewModelBase<HealthBarViewModel>(health, Reactions)
{
    private static readonly PropertyChangeReactor<HealthBarViewModel> Reactions = new(
        new Dictionary<string, (ImmutableArray<Action<HealthBarViewModel>> Updates, ImmutableArray<string> Notifications)>
        {
            [nameof(IEntityHealth.MaxHP)]  = ([], [nameof(MaxHP), nameof(HPRatio), nameof(ShieldRatio)]),
            [nameof(IEntityHealth.HP)]     = ([], [nameof(HP), nameof(HPRatio)]),
            [nameof(IEntityHealth.Shield)] = ([], [nameof(Shield), nameof(ShieldRatio)]),
            [nameof(IEntityHealth.Block)]  = ([], [nameof(Block)]),
        }.ToFrozenDictionary()
    );

    public int MaxHP => health.MaxHP.Value;

    public int HP => health.HP;
    public int Shield => health.Shield;
    public int Block => health.Block;


    public double HPRatio => MaxHP > 0 ? HP / (double)MaxHP : 0;
    public double ShieldRatio => MaxHP > 0 ? Shield / (double)MaxHP : 0;
}