using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public abstract class ActionStep
{
    public abstract void Act(Mob self, IReadOnlyList<Mob> targets, int value);

    public static ActionStep Default { get; } = new EmptyActionStep();
}