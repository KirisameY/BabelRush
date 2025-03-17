using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public class CommonActionStep(ActionDelegate action) : ActionStep
{
    public override void Act(Mob self, IReadOnlyList<Mob> targets, int value) => action.Invoke(self, targets, value);
}