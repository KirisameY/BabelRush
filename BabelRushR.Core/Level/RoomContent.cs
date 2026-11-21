using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Level;

public readonly record struct RoomContent(ISceneObject Content, double Offset);