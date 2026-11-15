using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Level;

public readonly record struct RoomContent(ISceneObject Content, double Offset);

public readonly record struct RoomContentContext(
    IGamePlay GamePlay,
    LevelNode Node,
    double Start,
    double Length);
