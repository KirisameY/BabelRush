using System.ComponentModel;

using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;
using BabelRushR.Core.Level;
using BabelRushR.Core.Scenery;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

public class LevelTests
{
    private static RegKey Key(string key) => RegKey.From("test", key);

    private static LevelNode Get(LevelMap map, string key) =>
        map.Nodes.Single(node => node.Definition.ID == Key(key));

    private static CommonGamePlay NewGamePlay(ILevel level) =>
        CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10), level: level);

    [Fact]
    public void Begin_Places_The_Start_Room()
    {
        var roomEntity = new TestEntity(maxHP: 5);
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10, roomEntity), new MapPosition(0.5, 0.9));
        var map = builder.Build(rootBuilder);
        var root = Get(map, "root");
        var level = new CommonLevel(map);

        var game = NewGamePlay(level);

        var placement = Assert.Single(level.PlacedRooms);
        Assert.Same(root, placement.Node);
        Assert.Equal(0d, placement.Start);
        Assert.Equal(10d, placement.End);
        Assert.Same(root, level.CurrentNode);
        Assert.Empty(level.PassedNodes);
        Assert.Contains(roomEntity, game.Scene.Entities);
        Assert.Equal(0d, roomEntity.Position);
    }

    [Fact]
    public void Selecting_A_Chain_Places_Rooms_Until_The_Next_Fork()
    {
        var rootEntity = new TestEntity(maxHP: 5);
        var aEntity = new TestEntity(maxHP: 5);
        var a1Entity = new TestEntity(maxHP: 5);
        var fEntity = new TestEntity(maxHP: 5);
        var gEntity = new TestEntity(maxHP: 5);
        var hEntity = new TestEntity(maxHP: 5);
        var cEntity = new TestEntity(maxHP: 5);

        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10, rootEntity), new MapPosition(0.5, 0.95));
        var aBuilder = builder.Add(new TestRoomDefinition(Key("a"), 20, aEntity), new MapPosition(0.25, 0.7));
        var a1Builder = builder.Add(new TestRoomDefinition(Key("a1"), 30, a1Entity), new MapPosition(0.25, 0.45));
        var fBuilder = builder.Add(new TestRoomDefinition(Key("f"), 40, fEntity), new MapPosition(0.25, 0.2));
        var gBuilder = builder.Add(new TestRoomDefinition(Key("g"), 50, gEntity), new MapPosition(0.1, 0.05));
        var hBuilder = builder.Add(new TestRoomDefinition(Key("h"), 50, hEntity), new MapPosition(0.4, 0.05));
        var cBuilder = builder.Add(new TestRoomDefinition(Key("c"), 60, cEntity), new MapPosition(0.75, 0.7));
        rootBuilder.ConnectTo(aBuilder).ConnectTo(cBuilder);
        aBuilder.ConnectTo(a1Builder);
        a1Builder.ConnectTo(fBuilder);
        fBuilder.ConnectTo(gBuilder).ConnectTo(hBuilder);

        var map = builder.Build(rootBuilder);
        var root = Get(map, "root");
        var a = Get(map, "a");
        var a1 = Get(map, "a1");
        var f = Get(map, "f");
        var g = Get(map, "g");
        var h = Get(map, "h");
        var c = Get(map, "c");

        var level = new CommonLevel(map);
        var game = NewGamePlay(level);

        Assert.True(level.TrySelectNext(a));

        Assert.Equal(new[] { root, a, a1, f }, level.PlacedRooms.Select(placement => placement.Node));
        Assert.Equal(new[] { 0d, 10d, 30d, 60d }, level.PlacedRooms.Select(placement => placement.Start));
        Assert.Same(f, level.CurrentNode);
        Assert.Equal(new[] { root, a, a1 }, level.PassedNodes);
        Assert.DoesNotContain(c, level.PlacedRooms.Select(placement => placement.Node));
        Assert.True(level.CanSelect(g));
        Assert.True(level.CanSelect(h));
        Assert.False(level.CanSelect(c));
        Assert.Contains(aEntity, game.Scene.Entities);
        Assert.Equal(10d, aEntity.Position);
        Assert.Contains(fEntity, game.Scene.Entities);
        Assert.Equal(60d, fEntity.Position);
    }

    [Fact]
    public void Selecting_A_Fork_Places_Only_That_Room()
    {
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.Add(new TestRoomDefinition(Key("a"), 20), new MapPosition(0.5, 0.5));
        var bBuilder = builder.Add(new TestRoomDefinition(Key("b"), 30), new MapPosition(0.25, 0.1));
        var cBuilder = builder.Add(new TestRoomDefinition(Key("c"), 30), new MapPosition(0.75, 0.1));
        rootBuilder.ConnectTo(aBuilder);
        aBuilder.ConnectTo(bBuilder).ConnectTo(cBuilder);

        var map = builder.Build(rootBuilder);
        var root = Get(map, "root");
        var a = Get(map, "a");
        var b = Get(map, "b");
        var c = Get(map, "c");

        var level = new CommonLevel(map);
        NewGamePlay(level);

        Assert.True(level.TrySelectNext(a));

        Assert.Equal(new[] { root, a }, level.PlacedRooms.Select(placement => placement.Node));
        Assert.Same(a, level.CurrentNode);
        Assert.Equal(new[] { root }, level.PassedNodes);
        Assert.True(level.CanSelect(b));
        Assert.True(level.CanSelect(c));
    }

    [Fact]
    public void Passed_Nodes_Keep_Placement_Order_Across_Selections()
    {
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.Add(new TestRoomDefinition(Key("a"), 20), new MapPosition(0.5, 0.6));
        var fBuilder = builder.Add(new TestRoomDefinition(Key("f"), 30), new MapPosition(0.5, 0.3));
        var bBuilder = builder.Add(new TestRoomDefinition(Key("b"), 40), new MapPosition(0.25, 0.05));
        var cBuilder = builder.Add(new TestRoomDefinition(Key("c"), 40), new MapPosition(0.75, 0.05));
        rootBuilder.ConnectTo(aBuilder);
        aBuilder.ConnectTo(fBuilder);
        fBuilder.ConnectTo(bBuilder).ConnectTo(cBuilder);

        var map = builder.Build(rootBuilder);
        var root = Get(map, "root");
        var a = Get(map, "a");
        var f = Get(map, "f");
        var b = Get(map, "b");

        var level = new CommonLevel(map);
        NewGamePlay(level);

        level.TrySelectNext(a);
        Assert.Equal(new[] { root, a }, level.PassedNodes);

        level.TrySelectNext(b);
        Assert.Equal(new[] { root, a, f }, level.PassedNodes);
        Assert.Equal(new[] { 0d, 10d, 30d, 60d }, level.PlacedRooms.Select(placement => placement.Start));
    }

    [Fact]
    public void Failed_Selection_Changes_Nothing()
    {
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.Add(new TestRoomDefinition(Key("a"), 20), new MapPosition(0.5, 0.5));
        var bBuilder = builder.Add(new TestRoomDefinition(Key("b"), 30), new MapPosition(0.5, 0.2));
        rootBuilder.ConnectTo(aBuilder);
        aBuilder.ConnectTo(bBuilder);

        var map = builder.Build(rootBuilder);
        var root = Get(map, "root");
        var a = Get(map, "a");
        var b = Get(map, "b");

        var level = new CommonLevel(map);
        var game = NewGamePlay(level);
        var entityCount = game.Scene.Entities.Count;

        Assert.False(level.TrySelectNext(b));
        Assert.False(level.TrySelectNext(root));
        Assert.Same(root, level.CurrentNode);
        Assert.Single(level.PlacedRooms);
        Assert.Empty(level.PassedNodes);
        Assert.Equal(entityCount, game.Scene.Entities.Count);
        Assert.True(level.CanSelect(a));
    }

    [Fact]
    public void Non_Entity_Content_Throws_Before_Any_State_Changes()
    {
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var badBuilder = builder.Add(new NonEntityRoomDefinition(Key("bad"), 10), new MapPosition(0.5, 0.5));
        rootBuilder.ConnectTo(badBuilder);

        var map = builder.Build(rootBuilder);
        var root = Get(map, "root");
        var bad = Get(map, "bad");

        var level = new CommonLevel(map);
        NewGamePlay(level);

        Assert.Throws<NotSupportedException>(() => level.TrySelectNext(bad));
        Assert.Same(root, level.CurrentNode);
        Assert.Single(level.PlacedRooms);
        Assert.Empty(level.PassedNodes);
    }

    [Fact]
    public void Selecting_From_An_Unattached_Level_Throws()
    {
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.Add(new TestRoomDefinition(Key("a"), 10), new MapPosition(0.5, 0.5));
        rootBuilder.ConnectTo(aBuilder);

        var map = builder.Build(rootBuilder);
        var a = Get(map, "a");
        var level = new CommonLevel(map);

        Assert.True(level.CanSelect(a));
        Assert.Throws<InvalidOperationException>(() => level.TryBegin());
        Assert.Throws<InvalidOperationException>(() => level.TrySelectNext(a));
    }

    [Fact]
    public void Begin_Is_Idempotent()
    {
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var map = builder.Build(rootBuilder);
        var level = new CommonLevel(map);
        NewGamePlay(level);

        var placed = level.PlacedRooms.Count;
        Assert.False(level.TryBegin());
        Assert.Equal(placed, level.PlacedRooms.Count);
    }

    [Fact]
    public void Selection_Raises_CurrentNode_And_PassedNodes_Notifications()
    {
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.Add(new TestRoomDefinition(Key("a"), 20), new MapPosition(0.5, 0.5));
        rootBuilder.ConnectTo(aBuilder);

        var map = builder.Build(rootBuilder);
        var a = Get(map, "a");
        var level = new CommonLevel(map);
        NewGamePlay(level);

        var changed = new List<string?>();
        level.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        Assert.True(level.TrySelectNext(a));

        Assert.Contains(nameof(ILevel.CurrentNode), changed);
        Assert.Contains(nameof(ILevel.PassedNodes), changed);
    }

    [Fact]
    public void Room_Contents_Are_Offset_From_The_Room_Start()
    {
        var first = new TestEntity(maxHP: 5);
        var second = new TestEntity(maxHP: 5);
        var builder = new LevelMapBuilder();
        var rootBuilder = builder.Add(new TestRoomDefinition(Key("root"), 10, first, second), new MapPosition(0.5, 0.9));
        var map = builder.Build(rootBuilder);
        var level = new CommonLevel(map);

        var game = NewGamePlay(level);

        Assert.Equal(0d, first.Position);
        Assert.Equal(1d, second.Position);
        Assert.Contains(first, game.Scene.Entities);
        Assert.Contains(second, game.Scene.Entities);
    }

    [Fact]
    public void The_Default_Level_Is_Inert()
    {
        var game = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10));
        var level = game.Level;

        Assert.Single(level.PlacedRooms);
        Assert.Empty(level.PassedNodes);
        Assert.False(level.TrySelectNext(level.CurrentNode));
    }

    private sealed class NonEntityRoomDefinition(RegKey id, double length) : IRoomDefinition
    {
        public RegKey ID => id;

        public double Length => length;

        public IEnumerable<RoomContent> CreateContents(RoomContentContext context)
        {
            yield return new RoomContent(new TestSceneObject(), 0);
        }
    }

    private sealed class TestSceneObject : ISceneObject
    {
        public double Position { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { }
            remove { }
        }
    }
}
