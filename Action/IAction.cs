using System.Collections.Generic;

using BabelRush.Mob;

namespace BabelRush.Action;

public interface IAction
{
    IActionType Type { get; }
    int Value { get; }
    
    void Act(IMob self, IEnumerable<IMob> targets);
}