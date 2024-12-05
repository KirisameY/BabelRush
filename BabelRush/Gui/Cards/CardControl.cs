using Godot;

namespace BabelRush.Gui.Cards;

//todo: 记得调整UI类型可见性为internal
internal partial class CardControl(CardInterface card) : Control
{
    private CardInterface _card = card;

    public override void _Ready()
    {
        AddChild(_card);
        CustomMinimumSize = new(60, 72);
        _card.Position = new(30, 36);
    }

    public override void _ExitTree()
    {
        RemoveChild(_card);
        QueueFree();
    }
}