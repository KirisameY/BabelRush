using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public abstract class Action
{
    public abstract ActionType Type { get; }
    public abstract int Value { get; }
    public abstract void Act(Mob self, IReadOnlySet<Mob> targets);
}