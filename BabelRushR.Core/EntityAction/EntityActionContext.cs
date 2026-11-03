using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.EntityAction;

public record EntityActionContext(IGamePlay GamePlay, IEntity Actor) { }