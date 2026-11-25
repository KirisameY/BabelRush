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
        CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10), level: new TestLevelDefinition(level));

    [Fact]
    public void Attaching_Places_The_Start_Room()
    {
        var roomEntity = new TestEntity(maxHP: 5);
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10, roomEntity), new MapPosition(0.5, 0.9));
        var map = builder.Build();
        var root = Get(map, "root");
        var level = new CommonLevel(map);

        var game = NewGamePlay(level);

        Assert.Same(root, Assert.Single(level.PlacedNodes));
        Assert.Same(root, level.CurrentFork);
        Assert.Same(root, level.CurrentRoom);
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

        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10, rootEntity), new MapPosition(0.5, 0.95));
        var aBuilder = builder.CreateNode(new TestRoomDefinition(Key("a"), 20, aEntity), new MapPosition(0.25, 0.7));
        var a1Builder = builder.CreateNode(new TestRoomDefinition(Key("a1"), 30, a1Entity), new MapPosition(0.25, 0.45));
        var fBuilder = builder.CreateNode(new TestRoomDefinition(Key("f"), 40, fEntity), new MapPosition(0.25, 0.2));
        var gBuilder = builder.CreateNode(new TestRoomDefinition(Key("g"), 50, gEntity), new MapPosition(0.1, 0.05));
        var hBuilder = builder.CreateNode(new TestRoomDefinition(Key("h"), 50, hEntity), new MapPosition(0.4, 0.05));
        var cBuilder = builder.CreateNode(new TestRoomDefinition(Key("c"), 60, cEntity), new MapPosition(0.75, 0.7));
        builder.StartNode.ConnectTo(aBuilder);
        builder.StartNode.ConnectTo(cBuilder);
        aBuilder.ConnectTo(a1Builder);
        a1Builder.ConnectTo(fBuilder);
        fBuilder.ConnectTo(gBuilder);
        fBuilder.ConnectTo(hBuilder);

        var map = builder.Build();
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

        Assert.Equal(new[] { root, a, a1, f }, level.PlacedNodes);
        Assert.Same(f, level.CurrentFork);
        Assert.Same(root, level.CurrentRoom);
        Assert.Empty(level.PassedNodes);
        Assert.DoesNotContain(c, level.PlacedNodes);
        Assert.Contains(g, level.SelectableNodes);
        Assert.Contains(h, level.SelectableNodes);
        Assert.DoesNotContain(c, level.SelectableNodes);
        Assert.Equal(10d, aEntity.Position);
        Assert.Equal(30d, a1Entity.Position);
        Assert.Equal(60d, fEntity.Position);
        Assert.Contains(aEntity, game.Scene.Entities);
        Assert.Contains(fEntity, game.Scene.Entities);
    }

    [Fact]
    public void Selecting_A_Fork_Places_Only_That_Room()
    {
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.CreateNode(new TestRoomDefinition(Key("a"), 20), new MapPosition(0.5, 0.5));
        var bBuilder = builder.CreateNode(new TestRoomDefinition(Key("b"), 30), new MapPosition(0.25, 0.1));
        var cBuilder = builder.CreateNode(new TestRoomDefinition(Key("c"), 30), new MapPosition(0.75, 0.1));
        builder.StartNode.ConnectTo(aBuilder);
        aBuilder.ConnectTo(bBuilder);
        aBuilder.ConnectTo(cBuilder);

        var map = builder.Build();
        var root = Get(map, "root");
        var a = Get(map, "a");
        var b = Get(map, "b");
        var c = Get(map, "c");

        var level = new CommonLevel(map);
        NewGamePlay(level);

        Assert.True(level.TrySelectNext(a));

        Assert.Equal(new[] { root, a }, level.PlacedNodes);
        Assert.Same(a, level.CurrentFork);
        Assert.Same(root, level.CurrentRoom);
        Assert.Empty(level.PassedNodes);
        Assert.Contains(b, level.SelectableNodes);
        Assert.Contains(c, level.SelectableNodes);
    }

    [Fact]
    public void Current_Fork_Advances_While_Current_Room_Stays()
    {
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.CreateNode(new TestRoomDefinition(Key("a"), 20), new MapPosition(0.5, 0.6));
        var fBuilder = builder.CreateNode(new TestRoomDefinition(Key("f"), 30), new MapPosition(0.5, 0.3));
        var bBuilder = builder.CreateNode(new TestRoomDefinition(Key("b"), 40), new MapPosition(0.25, 0.05));
        var cBuilder = builder.CreateNode(new TestRoomDefinition(Key("c"), 40), new MapPosition(0.75, 0.05));
        builder.StartNode.ConnectTo(aBuilder);
        aBuilder.ConnectTo(fBuilder);
        fBuilder.ConnectTo(bBuilder);
        fBuilder.ConnectTo(cBuilder);

        var map = builder.Build();
        var root = Get(map, "root");
        var a = Get(map, "a");
        var f = Get(map, "f");
        var b = Get(map, "b");

        var level = new CommonLevel(map);
        NewGamePlay(level);

        Assert.True(level.TrySelectNext(a));
        Assert.Same(f, level.CurrentFork);
        Assert.Same(root, level.CurrentRoom);
        Assert.Equal(new[] { root, a, f }, level.PlacedNodes);

        Assert.True(level.TrySelectNext(b));
        Assert.Same(b, level.CurrentFork);
        Assert.Same(root, level.CurrentRoom);
        Assert.Equal(new[] { root, a, f, b }, level.PlacedNodes);
    }

    [Fact]
    public void Failed_Selection_Changes_Nothing()
    {
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.CreateNode(new TestRoomDefinition(Key("a"), 20), new MapPosition(0.5, 0.5));
        var bBuilder = builder.CreateNode(new TestRoomDefinition(Key("b"), 30), new MapPosition(0.5, 0.2));
        builder.StartNode.ConnectTo(aBuilder);
        aBuilder.ConnectTo(bBuilder);

        var map = builder.Build();
        var root = Get(map, "root");
        var a = Get(map, "a");
        var b = Get(map, "b");

        var level = new CommonLevel(map);
        var game = NewGamePlay(level);
        var entityCount = game.Scene.Entities.Count;

        Assert.False(level.TrySelectNext(b));
        Assert.False(level.TrySelectNext(root));
        Assert.Same(root, level.CurrentFork);
        Assert.Same(root, level.CurrentRoom);
        Assert.Same(root, Assert.Single(level.PlacedNodes));
        Assert.Empty(level.PassedNodes);
        Assert.Equal(entityCount, game.Scene.Entities.Count);
        Assert.Contains(a, level.SelectableNodes);
    }

    [Fact]
    public void Non_Entity_Content_Throws_Before_Any_State_Changes()
    {
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var badBuilder = builder.CreateNode(new NonEntityRoomDefinition(Key("bad"), 10), new MapPosition(0.5, 0.5));
        builder.StartNode.ConnectTo(badBuilder);

        var map = builder.Build();
        var root = Get(map, "root");
        var bad = Get(map, "bad");

        var level = new CommonLevel(map);
        NewGamePlay(level);

        Assert.Throws<NotSupportedException>(() => level.TrySelectNext(bad));
        Assert.Same(root, level.CurrentFork);
        Assert.Same(root, level.CurrentRoom);
        Assert.Same(root, Assert.Single(level.PlacedNodes));
        Assert.Empty(level.PassedNodes);
    }

    [Fact]
    public void Selecting_From_An_Unattached_Level_Throws()
    {
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.CreateNode(new TestRoomDefinition(Key("a"), 10), new MapPosition(0.5, 0.5));
        builder.StartNode.ConnectTo(aBuilder);

        var map = builder.Build();
        var a = Get(map, "a");
        var level = new CommonLevel(map);

        Assert.Contains(a, level.SelectableNodes);
        Assert.Throws<InvalidOperationException>(() => level.TrySelectNext(a));
    }

    [Fact]
    public void Reassigning_The_Same_Level_Does_Not_Place_It_Again()
    {
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var map = builder.Build();
        var level = new CommonLevel(map);
        var game = NewGamePlay(level);

        var placed = level.PlacedNodes.Count;
        game.SetLevel(new TestLevelDefinition(level));

        Assert.Equal(placed, level.PlacedNodes.Count);
    }

    [Fact]
    public void Create_Hands_Itself_To_The_Level_Definition()
    {
        IGamePlay? seen = null;
        var definition = new TestLevelDefinition(Key("level"), gamePlay =>
        {
            seen = gamePlay;
            return new CommonLevel();
        });

        var game = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10), level: definition);

        Assert.Same(game, seen);
    }

    [Fact]
    public void SetLevel_Generates_From_The_Definition_And_Replaces_The_Level()
    {
        var first = new CommonLevel();
        var second = new CommonLevel();
        var game = CommonGamePlay.Create((ulong)Random.Shared.NextInt64(), new TestEntity(maxHP: 10), level: new TestLevelDefinition(first));
        var changed = new List<string?>();
        game.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        game.SetLevel(new TestLevelDefinition(second));

        Assert.Same(second, game.Level);
        Assert.Contains(nameof(IGamePlay.Level), changed);
    }

    [Fact]
    public void SetLevel_Detaches_The_Replaced_Level()
    {
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.CreateNode(new TestRoomDefinition(Key("a"), 10), new MapPosition(0.5, 0.5));
        builder.StartNode.ConnectTo(aBuilder);

        var map = builder.Build();
        var a = Get(map, "a");
        var old = new CommonLevel(map);
        var game = NewGamePlay(old);

        game.SetLevel(PlaceHolderLevelDefinition.Instance);

        // 換關卡後舊關卡已脫離 GamePlay，再選擇房間會因為未初始化而拋出。
        Assert.Throws<InvalidOperationException>(() => old.TrySelectNext(a));
        Assert.DoesNotContain(a, game.Level.SelectableNodes);
    }

    [Fact]
    public void Selection_Raises_CurrentFork_Notification()
    {
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10), new MapPosition(0.5, 0.9));
        var aBuilder = builder.CreateNode(new TestRoomDefinition(Key("a"), 20), new MapPosition(0.5, 0.5));
        builder.StartNode.ConnectTo(aBuilder);

        var map = builder.Build();
        var a = Get(map, "a");
        var level = new CommonLevel(map);
        NewGamePlay(level);

        var changed = new List<string?>();
        level.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        Assert.True(level.TrySelectNext(a));

        Assert.Contains(nameof(ILevel.CurrentFork), changed);
    }

    [Fact]
    public void Room_Contents_Are_Offset_From_The_Room_Start()
    {
        var first = new TestEntity(maxHP: 5);
        var second = new TestEntity(maxHP: 5);
        var builder = new LevelMapBuilder(new TestRoomDefinition(Key("root"), 10, first, second), new MapPosition(0.5, 0.9));
        var map = builder.Build();
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

        Assert.Single(level.PlacedNodes);
        Assert.Empty(level.PassedNodes);
        Assert.False(level.TrySelectNext(level.CurrentFork));
    }

    private sealed class NonEntityRoomDefinition(RegKey id, double length) : IRoomDefinition
    {
        public RegKey ID => id;

        public double Length => length;

        public IEnumerable<RoomContent> CreateContents()
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
