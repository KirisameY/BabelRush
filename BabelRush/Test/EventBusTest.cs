using System.Threading.Tasks;

using BabelRush.Event;

using Godot;

namespace BabelRush.Test;

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


    class TestEvent1(string msg) : BaseEvent
    {
        public string Msg { get; set; } = msg;
    }

    class TestEvent2(string msg, string msg2) : TestEvent1(msg)
    {
        public string Msg2 { get; set; } = msg2;
    }
}