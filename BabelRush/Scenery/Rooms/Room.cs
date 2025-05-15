using System.Collections.Immutable;

namespace BabelRush.Scenery.Rooms;

public sealed class Room(int length, ImmutableArray<SceneObject> objects) // 这个类目前来看其实没用，如果以后还没用就删掉好了
{
    //Properties
    public int Position { get; set; }
    public int Length => length;

    public ImmutableArray<SceneObject> Objects => objects;
}