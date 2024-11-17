using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public class ActionInstance(ActionType type, int value)
{
    public ActionType Type { get; } = type;
    public int Value { get; set; } = value;

    public void Act(Mob self, IReadOnlyList<Mob> targets)
    {
        foreach (var actionItem in Type.ActionItems)
        {
            actionItem.Act(self, targets, Value);
        }
    }
}