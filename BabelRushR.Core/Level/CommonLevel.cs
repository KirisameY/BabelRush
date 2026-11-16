using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Level;

public sealed class CommonLevel(LevelMap map) : ObservableObject, ILevel
{
    public CommonLevel() : this(LevelMap.Empty) { }

    private bool _begun;
    private double _nextStart;

    public IGamePlay? GamePlay { get; private set; }

    public LevelMap Map { get; } = map;

    public LevelNode CurrentFork
    {
        get;
        private set
        {
            if (ReferenceEquals(field, value)) return;
            field = value;
            OnPropertyChanged();
        }
    } = map.StartNode;

    public LevelNode CurrentRoom
    {
        get;
        private set => SetProperty(ref field, value);
    } = map.StartNode;

    private readonly NotifiableList<LevelNode> _placedNodes = [];
    public IReadOnlyNotifiableList<LevelNode> PlacedNodes => field ??= _placedNodes.AsReadOnlyNotifiableList();

    private readonly NotifiableList<LevelNode> _passedNodes = [];
    public IReadOnlyNotifiableList<LevelNode> PassedNodes => field ??= _passedNodes.AsReadOnlyNotifiableList();


    public bool TryBegin() // todo: Begin 機制刪掉，回頭改成Attach到GamePlay時直接收拾好場景
    {
        if (_begun) return false;

        var gamePlay = GamePlay ?? throw new InvalidOperationException($"{nameof(CommonLevel)} uninitialized");
        Place([CurrentRoom], gamePlay);
        _begun = true;
        return true;
    }

    public bool CanSelect(LevelNode node) =>
        Map.Nodes.Contains(node)
     && CurrentFork.NextNodes.Contains(node)
     && !_placedNodes.Contains(node);

    public bool TrySelectNext(LevelNode node)
    {
        var gamePlay = GamePlay ?? throw new InvalidOperationException($"{nameof(CommonLevel)} uninitialized");
        if (!_begun) TryBegin();
        if (!CanSelect(node)) return false;

        var chain = BuildChain(node);
        Place(chain, gamePlay);
        CurrentFork = chain[^1];
        return true;
    }

    private static List<LevelNode> BuildChain(LevelNode node)
    {
        var chain = new List<LevelNode> { node };
        var current = node;
        while (current.NextNodes.Count == 1)
        {
            current = current.NextNodes[0];
            chain.Add(current);
        }
        return chain;
    }

    private void Place(IReadOnlyList<LevelNode> chain, IGamePlay gamePlay)
    {
        var contents = new List<(RoomContent Content, double Start)>();
        var start = _nextStart;

        foreach (var node in chain)
        {
            var length = node.Definition.Length;
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

        foreach (var node in chain) _placedNodes.Add(node);

        for (var i = 0; i < contents.Count; i++)
        {
            var (content, contentStart) = contents[i];
            content.Content.Position    = contentStart + content.Offset;
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