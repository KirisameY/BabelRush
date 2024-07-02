using System.Collections.Generic;

using BabelRush.Entity;

namespace BabelRush.Action;

public interface IAction
{
    IActionType Type { get; }
    int Value { get; }
    
    void Act(IEntity self, IEnumerable<IEntity> targets);
}