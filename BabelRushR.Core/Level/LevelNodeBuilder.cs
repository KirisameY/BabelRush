namespace BabelRushR.Core.Level;

public sealed class LevelNodeBuilder
{
    private readonly LevelMapBuilder _owner;
    private readonly List<LevelNodeBuilder> _next = [];

    internal LevelNodeBuilder(LevelMapBuilder owner, IRoomDefinition definition, MapPosition position)
    {
        _owner = owner;
        Definition = definition;
        Position = position;
    }

    public IRoomDefinition Definition { get; }

    public MapPosition Position { get; }

    public IReadOnlyList<LevelNodeBuilder> Next => _next;

    public LevelNodeBuilder ConnectTo(LevelNodeBuilder next)
    {
        _owner.EnsureMutable();

        if (!ReferenceEquals(next._owner, _owner))
            throw new InvalidOperationException("Cannot connect nodes from different map builders.");

        if (_next.Contains(next))
            throw new InvalidOperationException("The edge has already been connected.");

        _next.Add(next);
        return this;
    }
}
