using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.Card;

public abstract record CardEvent(ICard Card) : GamePlayEvent;