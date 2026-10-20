using BabelRushR.Core.Entity;

namespace BabelRushR.Core.EntityAction;

public record EntityActionContext(IEntity? Actor)
{
    public static EntityActionContext Empty => field ??= new((IEntity?)null);
}