using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Scenery;

namespace BabelRushR.Core.Level;

public readonly record struct RoomContent(ISceneObject Content, double Offset);

public readonly record struct RoomContentContext(
    IGamePlay GamePlay,
    LevelNode Node,
    double Start, // todo: Wait，這個東西裏的很多信息都完全不必要吧，甚至其實所有信息都不必要。。。
    double Length
);