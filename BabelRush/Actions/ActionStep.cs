using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public class ActionStep(ActionDelegate? actionDelegate) // todo: 有点奇异搞笑了，回头改完注册表得把这个下了改个抽象
{
    private ActionDelegate? ActionDelegate { get; } = actionDelegate;

    public void Act(Mob self, IReadOnlyList<Mob> targets, int value) => ActionDelegate?.Invoke(self, targets, value);

    public static ActionStep Default { get; } = new(null);
}