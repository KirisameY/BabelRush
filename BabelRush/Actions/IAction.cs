using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Actions;

public interface IAction
{
    IActionType Type { get; }
    int Value { get; }
    
    void Act(IMob self, IEnumerable<IMob> targets);
}