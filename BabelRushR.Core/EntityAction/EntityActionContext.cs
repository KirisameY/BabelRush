using BabelRushR.Core.Card;
using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.EntityAction;

public abstract record EntityActionContext(IGamePlay GamePlay, IEntity Actor) { }

public record CardEntityActionContext(IGamePlay GamePlay, IEntity Actor, ICard Card) : EntityActionContext(GamePlay, Actor) { }