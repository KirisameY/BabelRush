using BabelRushR.Core.Level;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

public class LevelMapBuilderTests
{
    private static IRoomDefinition Room(string key = "room", double length = 10) =>
        new TestRoomDefinition(RegKey.From("test", key), length);

    [Theory]
    [InlineData(-0.01, 0.5)]
    [InlineData(1.01, 0.5)]
    [InlineData(0.5, -0.01)]
    [InlineData(0.5, 1.01)]
    public void Add_Rejects_Positions_Outside_The_Unit_Square(double x, double y)
    {
        var builder = new LevelMapBuilder();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Add(Room(), new MapPosition(x, y)));
    }

    [Fact]
    public void Add_Rejects_Non_Finite_Positions()
    {
        var builder = new LevelMapBuilder();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Add(Room(), new MapPosition(double.NaN, 0.5)));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Add(Room(), new MapPosition(0.5, double.PositiveInfinity)));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Add_Rejects_Invalid_Room_Lengths(double length)
    {
        var builder = new LevelMapBuilder();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Add(Room(length: length), new MapPosition(0.5, 0.5)));
    }

    [Fact]
    public void Connect_Rejects_Nodes_From_Another_Builder()
    {
        var first = new LevelMapBuilder();
        var second = new LevelMapBuilder();
        var a = first.Add(Room("a"), new MapPosition(0.1, 0.1));
        var b = second.Add(Room("b"), new MapPosition(0.9, 0.9));

        Assert.Throws<InvalidOperationException>(() => a.ConnectTo(b));
    }

    [Fact]
    public void Connect_Rejects_Duplicate_Edges()
    {
        var builder = new LevelMapBuilder();
        var a = builder.Add(Room("a"), new MapPosition(0.1, 0.1));
        var b = builder.Add(Room("b"), new MapPosition(0.9, 0.9));
        a.ConnectTo(b);

        Assert.Throws<InvalidOperationException>(() => a.ConnectTo(b));
    }

    [Fact]
    public void Build_Rejects_Unreachable_Nodes()
    {
        var builder = new LevelMapBuilder();
        var start = builder.Add(Room("start"), new MapPosition(0.5, 0.1));
        builder.Add(Room("orphan"), new MapPosition(0.5, 0.9));

        Assert.Throws<InvalidOperationException>(() => builder.Build(start));
    }

    [Fact]
    public void Build_Rejects_Cycles()
    {
        var builder = new LevelMapBuilder();
        var a = builder.Add(Room("a"), new MapPosition(0.1, 0.1));
        var b = builder.Add(Room("b"), new MapPosition(0.9, 0.9));
        a.ConnectTo(b);
        b.ConnectTo(a);

        Assert.Throws<InvalidOperationException>(() => builder.Build(a));
    }

    [Fact]
    public void Build_Forbids_Further_Mutation()
    {
        var builder = new LevelMapBuilder();
        var a = builder.Add(Room("a"), new MapPosition(0.1, 0.1));
        var b = builder.Add(Room("b"), new MapPosition(0.9, 0.9));
        a.ConnectTo(b);
        builder.Build(a);

        Assert.Throws<InvalidOperationException>(() => builder.Add(Room("late"), new MapPosition(0.5, 0.5)));
        Assert.Throws<InvalidOperationException>(() => a.ConnectTo(b));
    }

    [Fact]
    public void Build_Reuses_Shared_Successors()
    {
        var builder = new LevelMapBuilder();
        var root = builder.Add(Room("root"), new MapPosition(0.5, 0.1));
        var left = builder.Add(Room("left"), new MapPosition(0.25, 0.5));
        var right = builder.Add(Room("right"), new MapPosition(0.75, 0.5));
        var end = builder.Add(Room("end"), new MapPosition(0.5, 0.9));
        root.ConnectTo(left).ConnectTo(right);
        left.ConnectTo(end);
        right.ConnectTo(end);

        var map = builder.Build(root);

        var endNode = map.Nodes.Single(node => node.Definition.ID == RegKey.From("test", "end"));
        var leftNode = map.Nodes.Single(node => node.Definition.ID == RegKey.From("test", "left"));
        var rightNode = map.Nodes.Single(node => node.Definition.ID == RegKey.From("test", "right"));

        Assert.Same(endNode, Assert.Single(leftNode.NextNodes));
        Assert.Same(endNode, Assert.Single(rightNode.NextNodes));
    }

    [Fact]
    public void Build_Accepts_A_Terminal_Node()
    {
        var builder = new LevelMapBuilder();
        var start = builder.Add(Room("start"), new MapPosition(0.5, 0.5));

        var map = builder.Build(start);

        Assert.Same(map.StartNode, Assert.Single(map.Nodes));
        Assert.Empty(map.StartNode.NextNodes);
    }
}
