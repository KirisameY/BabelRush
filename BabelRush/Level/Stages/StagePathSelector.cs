using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using BabelRush.Gui.Screens.InGame;

using KirisameLib.Event;

namespace BabelRush.Level.Stages;

[EventHandlerContainer]
internal sealed partial class StagePathSelector
{
    // Init & cleanup
    public StagePathSelector(Stage stage)
    {
        Stage = stage;

        SubscribeInstanceHandler(Game.GameEventBus);
    }

    private bool _disposed = false;

    internal void InternalDispose()
    {
        if (_disposed) return;
        _disposed = true;

        UnsubscribeInstanceHandler(Game.GameEventBus);
        _getNextNodeTaskSource?.TrySetCanceled();
    }


    public Stage Stage { get; }

    public ImmutableArray<StageNode> SelectableNodes
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            _nextNode = null;
            _getNextNodeTaskSource?.TrySetCanceled();
            _getNextNodeTaskSource = null;
        }
    } = [];

    private StageNode? _nextNode;
    private TaskCompletionSource<StageNode>? _getNextNodeTaskSource = new();


    public Task<StageNode> GetNextNode()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_nextNode != null) return Task.FromResult(_nextNode);

        _getNextNodeTaskSource ??= new();
        return _getNextNodeTaskSource.Task;
    }


    [EventHandler]
    private void OnMapScreenChosenNode(MapScreenChosenNodeEvent e)
    {
        if (!SelectableNodes.Contains(e.Node)) return;

        _nextNode = e.Node;
        _getNextNodeTaskSource?.TrySetResult(e.Node);
    }
}