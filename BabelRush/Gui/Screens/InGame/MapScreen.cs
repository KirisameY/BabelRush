using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;

using BabelRush.Level;
using BabelRush.Level.Scenery;
using BabelRush.Level.Stages;

using Godot;

using KirisameLib.Event;
using KirisameLib.Extensions;

namespace BabelRush.Gui.Screens.InGame;

[EventHandlerContainer]
public partial class MapScreen : Control
{
    // fields
    private Scene? CurrentScene
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (value is null) return;

            SetStage(value.Stage);
        }
    }


    private void SetStage(Stage stage)
    {
        Queue<(StageNode? from, StageNode current)> queue = [];
        queue.Enqueue((null, stage.StartNode));
        while (queue.TryDequeue(out var path))
        {
            var node = path.current;
            node.NextRooms.Select(n => (node, n)).ForEach(queue.Enqueue);

            var token = new TextureButton { Disabled = true }; //OPTIMIZE: make this in pool
            token.TextureNormal = node.Room.Icon;
            AddChild(token);
            token.Position = (node.DisplayPosition * 0.8f + new Vector2(0.1f, 0.1f)) * Size;
            //todo: draw line

            token.Pressed += () => Game.GameEventBus.Publish(new MapScreenChosenNodeEvent(this, node));
        }
    }


    // GD Override
    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.GameEventBus);

        CurrentScene = Game.Play?.Stage.Scene;
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.GameEventBus);
    }


    // Event handlers
    [EventHandler]
    private void OnSceneReady(SceneReadyEvent e)
    {
        CurrentScene = e.Scene;
    }

    [EventHandler]
    private void OnStagePathChosenRequest(StagePathChosenRequestEvent e)
    {
        Show();
    }

    //todo: update available rooms
}