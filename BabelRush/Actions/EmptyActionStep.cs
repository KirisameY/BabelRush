using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public sealed class EmptyActionStep : ActionStep
{
    public override void Act(Mob self, IReadOnlyList<Mob> targets, int value) { }
}