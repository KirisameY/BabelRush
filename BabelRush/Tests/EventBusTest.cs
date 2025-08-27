using System.Threading.Tasks;

using Godot;

using KirisameLib.Asynchronous.SyncTasking;
using KirisameLib.Event;

namespace BabelRush.Tests;

[EventHandlerContainer]
public partial class EventBusTest : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SubscribeInstanceHandler(Game.GameEventBus);

        Task.Delay(1000).ContinueWith(_ =>
        {
            GD.Print(11111);
            Game.GameEventBus.PublishAndWaitFor(new TestEvent2("msg1", "msg22"))
                .ContinueWith(() => GD.Print("done"))
                .Ready();
        });
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    [EventHandler]
    private void EventHandler(BaseEvent e)
    {
        GD.Print("E");
    }

    [EventHandler]
    private void EventHandler0(TestEvent0 e)
    {
        GD.Print("E0", e.Msg);
    }

    [EventHandler]
    private async SyncTask EventHandler1(TestEvent1 e)
    {
        GD.Print("E1", e.Msg);
        await Game.GameEventBus.PublishAndWaitFor(new TestEvent0("msg0"));
        GD.Print("e1-done", e.Msg);
    }

    [EventHandler]
    private void EventHandler2(TestEvent2 e)
    {
        GD.Print("E2", e.Msg, e.Msg2);
    }


    private record TestEvent0(string Msg) : BaseEvent;

    private record TestEvent1(string Msg) : BaseEvent;

    private record TestEvent2(string Msg, string Msg2) : TestEvent1(Msg);
}