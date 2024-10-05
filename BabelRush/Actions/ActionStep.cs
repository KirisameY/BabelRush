using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public class ActionStep(ActionDelegate? actionDelegate)
{
    private ActionDelegate? ActionDelegate { get; } = actionDelegate;

    public void Act(Mob self, IReadOnlyList<Mob> targets, int value) => ActionDelegate?.Invoke(self, targets, value);

    public static ActionStep Default { get; } = new(null);
}