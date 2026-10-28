using BabelRushR.Core.Scenery;

using KirisameY.EventBus;

namespace BabelRushR.Core.GamePlay;

public abstract record GamePlayEvent : BaseEvent;

public sealed record GamePlaySceneReplacedEvent(IScene OldScene, IScene NewScene) : GamePlayEvent;