using BabelRushR.Core.Level;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

public class LevelMapBuilderTests
{
    private static IRoomDefinition Room(string key = "room", double length = 10) =>
        new TestRoomDefinition(RegKey.From("test", key), length);

    private static LevelNode Node(LevelMap map, string key) =>
        map.Nodes.Single(node => node.Definition.ID == RegKey.From("test", key));

    [Fact]
    public void CreateNode_Does_Not_Connect_The_Node()
    {
        var start = Room("start");
        var builder = new LevelMapBuilder(start, new MapPosition(0.5, 0.5));
        var orphan = builder.CreateNode(Room("orphan"), new MapPosition(0.5, 0.9));

        var map = builder.Build();

        Assert.Empty(orphan.FollowingNodes);
        Assert.Same(start, map.StartNode.Definition);
        Assert.Single(map.Nodes);
    }

    [Fact]
    public void Build_Returns_A_Map_Starting_At_The_Builder_Start_Node()
    {
        var definition = Room("start");
        var builder = new LevelMapBuilder(definition, new MapPosition(0.1, 0.2));

        var map = builder.Build();

        Assert.Same(definition, map.StartNode.Definition);
        Assert.Equal(new MapPosition(0.1, 0.2), map.StartNode.Position);
        Assert.Same(map.StartNode, Assert.Single(map.Nodes));
    }

    [Fact]
    public void ConnectTo_Rejects_A_Cycle()
    {
        var builder = new LevelMapBuilder(Room("a"), new MapPosition(0.1, 0.1));
        var b = builder.CreateNode(Room("b"), new MapPosition(0.5, 0.5));
        builder.StartNode.ConnectTo(b);

        Assert.Throws<InvalidOperationException>(() => b.ConnectTo(builder.StartNode));
    }

    [Fact]
    public void ConnectTo_Rejects_A_Self_Loop()
    {
        var builder = new LevelMapBuilder(Room("a"), new MapPosition(0.5, 0.5));

        Assert.Throws<InvalidOperationException>(() => builder.StartNode.ConnectTo(builder.StartNode));
    }

    [Fact]
    public void Connecting_The_Same_Edge_Twice_Keeps_One_Edge()
    {
        var builder = new LevelMapBuilder(Room("a"), new MapPosition(0.1, 0.1));
        var b = builder.CreateNode(Room("b"), new MapPosition(0.5, 0.5));
        builder.StartNode.ConnectTo(b);
        builder.StartNode.ConnectTo(b);

        var map = builder.Build();

        Assert.Equal(RegKey.From("test", "b"), Assert.Single(map.StartNode.NextNodes).Definition.ID);
    }

    [Fact]
    public void Build_Ignores_Nodes_That_Are_Not_Reachable_From_The_Start()
    {
        var builder = new LevelMapBuilder(Room("start"), new MapPosition(0.5, 0.1));
        builder.CreateNode(Room("orphan"), new MapPosition(0.5, 0.9));

        var map = builder.Build();

        Assert.Single(map.Nodes);
        Assert.Equal(RegKey.From("test", "start"), map.StartNode.Definition.ID);
    }

    [Fact]
    public void Build_Reuses_Shared_Successors()
    {
        var builder = new LevelMapBuilder(Room("root"), new MapPosition(0.5, 0.1));
        var left = builder.CreateNode(Room("left"), new MapPosition(0.25, 0.5));
        var right = builder.CreateNode(Room("right"), new MapPosition(0.75, 0.5));
        var end = builder.CreateNode(Room("end"), new MapPosition(0.5, 0.9));
        builder.StartNode.ConnectTo(left);
        builder.StartNode.ConnectTo(right);
        left.ConnectTo(end);
        right.ConnectTo(end);

        var map = builder.Build();

        var endNode = Node(map, "end");
        var leftNode = Node(map, "left");
        var rightNode = Node(map, "right");

        Assert.Same(endNode, Assert.Single(leftNode.NextNodes));
        Assert.Same(endNode, Assert.Single(rightNode.NextNodes));
    }

    [Fact]
    public void Build_Accepts_A_Terminal_Node()
    {
        var builder = new LevelMapBuilder(Room("start"), new MapPosition(0.5, 0.5));

        var map = builder.Build();

        Assert.Same(map.StartNode, Assert.Single(map.Nodes));
        Assert.Empty(map.StartNode.NextNodes);
    }

    [Fact]
    public void A_Built_Map_Does_Not_See_Edges_Connected_Afterwards()
    {
        var builder = new LevelMapBuilder(Room("start"), new MapPosition(0.5, 0.1));
        var a = builder.CreateNode(Room("a"), new MapPosition(0.5, 0.5));
        var b = builder.CreateNode(Room("b"), new MapPosition(0.5, 0.9));
        builder.StartNode.ConnectTo(a);

        var map = builder.Build();
        builder.StartNode.ConnectTo(b);

        Assert.Equal(RegKey.From("test", "a"), Assert.Single(map.StartNode.NextNodes).Definition.ID);
        Assert.Equal(2, builder.Build().StartNode.NextNodes.Count);
    }
}
