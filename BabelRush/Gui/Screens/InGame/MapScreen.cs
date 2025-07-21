using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;

using BabelRush.Level;
using BabelRush.Level.Scenery;

using Godot;

using KirisameLib.Event;
using KirisameLib.Extensions;

namespace BabelRush.Gui.Screens.InGame;

[EventHandlerContainer]
public partial class MapScreen : Control
{
    // todo: finish this

    // // C# api
    // internal async Task<StageNode> SelectStage(StageNode start, StageNode junction)
    // {
    //     if (Instance._activated) throw new InvalidOperationException("Path select screen has already activated");
    //     Instance._activated = true;
    //
    //     var playNode = Game.Play!.Node;
    //     playNode.AddChild(Instance);
    //
    //     TaskCompletionSource<StageNode> selectTaskSource = new();
    //
    //     Queue<(StageNode? from, StageNode current)> queue = [];
    //     queue.Enqueue((null, start));
    //     while (queue.TryDequeue(out var path))
    //     {
    //         var node = path.current;
    //         node.NextRooms.Select(n => (node, n)).ForEach(queue.Enqueue);
    //
    //         var token = new TextureButton { Disabled = true };
    //         //todo: draw & add it >_<
    //
    //         if (path.from != junction) continue;
    //         token.Pressed += () => selectTaskSource.SetResult(node);
    //     }
    //
    //     // wait for player
    //     var selected = await selectTaskSource.Task;
    //
    //     playNode.RemoveChild(Instance);
    //     Instance._activated = false;
    //     return selected;
    // }


    // GD Override
    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.GameEventBus);
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.GameEventBus);
    }


    // Event handlers
    [EventHandler]
    private void OnSceneReady(SceneReadyEvent e)
    {
        // todo
    }
}