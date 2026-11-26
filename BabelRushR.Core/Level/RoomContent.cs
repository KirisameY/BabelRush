using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;
using BabelRushR.Core.Scenery.Objects;

namespace BabelRushR.Core.Level;

public readonly record struct RoomContent(ISceneObject Content, double Offset);