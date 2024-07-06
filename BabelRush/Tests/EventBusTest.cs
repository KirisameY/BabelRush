using System.Threading.Tasks;

using Godot;

using KirisameLib.Events;

namespace BabelRush.Tests;

public partial class EventBusTest : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        EventBus.Register<BaseEvent>(EventHandler);
        EventBus.Register<TestEvent1>(EventHandler1);
        EventBus.Register<TestEvent2>(EventHandler2);

        Task.Delay(1000).ContinueWith(_ =>
        {
            EventBus.Publish(new TestEvent2("msg1", "msg22"));
        });
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    private void EventHandler(BaseEvent e)
    {
        GD.Print("E0");
    }

    private void EventHandler1(TestEvent1 e)
    {
        GD.Print("E1", e.Msg);
    }

    private void EventHandler2(TestEvent2 e)
    {
        GD.Print("E2", e.Msg, e.Msg2);
    }


    private record TestEvent1(string Msg) : BaseEvent;

    private record TestEvent2(string Msg, string Msg2) : TestEvent1(Msg);
}