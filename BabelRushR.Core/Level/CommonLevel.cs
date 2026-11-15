using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Level;

public sealed class CommonLevel : ObservableObject, ILevel
{
    private readonly NotifiableList<RoomPlacement> _placedRooms = [];
    private bool _begun;
    private double _nextStart;
    private IReadOnlyList<LevelNode>? _passedNodes;

    public CommonLevel() : this(LevelMap.Empty) { }

    public CommonLevel(LevelMap map)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
        CurrentNode = map.StartNode;
    }

    public IGamePlay? GamePlay { get; private set; }

    public LevelMap Map { get; }

    public LevelNode CurrentNode
    {
        get;
        private set
        {
            if (ReferenceEquals(field, value)) return;
            field = value;
            _passedNodes = null;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PassedNodes));
        }
    }

    public IReadOnlyNotifiableList<RoomPlacement> PlacedRooms => field ??= _placedRooms.AsReadOnlyNotifiableList();

    public IReadOnlyList<LevelNode> PassedNodes =>
        _passedNodes ??= [.. _placedRooms.Where(placement => !ReferenceEquals(placement.Node, CurrentNode))
                                         .Select(placement => placement.Node)];

    public bool TryBegin()
    {
        if (_begun) return false;

        var gamePlay = GamePlay ?? throw new InvalidOperationException($"{nameof(CommonLevel)} uninitialized");
        Place([CurrentNode], gamePlay);
        _begun = true;
        return true;
    }

    public bool CanSelect(LevelNode node) =>
        node is not null
        && Map.Nodes.Contains(node)
        && CurrentNode.NextNodes.Contains(node)
        && !_placedRooms.Any(placement => ReferenceEquals(placement.Node, node));

    public bool TrySelectNext(LevelNode node)
    {
        var gamePlay = GamePlay ?? throw new InvalidOperationException($"{nameof(CommonLevel)} uninitialized");
        if (!_begun) TryBegin();
        if (!CanSelect(node)) return false;

        var chain = BuildChain(node);
        Place(chain, gamePlay);
        CurrentNode = chain[^1];
        return true;
    }

    private static List<LevelNode> BuildChain(LevelNode node)
    {
        var chain = new List<LevelNode> { node };
        var current = node;
        while (current.NextNodes.Length == 1)
        {
            current = current.NextNodes[0];
            chain.Add(current);
        }
        return chain;
    }

    private void Place(IReadOnlyList<LevelNode> chain, IGamePlay gamePlay)
    {
        var placements = new List<RoomPlacement>(chain.Count);
        var contents = new List<(RoomContent Content, double Start)>();
        var start = _nextStart;

        foreach (var node in chain)
        {
            var length = node.Definition.Length;
            placements.Add(new RoomPlacement(node, start));
            foreach (var content in node.Definition.CreateContents(new RoomContentContext(gamePlay, node, start, length)))
            {
                contents.Add((content, start));
            }
            start += length;
        }

        // 先校验全部内容再改动状态，避免生成到一半失败时留下半个房间。
        var entities = new List<IEntity>(contents.Count);
        var seen = new HashSet<IEntity>();
        foreach (var (content, _) in contents)
        {
            if (content.Content is not IEntity entity)
                throw new NotSupportedException("Scene does not support non-entity room content yet.");

            if (!seen.Add(entity) || gamePlay.Scene.Entities.Contains(entity))
                throw new InvalidOperationException("Room content is already present in the scene.");

            entities.Add(entity);
        }

        foreach (var placement in placements) _placedRooms.Add(placement);

        for (var i = 0; i < contents.Count; i++)
        {
            var (content, contentStart) = contents[i];
            content.Content.Position = contentStart + content.Offset;
            if (!gamePlay.Scene.AddEntity(entities[i]))
                throw new InvalidOperationException("Room content is already present in the scene.");
        }

        _nextStart = start;
    }

    internal void Attach(IGamePlay gamePlay) => GamePlay = gamePlay;
    void ILevel.Attach(IGamePlay gamePlay) => Attach(gamePlay);
    internal void Unattach() => GamePlay = null;
    void ILevel.Unattach() => Unattach();
}
