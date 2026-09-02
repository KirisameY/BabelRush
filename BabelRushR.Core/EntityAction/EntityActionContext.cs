using BabelRushR.Core.Entity;

namespace BabelRushR.Core.EntityAction;

public record EntityActionContext(IEntity? Actor, IReadOnlyCollection<IEntity> Targets)
{
    public static EntityActionContext Empty => field ??= new(null, []);
}