using System.Collections.Immutable;

namespace BabelRushR.Mvvm.Infrastructure;

public class PropertyChangeReactor<TObj>(IReadOnlyDictionary<string, ImmutableArray<Action<TObj>>> dependencies)
{
    private readonly ImmutableArray<Action<TObj>> _allProperties =
        [..dependencies.Values.SelectMany(names => names).Distinct()];

    public void React(string? propertyName, TObj instance)
    {
        ImmutableArray<Action<TObj>>? actions = string.IsNullOrEmpty(propertyName) ? _allProperties :
            dependencies.TryGetValue(propertyName, out var affected) ? affected : null;

        if (actions is null) return;
        foreach (var action in actions) action.Invoke(instance);
    }
}